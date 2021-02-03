using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DAMBuddy2;
using System.Net;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using System.Configuration;

public static class Extensions
{
    /// <summary>
    /// Used to help with the paging of the repository views
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
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

/// <summary>
/// Manages the RepoInstances, one per configured ticket. Instances are loaded on demand. 
/// Also instantiates RepoCacheManager to manage ticket folders and git initial clone.
/// </summary>
public class RepoManager
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private static string CURRENT_REPO = "CURRENT_REPO";

    private string m_GitRepositoryURI = "https://github.com/ahs-ckm/ckm-mirror";
    private RepoCacheManager mRepoCacheManager;
    private Thread mThreadTidyRepository;
    private int m_intervalPull;
    private int m_pullDelay;
    private string m_CacheServiceURL;
    private List<RepoInstance> mRepoInstanceList;

    RepoCallbackSettings mRepoInstancCallbacks;
    private Dictionary<string, string> m_dictRepoState;
    private System.Threading.Timer m_timerMonitorTicketState;

    /// <summary>
    /// Helper to allow quick access to the RepoInstance for the current/active repository.
    /// </summary>
    public RepoInstance CurrentRepo
    {
        get => GetInstanceUnsafe(m_dictRepoState[CURRENT_REPO]);
    }

    public string CurrentRepoServerFolder
    {
        get => m_dictRepoState[m_dictRepoState[CURRENT_REPO]];
    }
            
    /// <summary>
    /// Represents the server side state of a ticket (drawn from change table)
    /// </summary>
    public class TicketChangeState
    {
        public bool active { get; set; }
        public bool uploading { get; set; }
        public bool ready { get; set; }

    }

    /// <summary>
    /// Constructor which loads persisted state from disk and then instantiates a RepoInstance for the 
    /// current/active repository (as of last state save).
    /// </summary>
    /// <param name="callbacks"></param>
    public RepoManager( RepoCallbackSettings callbacks )
    {
        var appsettings = ConfigurationManager.AppSettings;

        m_CacheServiceURL = appsettings["CacheServiceUrl"] ?? "App Settings not found";

        mRepoInstanceList = new List<RepoInstance>();

        m_CacheServiceURL = Utility.GetSettingString("CacheServiceUrl");

        m_intervalPull = Utility.GetSettingInt("GitPullInterval");
        m_pullDelay = Utility.GetSettingInt("GitPullDelay");

        mRepoInstancCallbacks = callbacks;
        
        LoadRepositoryState();


        mThreadTidyRepository = new Thread(TidyRepositoryState);
        mThreadTidyRepository.Priority = ThreadPriority.Lowest;
        mThreadTidyRepository.Start();

        string sTicketID = m_dictRepoState[CURRENT_REPO];
        if ( sTicketID == "") return;


        string sFolderID = m_dictRepoState[sTicketID];

        RepoInstance instance = null;

        if( !GetInstanceSafe(sTicketID, ref instance) )
        {
            instance = CreateRepoInstance(sTicketID, sFolderID, true);
        }
        else
        {
            instance.MakeActive();
        }

    }

    private void TidyRepositoryState() // verify that we need each folder. If not in dictRepoState, remove it.
    {
        List<string> listToGo = new List<string>();

        try
        {
            string sRoot = Utility.GetSettingString("FolderRoot");
            string[] ticketFolder = Directory.GetDirectories(sRoot,"*", SearchOption.TopDirectoryOnly);
            foreach (string folderpath in ticketFolder)
            {
                if (folderpath.Contains(Utility.GetSettingString("RepoCacheToken"))) continue;
                if (folderpath.Contains(Utility.GetSettingString("KeepTrash"))) continue;



                string sFolderName = Path.GetFileName(folderpath);

                if (!m_dictRepoState.ContainsKey(sFolderName) )
                {
                    var sUniqueSuffix = Guid.NewGuid();
                    string sNewName = folderpath;
                    if ( !sFolderName.Contains( "TODELETE") )
                    {
                        sNewName = folderpath + "-TODELETE-" + sUniqueSuffix.ToString().Substring(0, 7);
                        try
                        {

                            Directory.Move(folderpath, sNewName);
                        }
                        catch (Exception e)
                        {
                            Logger.LogException(NLog.LogLevel.Warn, $"Problems moving old ticket ({folderpath} -> {sNewName}", e);
                        }

                    }
                    listToGo.Add(sNewName);

                }
            }

            foreach(string pathToDelete in listToGo)
            { 
                try
                {
                    Utility.MakeAllWritable(pathToDelete);
                    Directory.Delete(pathToDelete, true);
                }
                catch ( Exception e) 
                {
                    Logger.LogException(NLog.LogLevel.Warn, $"Problems when trying to delete old ticket folder {pathToDelete}", e);
                }

            }

        }
        finally
        {
        }

    }


