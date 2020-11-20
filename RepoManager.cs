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
using System.IO.Compression;
using System.Net;
using net.sf.saxon.trans.rules;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;

public static class Extensions
{

    public static IEnumerable<IEnumerable<T>> Page<T>(this IEnumerable<T> source, int pageSize)
    {
        Contract.Requires(source != null);
        Contract.Requires(pageSize > 0);
        Contract.Ensures(Contract.Result<IEnumerable<IEnumerable<T>>>() != null);

        using (var enumerator = source.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                var currentPage = new List<T>(pageSize)
                {
                    enumerator.Current
                };

                while (currentPage.Count < pageSize && enumerator.MoveNext())
                {
                    currentPage.Add(enumerator.Current);
                }
                yield return new ReadOnlyCollection<T>(currentPage);
            }
        }
    }

}

public class RepoManager
{
    private static string DAM_UPLOAD_PORT = "10091"; // DEV
    private static string DAM_SCHEDULER_PORT = "10008";

    private static string CURRENT_REPO = "CURRENT_REPO";
    private static string FOLDER_ROOT = @"c:\TD";

    private static string BIN_DIR = @"C:\Users\jonbeeby\source\repos\DamBuddy2\packages\PortableGit\bin\";
       

    private string m_GitRepositoryURI = "https://github.com/ahs-ckm/ckm-mirror";
    private static string GITKEEP_INITIAL = @"\gitkeep\initial";
    private static string GITKEEP_UPDATE = @"\gitkeep\update";
    private static string KEEP_TRASH = @"\trash";
    private static string GITKEEP_SUFFIX = ".keep";
    //private static string WIP = @"\local\WIP";
    private static string ASSETS = @"\local";
    private static string WIP = @"\" + ASSETS + @"\WIP";
    

    private string gServerName = "http://ckcm.healthy.bewell.ca";

    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>

    public string WIPPath
    {
        get => m_ticketBaseFolder + WIP;
    }

    public string AssetPath
    {
        get => m_ticketBaseFolder + ASSETS;
    }


    private string m_ticketBaseFolder = "";
    private List<ListViewItem> m_masterlist;
    

    private FileSystemWatcher m_watcherRepo = null;
    private FileSystemWatcher m_watcherWIP = null;

    private int m_intervalPull = 5000;
    private System.Threading.Timer m_timerPull = null;

    private DateTime m_dtCloneStart;
    private DateTime m_dtCloneEnd;

    bool m_ReadyStateSetByUser = false;
    private static Dictionary<string, string> m_dictFileToPath;
    private Dictionary<string, string> m_dictID2Gitpath;
    private Dictionary<string, string> m_dictWIPName2Path;
    private Dictionary<string, string> m_dictWIPID2Path;

    private Dictionary<string, string> m_dictRepoState;

    public delegate void ModifiedCallback(string filename);
    public delegate void StaleCallback(string filename);
    public delegate void DisplayWIPCallback(string filename);//, string originalpath);
    public delegate void RemoveWIPCallback(string filename);
    public delegate void UploadStateCallback(string Ticket, TicketChangeState state);

    ModifiedCallback m_callbackScheduleState;
    ModifiedCallback m_callbackTicketState;
    ModifiedCallback m_callbackModifiedWIP;
    RemoveWIPCallback m_callbackRemoveWIP;
    StaleCallback m_callbackStale;
    DisplayWIPCallback m_callbackDisplayWIP;
    UploadStateCallback m_callbackUploadState;

    public string TicketFolder { get => m_ticketBaseFolder; }
    public ModifiedCallback CallbackScheduleState { get => m_callbackScheduleState; set => m_callbackScheduleState = value; }
    public ModifiedCallback CallbackTicketState { get => m_callbackTicketState; set => m_callbackTicketState = value; }
    public List<ListViewItem> Masterlist { get => m_masterlist; set => m_masterlist = value; }
    public UploadStateCallback CallbackUploadState { get => m_callbackUploadState; set => m_callbackUploadState = value; }

