using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DAMBuddy2;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

public class RepoManager
{
    private string m_LocalPath = "";
    private string m_GitRepositoryURI = "https://github.com/ahs-ckm/ckm-mirror";
    private static string GITKEEP_INITIAL = @"\gitkeep\initial";
    private static string GITKEEP_UPDATE = @"\gitkeep\update";
    private static string GITKEEP_SUFFIX = ".keep";
    private static string WIP = @"\local\WIP";

    private FileSystemWatcher m_watcherRepo = null;
    private FileSystemWatcher m_watcherWIP = null;

    private int m_intervalPull = 5000;
    private System.Threading.Timer m_timerPull = null;

    private DateTime m_dtCloneStart;
    private DateTime m_dtCloneEnd;
    private Dictionary<string, string> m_dictWIPName2Path;
    private Dictionary<string, string> m_dictWIPID2Path;

    public delegate void ModifiedCallback(string filename);
    public delegate void StaleCallback(string filename);
    public delegate void DisplayWIPCallback(string filename, string originalpath);
    public delegate void RemoveWIPCallback(string filename);

    ModifiedCallback m_callbackModifiedWIP;
    RemoveWIPCallback m_callbackRemoveWIP;
    StaleCallback m_callbackStale;
    DisplayWIPCallback m_callbackDisplayWIP;

    public string LocalPath { get => m_LocalPath; }