    /// <summary>
    /// Intializes the RepoManager, starting a process of checking the upload state of each repository/ticket. 
    /// Tickets may have been uploaded after the RepoManager was last closed, so any prior uploaded tickets will be removed.
    /// </summary>
    /// <param name="PullDelay"></param>
    /// <param name="PullInterval"></param>
    public void Init()//int PullDelay, int PullInterval)
    {
        string sRoot = Utility.GetSettingString("FolderRoot");
        string sBinDir = Utility.GetSettingString("BinDir");


        if (!Directory.Exists("c:\\temp")) Directory.CreateDirectory("c:\\temp");
        if (!Directory.Exists("c:\\temp\\dambuddy2")) Directory.CreateDirectory("c:\\temp\\dambuddy2");

        mRepoCacheManager = new RepoCacheManager(sRoot, 3, m_GitRepositoryURI, sBinDir, mRepoInstancCallbacks.callbackInfo);

        List<string> processlist = new List<string>();

        foreach (string ticket in m_dictRepoState.Keys)
        {
            processlist.Add(ticket);
        }

        // check each ticket is still active on the server?
        foreach (string ticket in processlist)
        {
            if (ticket != CURRENT_REPO)
            {
                ProcessTicketState(ticket);
            }
        }

    }


    /// <summary>
    /// Retrieves the repository list state from disk, which contains all the configured ticket/repos and 
    /// and indicator to say which is active/current.
    /// </summary>
    private void LoadRepositoryState()
    {
        m_dictRepoState = new Dictionary<string, string>();
        string sRoot = Utility.GetSettingString("FolderRoot");

        if( !Directory.Exists( sRoot ))
        {
            Directory.CreateDirectory(sRoot);
        }


        string filepath = sRoot + @"\" + "repostate.csv";
        if (!File.Exists(filepath))
        {
            // create if doesn't already exist
            var stream = File.Create(filepath);
            string token = CURRENT_REPO + ",";
            stream.Write(System.Text.Encoding.ASCII.GetBytes(token), 0, token.Length);
            stream.Close();

        }

        var reader = new StreamReader(File.OpenRead(filepath));

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line == "") break;
            var values = line.Split(',');