    public RepoManager( StaleCallback callbackStale, DisplayWIPCallback callbackDisplay, RemoveWIPCallback callbackRemove, ModifiedCallback callbackModifiedWIP)
    {
        m_callbackDisplayWIP = callbackDisplay;
        m_callbackStale = callbackStale;
        m_callbackRemoveWIP = callbackRemove;
        m_callbackModifiedWIP = callbackModifiedWIP;

        LoadRepositoryState();


        string repoFolder = GetCurrentRepository();

        m_ticketBaseFolder = FOLDER_ROOT + @"\" + repoFolder + @"";

        m_masterlist = new List<ListViewItem>();


        if (!File.Exists(m_ticketBaseFolder + @"\" + GITKEEP_INITIAL))
        {
            Directory.CreateDirectory(m_ticketBaseFolder + @"\" + GITKEEP_INITIAL);
        }

        if (!File.Exists(m_ticketBaseFolder + @"\" + GITKEEP_UPDATE))
        {
            Directory.CreateDirectory(m_ticketBaseFolder + @"\" + GITKEEP_UPDATE);
        }

        if (!File.Exists(m_ticketBaseFolder + @"\" + WIP))
        {
            Directory.CreateDirectory(m_ticketBaseFolder + @"\" + WIP);
        }

    }

    public void ConfigureAndLaunchTD()
    {
        string config = GetTicketConfigForOcean();
        OceanUtils.ConfigureTD( GetCurrentRepository(), config);
//        OceanUtils.AddConfig2()
        OceanUtils.LaunchTD();
    }

    private void LoadRepositoryState()
    {
        m_dictRepoState = new Dictionary<string, string>();

        string filepath = FOLDER_ROOT + @"\" + "repostate.csv";
        if (File.Exists(filepath))
        {
            var reader = new StreamReader(File.OpenRead(filepath));

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == "") break;
                var values = line.Split(',');

                m_dictRepoState.Add(values[0], values[1]);
            }
        }

    }

    private void SaveepositoryState()
    {
        string filepath = FOLDER_ROOT + @"\" + "repostate.csv";
        string csv = "";
        foreach (KeyValuePair<string, string> kvp in m_dictRepoState)
        {
            csv += kvp.Key;
            csv += ",";
            csv += kvp.Value;
            csv += "\n"; //newline to represent new pair
        }

        File.WriteAllText(filepath, csv);

    }

    public string GetCurrentRepositoryFolder()
    {
        return m_dictRepoState[m_dictRepoState[CURRENT_REPO]];
    }
    public string GetCurrentRepository()
    {
        return m_dictRepoState[CURRENT_REPO];
       
    }

    private void ProcessTicketState( string sTicketID )
    {
        TicketChangeState state = GetTicketUploadState(sTicketID);
        if( !state.active )
        {
            RemoveTicket(sTicketID);
            CallbackUploadState?.Invoke(sTicketID, state);
        }

    }

    private bool BackupTicket( string sTicketID )
    {
        // TODO: zip up to backup folder
        MessageBox.Show("Backing up :" + m_ticketBaseFolder);
        return true;
    }

    private void RemoveTicket( string sTicketID )
    {
        if( BackupTicket(sTicketID))
        {
            // TODO delete folder tree
            //Directory.Delete(m_ticketBaseFolder, true);
            
            m_dictRepoState.Remove(sTicketID);
            if( m_dictRepoState[CURRENT_REPO] == sTicketID ) { m_dictRepoState[CURRENT_REPO] = "";  }
        }
    }

    private TicketChangeState GetTicketUploadState( string sTicketID )
    {

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{gServerName}:{DAM_UPLOAD_PORT}/dynamic/change_status,{GetCurrentRepository()}");
        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        TicketChangeState state;

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            string jsonState = reader.ReadToEnd();
            state = System.Text.Json.JsonSerializer.Deserialize<RepoManager.TicketChangeState>(jsonState);
            

        }

        return state;
    }

    public List<string> GetAvailableRepositories()
    {
        List<string> repos = new List<string>();

        foreach (var item in m_dictRepoState.Keys)
        {
            if (item == CURRENT_REPO) continue;
            repos.Add(item);

        }
      ///  repos.Add("CSDFK-1488");
       // repos.Add("CSDFK-1489");

        return repos;
    }

    public bool SetCurrentRepository(string RepoName)
    {
        m_ticketBaseFolder= RepoName;

        return true;
    }



    private void CreateFolderStructure( string ticketname )
    {
        // path wrong : Directory.CreateDirectory(m_RepoPath + ticketname);

    }

    public string GetTicketConfigForOcean( )
    {
        string description = "the description";
        string ticket = GetCurrentRepository() ;
        
        string config = $@"<DAM><RepositoryData><RepositoryName>{ticket}</RepositoryName><Description>{description}</Description><TemplatesPath>{AssetPath}</TemplatesPath><ArchetypesPath>{AssetPath}\archetypes</ArchetypesPath><WorkingArchetypesPath/><CkmApiUrl>https://ahsckm.ca/ckm/rest/v1/</CkmApiUrl><CkmApiBatchSize>300</CkmApiBatchSize></RepositoryData></DAM>";

        return config;
    }

    public void Pull2()
    {
        string path = BIN_DIR;
        
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {

                    FileName = path + "git.exe",
                    WorkingDirectory = m_ticketBaseFolder,
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
  
        Repository repo = new Repository(m_ticketBaseFolder);

        FetchOptions fetchoptions = new FetchOptions();
        var remote = repo.Network.Remotes["origin"];
        var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);

        try
        {
            Commands.Fetch(repo, remote.Name, refSpecs, null, "test");
            Branch head = repo.Branches.Single(branch => branch.FriendlyName == "master");
            repo.Merge(head, new Signature("truecraft", "git@truecraft.io", new DateTimeOffset(DateTime.Now)), options.MergeOptions);

            Commands.Pull(repo, new Signature("truecraft", "git@truecraft.io", new DateTimeOffset(DateTime.Now)), options);
            var checkoutOptions = new CheckoutOptions();
            checkoutOptions.CheckoutModifiers = CheckoutModifiers.Force;

            Commands.Checkout(repo, head, checkoutOptions);

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

    }



    private void Empty(System.IO.DirectoryInfo directory)
    {
        foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
        foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
    }



    public bool Clone()
    {
        m_dtCloneStart = DateTime.Now;

        CloneOptions options = new CloneOptions();
        options.OnTransferProgress = TransferProgress;


        if (!File.Exists(m_ticketBaseFolder))
        {
            Directory.CreateDirectory(m_ticketBaseFolder);
        }

        Repository.Clone(m_GitRepositoryURI, m_ticketBaseFolder, options);

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



    public bool SetTicketReadiness(bool bReady)
    {
        m_ReadyStateSetByUser = bReady;
        bool result = false;
        string ReadyParam = "notready";
        if (bReady)
        {
            ReadyParam = "ready";
        }

        string damFolder = GetCurrentRepositoryFolder();

        string theParams = $"theState={ReadyParam}&theFolder={damFolder}";

        try
        {
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string response = client.UploadString(gServerName + ":" + DAM_UPLOAD_PORT + "/ready", theParams);
                Console.WriteLine(response);
            }
        }
        catch
        {
            MessageBox.Show("Failed to set ticket to Ready");
            return false;
        }

        UpdateSchedule();
        GetTicketScheduleStatus();
        return true;
    }

    public bool PrepareNewTicket( string theTicketJSON )
    {
        // set it up on the server
        // get the server folder back?

        // create directory structure
        // move pre-prepared git folder

        // switch context.

        return false;
    }

    public bool GetTicketScheduleStatus()
    {

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{gServerName}:{DAM_SCHEDULER_PORT}/dynamic/TicketStatus,{GetCurrentRepository()}");
        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            string jsonStatus = reader.ReadToEnd();
            CallbackScheduleState?.Invoke(jsonStatus);

        }

        return true;
    }

    private bool UpdateSchedule()
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{gServerName}:{DAM_SCHEDULER_PORT}/dynamic/BuildPlan.json");
        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            string status = reader.ReadToEnd();

        }

        return true;
    }


    private bool WIPRemoveFromServer(string sTemplateName, string sTID)
    {
        bool result = false;
        string damFolder = GetCurrentRepositoryFolder();

        using (WebClient client = new WebClient())
        {
            string theParams = $"theFolder={damFolder}&theTemplateID={sTID}";
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            string response = client.UploadString(gServerName + ":" + DAM_UPLOAD_PORT + "/RemoveWIP", theParams);
            Console.WriteLine(response);
        }
        result = true;


        return result;
    }


    private bool WIPToServer(string sTemplateName, string sTID)
    {
        return PostWIP();
        
        bool result = false;
        string damfolder = GetCurrentRepositoryFolder();

        using (WebClient client = new WebClient())
        {
            string theParams = $"theFolder={damfolder}&theTemplateID={sTID}&theTemplateName={sTemplateName}";
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            string response = client.UploadString(gServerName + ":" + DAM_UPLOAD_PORT + "/WIP", theParams);
            Console.WriteLine(response);
        }
        result = true;


        return result;
    }

    public void ApplyFilter(string filtertext)
    {

        return;
    }

    public string GetTemplateFilepath(string filename)
    {
        string sPath;
        bool exists = m_dictWIPName2Path.TryGetValue(Path.GetFileName(filename), out sPath);
        if (exists) return sPath;


        try
        {
            return m_dictFileToPath[filename];

        }
        catch { return ""; }
    }


    public void LoadRepositoryTemplates()
    {
        m_masterlist.Clear();
        ListViewItem newAsset = null;
        if (m_dictFileToPath == null) m_dictFileToPath = new Dictionary<string, string>();


        string[] templates = Directory.GetFiles(m_ticketBaseFolder, "*.oet", SearchOption.AllDirectories);
        foreach (string template in templates)
        {
            string filename = Path.GetFileName(template);

            m_dictFileToPath[filename] = template;

            newAsset = new ListViewItem(filename);
            newAsset.Tag = template;

            m_masterlist.Add(newAsset);
        }


    }
    // prepare an asset as WIP
    public void AddWIP(string gitfilepath)
    {

        string filename = Path.GetFileName(gitfilepath);
        string filepathWIP = m_ticketBaseFolder + WIP + @"\" + filename;
        string initialFile = m_ticketBaseFolder + GITKEEP_INITIAL + @"\" + filename + GITKEEP_SUFFIX;

        File.Copy(gitfilepath, filepathWIP);
        File.Move(gitfilepath, initialFile);

        MakeMd52(initialFile);

        string sTID = Utility.GetTemplateID(filepathWIP);

        m_dictID2Gitpath[sTID] = gitfilepath; // id -> original directory in git repo
        m_dictWIPName2Path[filename] = filepathWIP; // name -> filepath
        m_dictWIPID2Path[sTID] = filepathWIP; // id -> filepath

        WIPToServer(filename, sTID);

        SaveExistingWip();

        m_callbackDisplayWIP?.Invoke(filename);//;, gitfilepath);  // TODO - needs to be git filepath
    }

    private void SaveInitialState(string filepath)
    {
        // copy to git
        string filename = Path.GetFileName(filepath);
        string gitinitpath = m_ticketBaseFolder + @"\" + GITKEEP_INITIAL;
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
        string gitpath = m_ticketBaseFolder + @"\" + GITKEEP_UPDATE;
        string gitkeepfile = gitpath + @"\" + filename + GITKEEP_SUFFIX;
        if (File.Exists(gitkeepfile))
        {
            File.Delete(gitkeepfile);
        }
        File.Move(filepath, gitkeepfile);

        MakeMd52(gitkeepfile);
    }

    public string GetTemplateID(string filename)
    {
        if (isAssetinWIP(filename))
        {
            return Utility.GetTemplateID(m_ticketBaseFolder + @"\" + WIP + @"\" + filename);
        }

        return Utility.GetTemplateID(m_dictFileToPath[filename]);
    }

    private bool IsStale(string asset)
    {

        if (!File.Exists(m_ticketBaseFolder + GITKEEP_INITIAL + @"\" + asset + GITKEEP_SUFFIX + ".md5"))
            return false;

        if (!File.Exists(m_ticketBaseFolder + GITKEEP_UPDATE + @"\" + asset + GITKEEP_SUFFIX + ".md5"))
            return false;

        string WIPHash = File.ReadAllText(m_ticketBaseFolder + GITKEEP_INITIAL + @"\" + asset + GITKEEP_SUFFIX + ".md5");

        string UpdateHash = File.ReadAllText(m_ticketBaseFolder + GITKEEP_UPDATE + @"\" + asset + GITKEEP_SUFFIX + ".md5");

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
        }
    }

    private void TimeToPull(Object info)
    {
        Console.WriteLine("TimeToPull()");
        Pull2();
        GetTicketScheduleStatus();
    }

    private string ReadAsset(string filepath)
    {
       

        int retrycount = 0;
        bool opened = false;
        string contents = "";

        while (retrycount < 10 && !opened)
        {
            try // this needs to be retried because of race conditions between the file copy to WIP and the file change event handler for WIP
            {
                contents = File.ReadAllText(filepath);
                opened = true;
            }
            catch
            {
                retrycount++;
                Console.WriteLine($"PackAsset2: Retrying {retrycount} on {filepath}");
                Thread.Sleep(1000);
            }
            opened = true;
        }
        
        if (!opened)
        {
            throw new Exception($"PackAsset2() Failed to open file {filepath}");
        }
        return contents;
    }



    private void MakeMd52(string filepath)
    {
        // strip all spaces and write md5 to asset.oet.md5
        string assetcontent = ReadAsset(filepath);
        string hashvalue = "";
        byte[] hashBytes = { };

        using (var md5 = MD5.Create())
        {
            hashBytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(assetcontent));
        }

        if (File.Exists(filepath + ".md5")) File.Delete(filepath + ".md5");
        string hex = BitConverter.ToString(hashBytes);
        File.WriteAllText(filepath + ".md5", hex);
    }



    public bool DoClone()
    {
        try
        {
            Clone();
            return true;
        }
        catch
        {
            return false;
        }

    }

    public void Init(int PullDelay, int PullInterval)
    {
        m_intervalPull = PullInterval;

        m_timerPull = new System.Threading.Timer(TimeToPull, null, PullDelay, m_intervalPull);

        m_dictID2Gitpath = new Dictionary<string, string>();
        m_dictWIPName2Path = new Dictionary<string, string>();
        m_dictWIPID2Path = new Dictionary<string, string>();

        m_watcherRepo = new FileSystemWatcher();
        m_watcherRepo.Path = m_ticketBaseFolder + @"\" + ASSETS + @"\templates";
        m_watcherRepo.IncludeSubdirectories = true;
        m_watcherRepo.NotifyFilter = NotifyFilters.LastWrite;
        m_watcherRepo.Filter = "*.oet";
        m_watcherRepo.Created += OnChangedRepo;
        m_watcherRepo.Changed += OnChangedRepo;

        m_watcherRepo.EnableRaisingEvents = true;

        m_watcherWIP = new FileSystemWatcher();

        m_watcherWIP.Path = m_ticketBaseFolder + WIP;
        m_watcherWIP.IncludeSubdirectories = true;
        m_watcherWIP.NotifyFilter = NotifyFilters.LastWrite;
        m_watcherWIP.Filter = "*.oet";
        m_watcherWIP.Changed += OnChangedWIP;
        m_watcherWIP.EnableRaisingEvents = true;

        LoadExistingWIP();



        List<string> processlist = new List<string>();

        foreach ( string ticket in m_dictRepoState.Keys)
        {
            processlist.Add(ticket);         
        }


        foreach( string ticket in processlist )
        {
            if (ticket != CURRENT_REPO)
            {
                ProcessTicketState(ticket);
            }

        }
       // ProcessTicketState(m_dictRepoState[CURRENT_REPO]);


    }


    public bool PostWIP()
    {
        bool result = false;

        
        string zipname = @"c:\temp\dambuddy2\togo-" + GetCurrentRepository() + ".zip";
        //try
        {
            //string directory = gCacheDir + "\\" + DAM_FOLDER;
            string directory = m_ticketBaseFolder + WIP;

            if (File.Exists(zipname))
            {
                File.Delete(zipname);
            }

            ZipFile.CreateFromDirectory(directory, zipname);

            //Directory.Move(directory, directory + "-posted");

            long length = new System.IO.FileInfo(zipname).Length;
            Console.WriteLine("\nSending file length: {0}", length);

            string damfolder = GetCurrentRepositoryFolder();

            using (WebClient client = new WebClient())
            {
                byte[] responseArray = client.UploadFile(gServerName + ":" + DAM_UPLOAD_PORT + "/upload," + damfolder, "POST", zipname);
                // Decode and display the response.
                Console.WriteLine("\nResponse Received. The contents of the file uploaded are:\n{0}",
                    System.Text.Encoding.ASCII.GetString(responseArray));
            }
            result = true;
        }

        return result;
    }

    public string PrepareForUpload()
    {
        string folder = m_dictRepoState[m_dictRepoState[CURRENT_REPO]];
        // TODO: move to config
        
        // TODO: start timer to check status

        return $"http://ckcm.healthy.bewell.ca:10081/init,{folder},VGhpcyBpcyB0aGUgSW1wbGVtZW50YXRpb24gTm90ZQ==,am9uLmJlZWJ5,UGE1NXdvcmQ=";


    }

    public void Closedown()
    {
        SaveExistingWip();
        SaveepositoryState();
    }

    private void SaveExistingWip()
    {
        string csv = "";
        foreach (KeyValuePair<string, string> kvp in m_dictWIPName2Path)
        {
            csv += kvp.Key;
            csv += ",";
            csv += kvp.Value;
            csv += "\n"; //newline to represent new pair
        }

        File.WriteAllText(m_ticketBaseFolder +  @"\WIP.csv", csv);


        csv = "";
        foreach (KeyValuePair<string, string> kvp in m_dictID2Gitpath)
        {
            csv += kvp.Key;
            csv += ",";
            csv += kvp.Value;
            csv += "\n"; //newline to represent new pair
        }

        File.WriteAllText(m_ticketBaseFolder + @"\ID2Gitpath.csv", csv);


        csv = "";
        foreach (KeyValuePair<string, string> kvp in m_dictWIPID2Path)
        {
            csv += kvp.Key;
            csv += ", ";
            csv += kvp.Value;
            csv += "\n"; //newline to represent new pair
        }

        File.WriteAllText(m_ticketBaseFolder + @"\WIPID.csv", csv);
        File.WriteAllText(m_ticketBaseFolder + @"\ReadyState.txt", m_ReadyStateSetByUser.ToString());
    }

    public void DisplayWIP(string filename)//, string originalpath)
    {
        if (m_callbackDisplayWIP != null)
        {
            m_callbackDisplayWIP(filename);//, originalpath);
        }
    }

    public void LoadExistingWIP()
    {

        string filepath = m_ticketBaseFolder  + @"\WIP.csv";
        if (File.Exists(filepath))
        {
            var reader = new StreamReader(File.OpenRead(filepath));

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == "") break;
                var values = line.Split(',');
                string filename = values[0];
                string wippath = values[1];
                m_dictWIPName2Path.Add(filename, wippath);
                DisplayWIP(filename);//, originalpath);

                string filepathWIP = m_ticketBaseFolder + WIP + @"\" + filename;

                CompareWIP2Initial( filepathWIP); // to ensure tracking of modifications
            }
        }


        filepath = m_ticketBaseFolder  + @"\ID2Gitpath.csv";
        if (File.Exists(filepath))
        {
            var reader = new StreamReader(File.OpenRead(filepath));

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == "") break;
                var values = line.Split(',');

                m_dictID2Gitpath.Add(values[0], values[1]);
            }
        }

        filepath = m_ticketBaseFolder  + @"\WIPID.csv";
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

        filepath = m_ticketBaseFolder  + @"\ReadyState.txt";

        if (File.Exists(filepath))
        {
            var reader = new StreamReader(File.OpenRead(filepath));

            var line = reader.ReadLine();
            if( line == "True" ) { m_ReadyStateSetByUser = true; }
            SetTicketReadiness(m_ReadyStateSetByUser);
            m_callbackTicketState?.Invoke(line);
        }

    }


    public bool RemoveWIP(string filename)//, string gitpath)
    {
        string wipFile = m_ticketBaseFolder + @"\" + WIP + @"\" + filename;

        if (!File.Exists(wipFile))
        {
            Console.WriteLine($"RemoveWIP() : {wipFile} doesn't exist");
            return false;
        }

        // if modified message the user

        // delete file in WIP
        string sTID = Utility.GetTemplateID(wipFile);
        string gitpath = m_dictID2Gitpath[sTID];
        string initialFile = m_ticketBaseFolder + @"\" + GITKEEP_INITIAL + @"\" + filename + GITKEEP_SUFFIX;
        string updateFile = m_ticketBaseFolder + @"\" + GITKEEP_UPDATE + @"\" + filename + GITKEEP_SUFFIX;
        string trashDir = m_ticketBaseFolder + KEEP_TRASH;
        try
        {
            WIPRemoveFromServer(filename, GetTemplateID(filename));

                m_dictWIPName2Path.Remove(filename);
                m_dictWIPName2Path.Remove(sTID);
                m_dictWIPID2Path.Remove(sTID);
                m_dictID2Gitpath.Remove(sTID);


                if( File.Exists(wipFile))
                {
                    if (!File.Exists(trashDir))
                        Directory.CreateDirectory(trashDir);

                    if (File.Exists( trashDir + "\\" + filename + ".old"))
                        File.Delete(trashDir + "\\" + filename + ".old");

                    File.Move(wipFile, trashDir + "\\" + filename + ".old");
                }
                if (File.Exists(wipFile + ".md5"))
                {
                    File.Delete(wipFile + ".md5");
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

            if (m_callbackRemoveWIP != null)
            {
                m_callbackRemoveWIP(filename);
            }

            SaveExistingWip();
        }
        catch (Exception e)
        {
            Console.WriteLine( "RemoveWIP() : " + e.Message);
        }

        return true;
    }

    private void CompareWIP2Initial ( string filepath )
    {

        string asset = Path.GetFileName(filepath);

        string initialasset = m_ticketBaseFolder + GITKEEP_INITIAL + @"\" + asset + GITKEEP_SUFFIX;
        byte[] WIPHashBytes = new byte[16];

        string WIPHashHex = "";

        string wipContents = ReadAsset(filepath);
        string initialContents = ReadAsset(initialasset);

        if (string.IsNullOrEmpty(initialContents))
            return;

        if (String.IsNullOrEmpty(wipContents))
            return;

        if (!wipContents.Equals(initialContents))
        {

            m_callbackModifiedWIP?.Invoke(asset);

        }

    }

    private void OnChangedWIP(object source, FileSystemEventArgs e)
    {
        Console.WriteLine($"OnChangedWIP File: {e.FullPath} {e.ChangeType}");
        CompareWIP2Initial(e.FullPath);
        
        // should check whether the md5 of the file is actually different to the initial md5
    }

    private void OnChangedRepo(object source, FileSystemEventArgs e)
    {
        Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");

        if (isAssetinWIP(e.Name))
        {
            UpdateOnWIP(e.FullPath);
        }
    }

    public class TicketScheduleState
    {
        public string UploadEnabled { get; set; }
        public string ScheduleState { get; set; }
    }


    public class TicketChangeState
    {
        public bool active{ get; set; }
        public bool uploading { get; set; }
        public bool ready { get; set; }

    }

    public void TestJira(string ticket)
    {
        JiraService.GetJiraIssue(ticket);
    }

}
