using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DAMBuddy2
{
    class ThreadArgs
    {
        public string ThreadID;
        public string RepoPath;
        public Thread ThreadInstance;
        public bool ExceptionEncountered;
        public ContextCallback callbackError;
        public ContextCallback callbackComplete;
    }
    
    class RepoCacheState
    {
        public string Path;
        public bool CloneCompleted;
        public DateTime LastPullDate;
    }

   /* class RepoList
    {
        public RepoList() { CacheList = new List<RepoCacheState>(); }
        public List<RepoCacheState> CacheList { get; set; }
        public int count { get; set; }
    }*/

    class RepoCacheManager
    {
        // manages a set of folders which contain git repositories cloned from the ckmmirror repository
        //
        // the cache of repositories is consumed when tickets are linked to jira
        // using a pre-prepared git repository avoids the user having to wait whilst a git clone is performed.
        // instead, the git clone is performed on a background thread

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static string mGitBinariesPath = "";                               // path the folder containing git binaries

        private static string REPOCACHE_FOLDER = @"\repocache";             // the name of the folder under which the cache folders are stored.
        private static string WORKING_FOLDER = @"\repotemp\";                     // will contain the repos which didn't get completely cloned (e.g. if the app was closed before the clone() finished).


        private string STATE_FILEPATH = @"\repoliststate.json";             // the file holding the state of the cache for persistence

        private string mRepoCacheStateFilepath = "";
        private string mRepoWorkingFilepath;
        private string mRemoteGitRepository = "";                           // the URL for the git repo to be cloned (the CKMMirror repository)
        private ModifiedCallback mCallbackInfo;
        public string mRootFolder = "";                                     // the top level directory path
        private int mCacheSize = -1;                                        // how many cache folders should be maintained 
        
        private List<RepoCacheState> mRepoCacheList = null;                 // state of all the cached repos (serialized to mRepoCacheStateFilepath)

        private List<ThreadArgs> mThreads = null;                           // list of all created threads
        private bool mIsSClosingDown = false;

        public int AvailableCaches { get => mRepoCacheList.Where(x => x.CloneCompleted).Count(); }
        
        public RepoCacheManager( string rootfolder, int cachesize, string gitRemoteRepoURL, string gitBinariesPath, ModifiedCallback callbackInfoUpdate )
        {

            Logger.Info("RepoCacheManager() : Hello world");

            mRootFolder = rootfolder;
            mCacheSize = cachesize;
            mGitBinariesPath = gitBinariesPath;
            mRemoteGitRepository = gitRemoteRepoURL;
            mCallbackInfo = callbackInfoUpdate;

            mRepoCacheStateFilepath = mRootFolder + REPOCACHE_FOLDER + STATE_FILEPATH;
            mRepoWorkingFilepath = mRootFolder + WORKING_FOLDER;

            if (!Directory.Exists(mRepoWorkingFilepath)) Directory.CreateDirectory(mRepoWorkingFilepath);

            if (!Directory.Exists(mRootFolder + REPOCACHE_FOLDER)) Directory.CreateDirectory(mRootFolder + REPOCACHE_FOLDER);


            mThreads = new List<ThreadArgs>();

            LoadCacheState();
            VerifyCacheState();

            callbackInfoUpdate?.Invoke($"Available Caches: {AvailableCaches}/{cachesize}");


            ManageCaches();
            
            
            DeleteTrash();
        }

        private void DeleteTrash()
        {
            string foldername;
            var trashfolders = Directory.EnumerateDirectories(mRepoWorkingFilepath);
            foreach (var folder in trashfolders)
            {
                try
                {
                    Directory.Delete(folder, true);
                    /*
                                    }

                                    foldername = folder.Substring(folder.LastIndexOf("\\") + 1);
                                    if (mRepoCacheList.Where( cache => cache.Path.Equals( folder) && cache.CloneCompleted).Count() == 0 ) {
                                        // remove folder
                                        Directory.Move(folder, mRepoWorkingFilepath + "\\" + foldername);
                                    }*/
                }
                catch { }
            }
        }

        ~RepoCacheManager()
        {
//            SaveCacheState();
//            StopThreads();
        }

        public void CloseDown()
        {
            mIsSClosingDown = true;
            StopThreads();

            SaveCacheState();

        }


        private void StopThreads()
        {
            // terminates/destroys any threads currently active.

            foreach( ThreadArgs thread in mThreads )
            {

                thread.ThreadInstance.Abort();
            }
        }

        private void ThreadComplete( object args )
        {
            ThreadArgs threadargs = (ThreadArgs)args;

            // TODO: remove from threadlist
            //string threadid = (string)id;

            //threadaargs.ThreadID

            mThreads.RemoveAll(item => item.ThreadID == threadargs.ThreadID);

            for( int i = 0; i < mRepoCacheList.Count; i++)
            {
                if (mRepoCacheList[i].Path == threadargs.RepoPath )
                {

                    string cachepath = "";
                    if( MoveWorkingToCache(threadargs.RepoPath, ref cachepath) )
                    {
                        mRepoCacheList[i].Path = cachepath;
                        mRepoCacheList[i].CloneCompleted = true;
                        mCallbackInfo?.Invoke($"Cache Clone Completed. Available Caches: {AvailableCaches}/{mCacheSize}");
                        SaveCacheState();
                    }

                    break;
                }
            }

            ManageCaches();
            

        }

        private bool MoveWorkingToCache( string workingpath, ref string cachepath)
        {
            cachepath = workingpath.Replace(WORKING_FOLDER, REPOCACHE_FOLDER + "\\");

            Directory.Move(workingpath, cachepath);

            return true;
        }

        private void ThreadException( object e )
        {
            Exception theException = (Exception)e;
            // TODO: process exception.
        }

        private string CreateNewCache( string name )
        {

            string cachefolder = mRepoWorkingFilepath + name;
            if ( Directory.Exists( cachefolder))
            {
                Directory.Delete(cachefolder);

            }

            var info = Directory.CreateDirectory(cachefolder);
            Console.WriteLine(info.FullName);

            var args = new ThreadArgs();
            args.ThreadID = $"{cachefolder}-clone";
            args.RepoPath = cachefolder;
            args.ThreadInstance = new Thread(Clone);
            args.callbackError = ThreadException;
            args.callbackComplete = ThreadComplete;

            args.ThreadInstance.Start(args);

            mCallbackInfo?.Invoke($"Preparing new cache. Available caches = {AvailableCaches}/{mCacheSize}");

            mThreads.Add(args);

            return cachefolder;
        }

        public static void Pull2( string repofolder )
        {
            
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {

                        FileName = mGitBinariesPath + "git.exe",
                        WorkingDirectory = repofolder,
                        Arguments = "pull",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };


                process.OutputDataReceived += new DataReceivedEventHandler((s, eData) =>
                {
                    Console.WriteLine(eData.Data);

                    //AddSearchResult(eData.Data);

                });

                process.ErrorDataReceived += new DataReceivedEventHandler((s, eData) =>
                {
                    Console.WriteLine(eData.Data);
                });

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }


        public void Clone( object args ) //(string repopath)
        {
            //m_dtCloneStart = DateTime.Now;
            ThreadArgs threadargs = (ThreadArgs)args;
            CloneOptions options = new CloneOptions();
            options.OnTransferProgress = GitProgress;

            try
            {
                if (!File.Exists(threadargs.RepoPath))
                {
                    var info =  Directory.CreateDirectory(threadargs.RepoPath);
                    Console.WriteLine($"Created this directory for Clone(): {info.FullName}");
                }

                Repository.Clone(mRemoteGitRepository, threadargs.RepoPath, options);

            } catch ( Exception e )
            {
                if (!mIsSClosingDown)
                {
                    threadargs.callbackError?.Invoke(e);

                }
                return;
            }
            if (!mIsSClosingDown)
                threadargs.callbackComplete?.Invoke(args);

            //m_dtCloneEnd = DateTime.Now;

        }


        public static bool GitProgress(TransferProgress progress)
        {
            //Console.WriteLine($"Objects: {progress.ReceivedObjects} of {progress.TotalObjects}");
            return true;
        }

        private void LoadCacheState()
        {
            mRepoCacheList = new List<RepoCacheState>();

            
            if( File.Exists(mRepoCacheStateFilepath))
            {
                string jsonstate = File.ReadAllText(mRepoCacheStateFilepath);
                mRepoCacheList = JsonConvert.DeserializeObject<List<RepoCacheState>>(jsonstate);
            }
        }

        private void SaveCacheState()
        {
            //string statejson = System.Text.Json.JsonSerializer.Serialize(mRepoCacheList.CacheList);


            for (int i = mRepoCacheList.Count - 1; i > -1; i--)
            {
                if( !mRepoCacheList[i].CloneCompleted )
                {
                    mRepoCacheList.RemoveAt(i);
                }
            }



            string statejson = JsonConvert.SerializeObject(mRepoCacheList);


            if ( File.Exists(mRepoCacheStateFilepath ) )
            {

                File.Delete(mRepoCacheStateFilepath);
            }


            File.WriteAllText(mRepoCacheStateFilepath, statejson);
        }

        private void ManageCaches()
        {
            // checks that the number of available caches match the cache size and creates caches if needed

            // for existing caches, it checks that the cache has been cloned and, if so, ensures each cache has been git pulled today.

            if( mRepoCacheList.Count < mCacheSize)
            {
                string cacheid = Guid.NewGuid().ToString().Substring(0,4);
                // we need to create some
                RepoCacheState newCacheState = new RepoCacheState();
                newCacheState.Path = CreateNewCache(cacheid);
                mRepoCacheList.Add(newCacheState);
            //    mRepoCacheList.count++;
            }
        }


        private void VerifyCacheState()
        {
            for( int i = mRepoCacheList.Count -1; i >= 0; i--)
            {
                var cache = mRepoCacheList[i];
                if (!Directory.Exists(cache.Path))
                    mRepoCacheList.RemoveAt(i);
            }
            return;
        }

        public bool SetupTicket(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }


            for( int i = mRepoCacheList.Count - 1; i > -1 ; i-- )
            {
                var cache = mRepoCacheList[i];
                if ( cache.CloneCompleted )
                {

                    Directory.Move(cache.Path, path);

                    mRepoCacheList.RemoveAt(i);


                    if (!File.Exists(path + @"\" + RepoManager.GITKEEP_INITIAL))
                    {
                        Directory.CreateDirectory(path + @"\" + RepoManager.GITKEEP_INITIAL);
                    }

                    if (!File.Exists(path + @"\" + RepoManager.GITKEEP_UPDATE))
                    {
                        Directory.CreateDirectory(path + @"\" + RepoManager.GITKEEP_UPDATE);
                    }

                    if (!File.Exists(path + @"\" + RepoManager.WIP))
                    {
                        Directory.CreateDirectory(path + @"\" + RepoManager.WIP);
                    }

                    break;

                }

            }

            //RepoCacheState cache = mRepoCacheList.ElementAt(mRepoCacheList.Count - 1);
            ManageCaches();
            return true;
        }
    
/*
        private void Init()
        {
            //TestSerialization();
            // read all the existing folders in the cache
            
            // each cache folder needs a persistent state: has git clone run successfully once?

            // if a cache has been cloned, how often to pull into it? once daily (for each cache).
        }
        */
   /*     public void PrepareCache( string cacheFolder)
        {
           ;
            if ( !Directory.Exists( cacheFolder ))
            {
                Directory.CreateDirectory(cacheFolder);
            }



        }*/
    }
}