            m_dictRepoState.Add(values[0], values[1]);
        }

        // check that all the repo directories actually exist
        // if the app wasn't closed cleanly, then the state may be inaccurate.

        for (int i = m_dictRepoState.Count - 1;  i >= 0; i--)
        {
            var item = m_dictRepoState.ElementAt(i);
            string sTicketFolder = item.Key;

            if (sTicketFolder == CURRENT_REPO) continue; // ignore token

            if(!Directory.Exists( sRoot + "\\" + sTicketFolder ))
            {
                if (m_dictRepoState[CURRENT_REPO] == sTicketFolder) m_dictRepoState[CURRENT_REPO] = "";
                m_dictRepoState.Remove(sTicketFolder);
            }


        }

    }

    /// <summary>
    /// Writes the repository list state to disk, which contains all the configured ticket/repos and 
    /// and indicator to say which is active/current.
    /// </summary>
    private void SaveepositoryState()
    {
        string sRoot = Utility.GetSettingString("FolderRoot");

        string filepath = sRoot + @"\" + "repostate.csv";
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

    /// <summary>
    /// Watchdog process to check whether a ticket has finished uploading, and if so, close/remove it.
    /// </summary>
    /// <param name="sTicketID"></param>
    /// 
    private void ProcessTicketState(Object info)
    {
        string sTicketID = (string)info;
        TicketChangeState state = GetTicketUploadState(sTicketID);
        if (!state.active)
        {

            RemoveTicket(sTicketID);
            //TODO: is this needed - if we close a ticket whilst the UI has it open - like after an upload finishes?
            mRepoInstancCallbacks.callbackUploadState?.Invoke(sTicketID, state);
        }

    }


    private void ThreadProcessTicketState(Object info )
    {
        string sTicketID = (string)info;
        string sFolder;

        if (!m_dictRepoState.TryGetValue(sTicketID, out sFolder))
        {

            if (m_timerMonitorTicketState != null)
            {
                m_timerMonitorTicketState.Dispose();
                m_timerMonitorTicketState = null;
            }
            return;
        }


        ProcessTicketState(info);
    }

    /// <summary>
    /// Retrieves the ticket state from the server
    /// </summary>
    /// <param name="sTicketID"></param>
    /// <returns></returns>
    private TicketChangeState GetTicketUploadState(string sTicketID)
    {
        string Server = Utility.GetSettingString("ServerName");
        string Port = Utility.GetSettingString("DAMUploadPort");
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{Server}:{Port}/dynamic/change_status,{sTicketID}");
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



    private RepoInstance GetInstanceUnsafe(string sTicketID)
    {

        foreach (var instance in mRepoInstanceList)
        {
            if (instance.TicketID == sTicketID)
            {
                return instance;
            }
        }

        return null;
    }


    /// <summary>
    /// Helper function to find and return the RepoInstance corresponding to the passed in ticket
    /// </summary>
    /// <param name="sTicketID"></param>
    /// <returns></returns>
    private bool GetInstanceSafe( string sTicketID, ref RepoInstance theInstance)
    {
        
        foreach (var instance in mRepoInstanceList)
        {
            if (instance.TicketID == sTicketID) 
            {
                theInstance = instance;
                return true;
            }
        }

        return false;
    }
    /// <summary>
    /// Removes from the RepoState the RepoInstance corresponding to the ticket parameter 
    /// </summary>
    /// <param name="sTicketID"></param>



    private bool BackupTicket(string sPath)
    {
        if( Directory.Exists( sPath))
        {
           // Directory.Move(sPath, sPath + "-DELETEME");
        }

        // TODO: zip up to backup folder
        //MessageBox.Show("Backing up :" + sPath );
        return true;
    }

    private void MoveToTrash( string sTicketID )
    {
        RepoInstance repo = null;
        string sRoot = Utility.GetSettingString("FolderRoot");

        string sTrashFolder = sRoot + "\\" + Utility.GetSettingString("KeepTrash");


        if ( GetInstanceSafe(sTicketID, ref repo))
        {
            if( !Directory.Exists( sTrashFolder))
            {
                Directory.CreateDirectory(sTrashFolder);
            }

            repo.Shutdown();

            string sTicketFolder = sRoot + "\\" + sTicketID;
            try
            {
                Directory.Move(sTicketFolder, sTrashFolder + "\\" + sTicketID);

            }
            catch            
            {

            }

        }

    }

    public void RemoveTicket(string sTicketID)
    {

        string sFolder;

        if( !m_dictRepoState.TryGetValue(sTicketID, out sFolder))
        {
            return;
        }

        //string sFolder = m_dictRepoState[sTicketID];

        if (m_dictRepoState[CURRENT_REPO] == sTicketID)
        {
            if (m_timerMonitorTicketState != null)
            {
                m_timerMonitorTicketState.Dispose();
                m_timerMonitorTicketState = null;
            }
        }
        string sRoot = Utility.GetSettingString("FolderRoot");

        string path = sRoot + "\\" + sTicketID;

        if (BackupTicket( path) )
        {

            string Server = Utility.GetSettingString("ServerName");
            RepoInstance.CloseTicketOnServer(sTicketID, sFolder, Server);
            mRepoInstanceList.Remove(GetInstanceUnsafe(sTicketID));
            m_dictRepoState.Remove(sTicketID);

            if (m_dictRepoState[CURRENT_REPO] == sTicketID)
            {
                m_dictRepoState[CURRENT_REPO] = "";
            }
        }

    }

    /// <summary>
    /// Retruns a list of tickets, each representing a configured repository in the RepoState
    /// </summary>
    /// <returns></returns>
    public List<string> GetAvailableRepositories()
    {
        List<string> repos = new List<string>();

        foreach (var item in m_dictRepoState.Keys)
        {
            if (item == CURRENT_REPO) continue;
            repos.Add(item);

        }
        return repos;
    }

    /// <summary>
    /// Creates a RepoInstance and configures ticket/folder info, git parameters and callbacks for UI notifcations. 
    /// </summary>
    /// <param name="sTicketID"></param>
    /// <param name="sFolderID"></param>
    /// <param name="isCurrent"></param>
    /// <returns></returns>
    private RepoInstance CreateRepoInstance( string sTicketID, string sFolderID, bool isCurrent )
    {

        RepoInstanceConfig config = new RepoInstanceConfig();
        config.TicketID = sTicketID;
        config.FolderID = sFolderID;
        string sRoot = Utility.GetSettingString("FolderRoot");

        string Server = Utility.GetSettingString("ServerName");
        config.URLServer = Server;
        config.URLCache = m_CacheServiceURL;
        config.GitPullInitialDelay = m_pullDelay;
        config.GitPullInterval = m_intervalPull;
        config.BaseFolder = sRoot + "\\" + sTicketID ;
        config.isActive = isCurrent;

        var instance = new RepoInstance(config, mRepoInstancCallbacks);
        
        mRepoInstanceList.Add(instance);
        instance.SetTicketReadiness(false);
        
        return instance;
    }
    /// <summary>
    /// Switches to, and if needed creates, the current RepoInstance to correspond to the ticket parameter
    /// </summary>
    /// <param name="sTicketID"></param>
    /// <returns></returns>
    public bool SetCurrentRepository(string sTicketID)
    {

        if (m_dictRepoState[CURRENT_REPO] == sTicketID) return true; // already current

        RepoInstance oldInstance = null; 
            
        if( GetInstanceSafe(m_dictRepoState[CURRENT_REPO], ref oldInstance))
        {

            // tell current repo instance that it's not current
            oldInstance.MakeInactive();

        }


        RepoInstance instance = GetInstanceUnsafe(sTicketID);


        if (instance == null )
        {
            // check if repo instance exists, if not create it 
            string sFolderID = m_dictRepoState[sTicketID];
            instance = CreateRepoInstance(sTicketID, sFolderID, true);
        }
        else
        {
            // tell new repo instance that it is now current.
            instance.MakeActive();
        }

        m_dictRepoState[CURRENT_REPO] = sTicketID;

        return true;
    }

    /// <summary>
    /// Orchestrates the creation of a new Repository for a given ticket (represented by the json object parameter).
    /// </summary>
    /// <param name="jsonTicket"></param>
    /// <returns></returns>
    public bool PrepareNewTicket( string jsonTicket )
    {
        // set it up on the server
        // get the server folder back?

        // create directory structure
        // move pre-prepared git folder

        // switch context.
        if (jsonTicket == null) return false;

        JObject jsonissue = JObject.Parse(jsonTicket);
        string ticketID = (string)jsonissue["key"];
        string sRoot = Utility.GetSettingString("FolderRoot");

        string assignee = (string)jsonissue["fields"]["assignee"]["displayName"];
        string description = (string)jsonissue["fields"]["description"];
        string email = (string)jsonissue["fields"]["assignee"]["emailAddress"];



        if (mRepoCacheManager.SetupTicket(sRoot + "\\" + ticketID))
        {

            string FolderID = ServerLinkTicket(jsonissue);

            CreateRepoInstance(ticketID, FolderID, true);

            //m_dictRepoDescription.Add(ticketID, description);
            m_dictRepoState.Add(ticketID, FolderID);
            m_dictRepoState[CURRENT_REPO] = ticketID;

            Console.WriteLine($"Current Repository is now {CurrentRepo}");
            SaveepositoryState();
            
            return true;
        };

        return false;
    }

    /// <summary>
    /// Calls the server to link/allocate the ticket to a folder on the server. Largely to ensure backwards compatibility with existing DAM otols.
    /// The server returns the FolderID.
    /// </summary>
    /// <param name="theIssue"></param>
    /// <returns>the ID of the associated server folder</returns>
    public string ServerLinkTicket(JObject theIssue)
    {
        string sFolderName = "";

        try
        {
            string sTicketID = (string)theIssue["key"];
            string sDescription = (string)theIssue["fields"]["description"];
            string sLead = Utility.GetSettingString("User");
            string sAssignee = (string)theIssue["fields"]["assignee"]["displayName"];
            string theParams = $"theTicket={sTicketID}&theDescription={sDescription}&theLead={sLead}&theAssignee={sAssignee}";

            using (WebClient client = new WebClient())
            {
                string Port = Utility.GetSettingString("DAMUploadPort");
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                string Server = Utility.GetSettingString("ServerName");
                sFolderName = client.UploadString(Server + ":" + Port + "/linkTicket", theParams);
                Console.WriteLine(sFolderName);

            }

        }
        catch ( Exception e )
        {
            Logger.LogException(NLog.LogLevel.Error, "Problems occured when trying to link the ticket.", e);
        }

        return sFolderName;

    }

    /// <summary>
    /// Provides the endpoint to call for the upload process and also starts a watchdog to 
    /// monitor ticket upload state (and eventually close the associated ticket, if upload completes successfully).
    /// </summary>
    /// <returns></returns>
    public string PrepareForUpload()
    {
        string ticket = m_dictRepoState[CURRENT_REPO];
        string folder = m_dictRepoState[ticket];
     
        m_timerMonitorTicketState = new System.Threading.Timer( ThreadProcessTicketState, ticket, 5000,5000);

        string UploadUrl = Utility.GetSettingString("DAMUploadUrl");
        string user = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Utility.GetSettingString("User")));
        string pw = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Utility.GetSettingString("Password")));
        string message = "hardcoded change message";
        string ChangeMessage = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message));

        string sFullURL = UploadUrl + folder + "," + ChangeMessage + "," + user + "," + pw;


        return sFullURL;
    }


    /// <summary>
    /// Prepares instances for destructions - time to save state to disk and quietly manage any running threads
    /// </summary>
    public void Shutdown()
    {

        if (mThreadTidyRepository != null)
        {
            try
            {
                mThreadTidyRepository.Abort();
            }
            catch
            { }
        }

        if ( m_timerMonitorTicketState != null)
        {
            m_timerMonitorTicketState.Dispose();


        }

        mRepoCacheManager.Shutdown();

        foreach( var instance in mRepoInstanceList)
        {
            instance.Shutdown();
        }

        SaveepositoryState();
        mRepoCacheManager.Shutdown();
    }


}
