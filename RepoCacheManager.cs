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

        private static Mutex mCacheStateMutex = new Mutex();
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
        
        private List<RepoCacheState> mSharedRepoCacheList = null;                 // state of all the cached repos (serialized to mRepoCacheStateFilepath)

        private List<ThreadArgs> mCacheCreateThreads = null;                           // list of all created threads
        private bool mIsShuttingDown = false;

       // public int AvailableCaches { get => mRepoCacheList.Where(x => x.CloneCompleted).Count(); }
        
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


            mCacheCreateThreads = new List<ThreadArgs>();

            LoadCacheState();
            VerifyCacheState();

            var repoCacheList = BorrowRepoCacheStates(null);
            try
            {
                var AvailableCaches = repoCacheList.Where(x => x.CloneCompleted).Count();
                callbackInfoUpdate?.Invoke($"Available Caches: {AvailableCaches}/{cachesize}");

            }
            finally
            {
                ReturnRepoCacheState(null);
            }

            //StartManager();
            DeleteTrash();
        }

        private void StartManager()
        {
            throw new NotImplementedException();
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

        public void Shutdown()
        {
            mIsShuttingDown = true;
            
            StopThreads();

            // at this point all threads should be dead, but there's a risk that a thread might have 
            // not released the mutext when it was aborted, so the mutex is ignored at this point in case
            // the main thread is blocked indefinitely (as no threads will be alive to release the mutex).

            SaveCacheState(mSharedRepoCacheList); 
        }


        private void StopThreads()
        {
            // terminates/destroys any threads currently active.

            foreach( ThreadArgs thread in mCacheCreateThreads )
            {

                thread.ThreadInstance.Abort();
            }
        }

        private void ReturnRepoCacheState( ThreadArgs theReturningThread )
        {
            string sThreadId = "main thread";
            if (theReturningThread != null) sThreadId = theReturningThread.ThreadID;

            Console.WriteLine($"{sThreadId} has released mCacheStateShared");
            mCacheStateMutex.ReleaseMutex();
        }

        private List<RepoCacheState> BorrowRepoCacheStates(ThreadArgs theRequestingThread)
        {
            string sThreadId = "main thread";
            if (theRequestingThread != null) sThreadId = theRequestingThread.ThreadID;
            Console.WriteLine($"{sThreadId} is waiting for mCacheStateShared");
            mCacheStateMutex.WaitOne();
            Console.WriteLine($"{sThreadId} has received mCacheStateShared");
            return mSharedRepoCacheList;
        }

        private void ThreadComplete( object args )
        {
            ThreadArgs threadargs = (ThreadArgs)args;

            // TODO: remove from threadlist

            mCacheCreateThreads.RemoveAll(item => item.ThreadID == threadargs.ThreadID);


            var repoCacheList = BorrowRepoCacheStates( threadargs );
            try
            {
                for (int i = 0; i < repoCacheList.Count; i++)
                {
                    if (repoCacheList[i].Path == threadargs.RepoPath)
                    {

                        string cachepath = "";
                        if (MoveWorkingToCache(threadargs.RepoPath, ref cachepath))
                        {
                            repoCacheList[i].Path = cachepath;
                            repoCacheList[i].CloneCompleted = true;
                            var nAvailableCaches = repoCacheList.Where(x => x.CloneCompleted).Count();
                            mCallbackInfo?.Invoke($"Cache Clone Completed ({threadargs.ThreadID}). Available Caches: {nAvailableCaches}/{mCacheSize}");
                            repoCacheList.RemoveAt(i);
                            SaveCacheState(repoCacheList);
                        }

                        break;
                    }
                }

            }
            finally { ReturnRepoCacheState(threadargs);  }

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

/*        private string CreateNewCache( string name )
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
            args.ThreadInstance.Name = args.ThreadID;
            args.ThreadInstance.Start(args);
           

            mCallbackInfo?.Invoke($"Preparing new cache. Available caches = {AvailableCaches}/{mCacheSize}");

            mCacheCreateThreads.Add(args);

            return cachefolder;
        }*/

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
                if (!mIsShuttingDown)
                {
                    threadargs.callbackError?.Invoke(e);

                }
                return;
            }
            if (!mIsShuttingDown)
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
            mSharedRepoCacheList = new List<RepoCacheState>();

            
            if( File.Exists(mRepoCacheStateFilepath))
            {
                string jsonstate = File.ReadAllText(mRepoCacheStateFilepath);
                mSharedRepoCacheList = JsonConvert.DeserializeObject<List<RepoCacheState>>(jsonstate);
            }
        }

        private void SaveCacheState( List<RepoCacheState> repoCacheList)
        {
            //string statejson = System.Text.Json.JsonSerializer.Serialize(mRepoCacheList.CacheList);
            //            var repoCacheList = BorrowRepoCacheStates(null);

            if (repoCacheList == null) return;

            string statejson = JsonConvert.SerializeObject(repoCacheList);

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

            //while( !mIsShuttingDown)
            var repoCacheList = BorrowRepoCacheStates(null);
            try
            {

                if (repoCacheList.Count < mCacheSize)
                {
                    string cacheid = Guid.NewGuid().ToString().Substring(0, 4);
                    // we need to create some
                    RepoCacheState newCacheState = new RepoCacheState();
                    ///
                    string cachefolder = mRepoWorkingFilepath + cacheid;
                    if (Directory.Exists(cachefolder))
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
                    args.ThreadInstance.Name = args.ThreadID;
                    args.ThreadInstance.Start(args);
                    //mRepoCacheList.Where(x => x.CloneCompleted).Count();

                    mCallbackInfo?.Invoke($"Preparing new cache. Available caches = {repoCacheList.Where(x => x.CloneCompleted).Count()}/{mCacheSize}");
                    mCacheCreateThreads.Add(args);

                    ///
                    newCacheState.Path = cachefolder;
                    repoCacheList.Add(newCacheState);
                }

            }
            finally
            {
                ReturnRepoCacheState( null );
            }
        }


        private void VerifyCacheState()
        {
            var repoCacheList = BorrowRepoCacheStates(null);
            try
            {

                for (int i = repoCacheList.Count - 1; i >= 0; i--)
                {
                    var cache = repoCacheList[i];
                    if (!Directory.Exists(cache.Path))
                        repoCacheList.RemoveAt(i);
                }
                return;
            }
            finally
            {
                ReturnRepoCacheState(null);
            }

        }

        public bool SetupTicket(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }


            var repoCacheList = BorrowRepoCacheStates(null);
            try
            {

                for (int i = repoCacheList.Count - 1; i > -1; i--)
                {
                    var cache = repoCacheList[i];
                    if (cache.CloneCompleted)
                    {

                        Directory.Move(cache.Path, path);

                        repoCacheList.RemoveAt(i);


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
            }
            finally
            {
                ReturnRepoCacheState(null);
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