    public RepoManager(string localpath, StaleCallback callbackStale, DisplayWIPCallback callbackDisplay, RemoveWIPCallback callbackRemove, ModifiedCallback callbackModifiedWIP)
    {
        m_callbackDisplayWIP = callbackDisplay;
        m_callbackStale = callbackStale;
        m_callbackRemoveWIP = callbackRemove;
        m_callbackModifiedWIP = callbackModifiedWIP;

        m_LocalPath = localpath + @"";


        if (!File.Exists(m_LocalPath + @"\" + GITKEEP_INITIAL))
        {
            Directory.CreateDirectory(m_LocalPath + @"\" + GITKEEP_INITIAL);
        }

        if (!File.Exists(m_LocalPath + @"\" + GITKEEP_UPDATE))
        {
            Directory.CreateDirectory(m_LocalPath + @"\" + GITKEEP_UPDATE);
        }

        if (!File.Exists(m_LocalPath + @"\" + WIP))
        {
            Directory.CreateDirectory(m_LocalPath + @"\" + WIP);
        }

    }

    public void Pull2()
    {

        string path = @"C:\Users\jonbeeby\source\repos\DamBuddy2\packages\PortableGit\bin\";
        //C: \Users\jonbeeby\source\repos\DamBuddy2\packages\PortableGit\bin\
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {

                    FileName = path + "git.exe",
                    WorkingDirectory = m_LocalPath,
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

    public void Pull()
    {
        PullOptions options = new PullOptions();
        options.FetchOptions = new FetchOptions();
        options.MergeOptions = new MergeOptions();

        options.MergeOptions.FailOnConflict = false;
        options.MergeOptions.MergeFileFavor = MergeFileFavor.Theirs;
        options.MergeOptions.FileConflictStrategy = CheckoutFileConflictStrategy.Theirs;
        //options.MergeOptions.CheckoutNotifyFlags

        /*var fetchOptions = new FetchOptions();

        TransferProgress p;
        

        TransferProgressHandler a

        fetchOptions.OnProgress
*/
        //options.FetchOptions.OnProgress = TransferProgress;
        Repository repo = new Repository(m_LocalPath);

        FetchOptions fetchoptions = new FetchOptions();
        var remote = repo.Network.Remotes["origin"];
        var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);

        //repo.Network.Pull(new Signature("truecraft", "git@truecraft.io", new DateTimeOffset(DateTime.Now)), options);
        try
        {
            Commands.Fetch(repo, remote.Name, refSpecs, null, "test");
            Branch head = repo.Branches.Single(branch => branch.FriendlyName == "master");
            //repo.Reset(ResetMode.Hard, repo.Branches.First(b => b.IsCurrentRepositoryHead).Tip);
            // repo.Reset(ResetMode.Hard, head.Tip);

            //var mergeoptions = new MergeOptions;

            repo.Merge(head, new Signature("truecraft", "git@truecraft.io", new DateTimeOffset(DateTime.Now)), options.MergeOptions);

            Commands.Pull(repo, new Signature("truecraft", "git@truecraft.io", new DateTimeOffset(DateTime.Now)), options);



            var checkoutOptions = new CheckoutOptions();
            checkoutOptions.CheckoutModifiers = CheckoutModifiers.Force;

            Commands.Checkout(repo, head, checkoutOptions);
            //repo.Checkout(head, checkoutOptions);

        } catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

    }

    public bool Clone()
    {
        m_dtCloneStart = DateTime.Now;

        CloneOptions options = new CloneOptions();
        options.OnTransferProgress = TransferProgress;


        if (!File.Exists(m_LocalPath))
        {
            Directory.CreateDirectory(m_LocalPath);
        }

        Repository.Clone(m_GitRepositoryURI, m_LocalPath, options);

        m_dtCloneEnd = DateTime.Now;

        return true;
    }

    // callback to notify stale state
    public void RegisterStaleCallBack(StaleCallback callback)
    {
        m_callbackStale = callback;
    }

    public void RegisterDisplayWIPCallback(DisplayWIPCallback callback)
    {
        m_callbackDisplayWIP = callback;
    }

    

    internal bool isAssetinWIPByID(string sEmbeddedId, ref string filepath)
    {
        filepath = "";
        bool exists = m_dictWIPID2Path.TryGetValue(sEmbeddedId, out filepath);
        return exists;


    }

    public static bool TransferProgress(TransferProgress progress)
    {
        Console.WriteLine($"Objects: {progress.ReceivedObjects} of {progress.TotalObjects}");
        return true;
    }

    // prepare an asset as WIP
    public void AddWIP(string filepath)
    {   //C:\TD\git2\1\mgr\WIP


        string filepathWIP = m_LocalPath + WIP + @"\" + Path.GetFileName(filepath);
        File.Copy(filepath, filepathWIP);

        string initialFile = m_LocalPath + GITKEEP_INITIAL + @"\" + Path.GetFileName(filepath) + GITKEEP_SUFFIX;

        File.Move(filepath, initialFile);

        MakeMd52(initialFile);

        m_dictWIPName2Path[Path.GetFileName(filepath)] = filepath; // name -> filepath
        m_dictWIPID2Path[Utility.GetTemplateID(filepathWIP)] = filepathWIP; // id -> filepath

        SaveExistingWip();

        if (m_callbackDisplayWIP != null)
        {
            m_callbackDisplayWIP(Path.GetFileName(filepath), filepath);
        }

        //copy filepath to 
    }

    private void SaveInitialState(string filepath)
    {
        // copy to git
        string filename = Path.GetFileName(filepath);
        string gitinitpath = m_LocalPath + @"\" + GITKEEP_INITIAL;
        File.Move(filepath, gitinitpath);

        MakeMd52(gitinitpath);
    }


    public bool isAssetinWIP(string filename)
    {
        string sTID = "";
        bool exists = m_dictWIPName2Path.TryGetValue(Path.GetFileName(filename), out sTID);
        return exists;
    }

    private void SaveUpdateState(string filepath)
    {
        // copy to git
        string filename = Path.GetFileName(filepath);
        string gitpath = m_LocalPath + @"\" + GITKEEP_UPDATE;
        string gitkeepfile = gitpath + @"\" + filename + GITKEEP_SUFFIX;
        if (File.Exists(gitkeepfile))
        {
            File.Delete(gitkeepfile);
        }
        File.Move(filepath, gitkeepfile);

        MakeMd52(gitkeepfile);
    }
    
    public string GetTemplateID( string filename )
    {
        if( isAssetinWIP(filename))
        {
            return Utility.GetTemplateID(m_LocalPath + @"\" + WIP + @"\" + filename);
        }

        return "";
    }

    private bool IsStale(string asset)
    {

        if (!File.Exists(m_LocalPath + GITKEEP_INITIAL + @"\" + asset + GITKEEP_SUFFIX + ".md5")) 
            return false;

        if (!File.Exists(m_LocalPath + GITKEEP_UPDATE + @"\" + asset + GITKEEP_SUFFIX + ".md5")) 
            return false;

        string WIPHash = File.ReadAllText(m_LocalPath + GITKEEP_INITIAL + @"\" + asset + GITKEEP_SUFFIX + ".md5");

        string UpdateHash = File.ReadAllText(m_LocalPath + GITKEEP_UPDATE + @"\" + asset + GITKEEP_SUFFIX + ".md5");

        if (String.Compare(WIPHash, UpdateHash) == 0)
        {
            return false;
        }

        return true;
    }

    // called when an asset is 
    public void UpdateOnWIP(string filepath)
    {
        // move file to gitupdate
        SaveUpdateState(filepath);
        if (IsStale(Path.GetFileName(filepath)))
        {
            if (m_callbackStale != null)
            {
                m_callbackStale(Path.GetFileName(filepath));
            }
            //MessageBox.Show("Stale Asset " + Path.GetFileName(filepath));
        }
        // 
    }

    private void TimeToPull(Object info)
    {
        Console.WriteLine("TimeToPull()");
        //Pull();
        Pull2();
    }

    private string PackAsset2(string filepath)
    {
        string contents = File.ReadAllText(filepath);
        string packed = System.Text.RegularExpressions.Regex.Replace(contents, @"\s+", String.Empty);
        packed = System.Text.RegularExpressions.Regex.Replace(packed, @"\s\n", String.Empty);
        return packed;
    }

    private void PackAsset(string filepath)
    {
        string contents = File.ReadAllText(filepath);
        string packed = System.Text.RegularExpressions.Regex.Replace(contents, @"\s+", String.Empty);
        packed = System.Text.RegularExpressions.Regex.Replace(packed, @"\s\n", String.Empty);


        File.WriteAllText(filepath, packed);

    }

    private void MakeMd52(string filepath)
    {
        // strip all spaces and write md5 to asset.oet.md5
        string packedasset = PackAsset2(filepath);
        string hashvalue = "";

        using (var md5 = MD5.Create())
        {

            var bytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(packedasset));
            hashvalue = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
           
        }

        if (File.Exists(filepath + ".md5")) File.Delete(filepath + ".md5");

        File.WriteAllText(filepath + ".md5", hashvalue);

    }


    private void MakeMd5(string filepath)
    {
        // strip all spaces and write md5 to asset.oet.md5
        PackAsset(filepath);
        string hashvalue = "";

        using (var md5 = MD5.Create())
        {
            using( var stream = File.OpenRead(filepath))
            {
                var bytes = md5.ComputeHash(stream);
                hashvalue = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            }
        }

        if (File.Exists(filepath + ".md5")) File.Delete(filepath + ".md5");

        File.WriteAllText(filepath + ".md5", hashvalue);
    }

    public bool DoClone()
    {
        try
        {
            Clone();
            return true;
        } catch
        {
            return false;
        }

    }
    
    public void Init( int PullDelay, int PullInterval )
    {
		m_intervalPull = PullInterval;
        
		m_timerPull = new System.Threading.Timer(TimeToPull,null, PullDelay, m_intervalPull);

        m_dictWIPName2Path = new Dictionary<string, string>();
        m_dictWIPID2Path = new Dictionary<string, string>();

        m_watcherRepo = new FileSystemWatcher();
        m_watcherRepo.Path = m_LocalPath + @"local\templates";
        m_watcherRepo.IncludeSubdirectories = true;
        m_watcherRepo.NotifyFilter = NotifyFilters.LastWrite;
        m_watcherRepo.Filter = "*.oet";
        m_watcherRepo.Created += OnChangedRepo;
        m_watcherRepo.Changed += OnChangedRepo;

        m_watcherRepo.EnableRaisingEvents = true;

        m_watcherWIP = new FileSystemWatcher();
       
        m_watcherWIP.Path = m_LocalPath + WIP;
        m_watcherWIP.IncludeSubdirectories = true;
        m_watcherWIP.NotifyFilter = NotifyFilters.LastWrite;
        m_watcherWIP.Filter = "*.oet";
        m_watcherWIP.Created += OnChangedWIP;
        m_watcherWIP.Changed += OnChangedWIP;
        m_watcherWIP.EnableRaisingEvents = true;


        LoadExistingWIP();



    }

    public void SaveExistingWip()
    {
        string csv = "";
        foreach (KeyValuePair<string, string> kvp in m_dictWIPName2Path)
        {
            csv += kvp.Key;
            csv += ",";
            csv += kvp.Value;
            csv += "\n"; //newline to represent new pair
        }

        File.WriteAllText(m_LocalPath + @"\" + WIP + @"\WIP.csv", csv);



        csv = "";
        foreach (KeyValuePair<string, string> kvp in m_dictWIPID2Path)
        {
            csv += kvp.Key;
            csv += ",";
            csv += kvp.Value;
            csv += "\n"; //newline to represent new pair
        }

        File.WriteAllText(m_LocalPath + @"\" + WIP + @"\WIPID.csv", csv);
    }

    public void DisplayWIP( string filename, string originalpath)
    {
        if (m_callbackDisplayWIP != null)
        {
            m_callbackDisplayWIP(filename, originalpath);
        }
    }

    public void LoadExistingWIP()
    {
        string filepath = m_LocalPath + @"\" + WIP + @"\WIP.csv";
        if (File.Exists(filepath))
        {
            var reader = new StreamReader(File.OpenRead(filepath));
            
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == "") break;
                var values = line.Split(',');
                
                m_dictWIPName2Path.Add(values[0], values[1]);
                DisplayWIP(values[0], values[1]);
            }
        }


        filepath = m_LocalPath + @"\" + WIP + @"\WIPID.csv";
        if (File.Exists(filepath))
        {
            var reader = new StreamReader(File.OpenRead(filepath));

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == "") break;
                var values = line.Split(',');

                m_dictWIPID2Path.Add(values[0], values[1]);
            }           
        }
    }

    public bool RemoveWIP( string filename, string gitpath )
    {
        // if modified message the user

        // delete file in WIP
        

        string wipFile = m_LocalPath + @"\" + WIP + @"\" + filename;
        string initialFile = m_LocalPath + @"\" + GITKEEP_INITIAL+ @"\" + filename + GITKEEP_SUFFIX;
        string updateFile = m_LocalPath + @"\" + GITKEEP_UPDATE + @"\" + filename + GITKEEP_SUFFIX;
        try
        {

            if (File.Exists(wipFile))
            {
                m_dictWIPName2Path.Remove(filename);
                m_dictWIPName2Path.Remove(Utility.GetTemplateID(wipFile));

                File.Delete(wipFile);
                if( File.Exists(wipFile + ".md5"))
                {
                    File.Delete(wipFile + ".md5");
                }

                
            }

            // move inital/update file back to repo path
            if (File.Exists(updateFile))
            {
                File.Move(updateFile, gitpath);
                if (File.Exists(initialFile)) File.Delete(initialFile);
                if (File.Exists(updateFile + ".md5")) File.Delete(updateFile + ".md5");
            }
            else
            {
                File.Move(initialFile, gitpath);
                if (File.Exists(initialFile + ".md5")) File.Delete(initialFile + ".md5");
            
            }

            if ( m_callbackRemoveWIP != null)
            {
                m_callbackRemoveWIP(filename);

            }

            SaveExistingWip();
        }
        catch ( Exception e )
        {
            
        }

        return true;
    }

    

    private void OnChangedWIP(object source, FileSystemEventArgs e)
    {

        Console.WriteLine($"OnChangedWIP File: {e.FullPath} {e.ChangeType}");

        // TODO:
        // should check whether the md5 of the file is actually different to the initial md5

        if (m_callbackModifiedWIP != null )
        {
            m_callbackModifiedWIP(Path.GetFileName(e.FullPath));
        }

    }

    private void OnChangedRepo(object source, FileSystemEventArgs e)
    {

        Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
        
        if( isAssetinWIP(e.Name) )
        {
            UpdateOnWIP(e.FullPath);
        }

        
    }
    

}
