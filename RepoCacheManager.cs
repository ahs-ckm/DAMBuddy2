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

        private static string REPOCACHE_FOLDER = @"\repocache";             // the name of the folder under which the cache folders are stored.
        private string STATE_FILEPATH = @"\repoliststate.json";             // the file holding the state of the cache for persistence

        private string mRepoCacheStateFilepath = "";
        private string mGitBinariesPath = "";                               // path the folder containing git binaries
        private string mRemoteGitRepository = "";                           // the URL for the git repo to be cloned (the CKMMirror repository)

        public string mRootFolder = "";                                     // the top level directory path
        private int mCacheSize = -1;                                        // how many cache folders should be maintained 
        
        private List<RepoCacheState> mRepoCacheList = null;                             // state of all the cached repos (serialized to mRepoCacheStateFilepath)

        private List<ThreadArgs> mThreads = null;                           // list of all created threads
        
        public RepoCacheManager( string rootfolder, int cachesize, string gitRemoteRepoURL, string gitBinariesPath )
        {
            mRootFolder = rootfolder;
            mCacheSize = cachesize;
            mGitBinariesPath = gitBinariesPath;
            mRemoteGitRepository = gitRemoteRepoURL;

            mRepoCacheStateFilepath = mRootFolder + REPOCACHE_FOLDER + STATE_FILEPATH;

            mThreads = new List<ThreadArgs>();

            Init();
            EmptyTrash();
        }
        
        ~RepoCacheManager()
        {
//            SaveCacheState();
//            StopThreads();
        }

        public void CloseDown()
        {
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

        private void EmptyTrash()
        {
            // removing repositories can take time due to the high number of files
            // TODO: threaded delete.
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
                    mRepoCacheList[i].CloneCompleted = true;
                    break;
                }
            }

            

        }



        private void ThreadException( object e )
        {
            Exception theException = (Exception)e;
            // TODO: process exception.
        }

        private string CreateNewCache( string name )
        {

            string cachefolder = mRootFolder + REPOCACHE_FOLDER + @"\" + name;
            if ( Directory.Exists( cachefolder))
            {
                Directory.Delete(cachefolder);
            }

            Directory.CreateDirectory(cachefolder);

            var args = new ThreadArgs();
            args.ThreadID = $"{cachefolder}-clone";
            args.RepoPath = cachefolder;
            args.ThreadInstance = new Thread(Clone);
            args.callbackError = ThreadException;
            args.callbackComplete = ThreadComplete;

            args.ThreadInstance.Start(args);

            mThreads.Add(args);

            return cachefolder;
        }

        public void Pull2( string repofolder )
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
                    Directory.CreateDirectory(threadargs.RepoPath);
                }

                Repository.Clone(mRemoteGitRepository, threadargs.RepoPath, options);

            } catch ( Exception e )
            {
                threadargs.callbackError?.Invoke(e) ;
            }

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

        private void TestSerialization()
        {
            LoadCacheState();
            RepoCacheState state = new RepoCacheState();
            state.Path = "test";
            mRepoCacheList.Add(state);
            SaveCacheState();

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

            return true;
        }
    

        private void Init()
        {
            //TestSerialization();
            // read all the existing folders in the cache
            LoadCacheState();
            ManageCaches();
            
            // each cache folder needs a persistent state: has git clone run successfully once?

            // if a cache has been cloned, how often to pull into it? once daily (for each cache).
        }
        
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
