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
    
    private static string DAM_UPLOAD_PORT = "10091"; // DEV
    private static string DAM_SCHEDULER_PORT = "10008";

    private static string CURRENT_REPO = "CURRENT_REPO";
    private static string FOLDER_ROOT = @"c:\TD";

    private static string BIN_DIR = @"C:\Users\jonbeeby\source\repos\DamBuddy2\packages\PortableGit\bin\";


    private string m_GitRepositoryURI = "https://github.com/ahs-ckm/ckm-mirror";
    public static string GITKEEP_INITIAL = @"\gitkeep\initial";
    public static string GITKEEP_UPDATE = @"\gitkeep\update";
    public static string KEEP_TRASH = @"\trash";
    private static string GITKEEP_SUFFIX = ".keep";
    //private static string WIP = @"\local\WIP";
    public static string ASSETS = @"\local";
    public static string WIP = @"\" + ASSETS + @"\WIP";


    private string gServerName = "http://ckcm.healthy.bewell.ca";


    private RepoCacheManager mRepoCacheManager;



    private int m_intervalPull = 5000;
    private int m_pullDelay;
    private string m_CacheServiceURL;
    private List<RepoInstance> mRepoInstanceList;

    RepoCallbackSettings mRepoInstancCallbacks;

    bool m_ReadyStateSetByUser = false;

    private static Dictionary<string, string> m_dictFileToPath;
    private Dictionary<string, string> m_dictRepoState;

    /// <summary>
    /// Helper to allow quick access to the RepoInstance for the current/active repository.
    /// </summary>
    public RepoInstance CurrentRepo
    {
        get => GetInstance(m_dictRepoState[CURRENT_REPO]);
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


        mRepoInstancCallbacks = callbacks;
        
        LoadRepositoryState();
        string sTicketID = m_dictRepoState[CURRENT_REPO];
        string sFolderID = m_dictRepoState[sTicketID];

        var instance = GetInstance(sTicketID);
        if( instance == null )
        {
            instance = CreateRepoInstance(sTicketID, sFolderID, true);
        }
        else
        {
            instance.MakeActive();
        }

    }


/// <summary>
/// Intializes the RepoManager, starting a process of checking the upload state of each repository/ticket. 
/// Tickets may have been uploaded after the RepoManager was last closed, so any prior uploaded tickets will be removed.
/// </summary>
/// <param name="PullDelay"></param>
/// <param name="PullInterval"></param>
    public void Init(int PullDelay, int PullInterval)
    {
        mRepoCacheManager = new RepoCacheManager(FOLDER_ROOT, 3, m_GitRepositoryURI, BIN_DIR);

        m_intervalPull = PullInterval;
        m_pullDelay = PullDelay;

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

    /// <summary>
    /// Writes the repository list state to disk, which contains all the configured ticket/repos and 
    /// and indicator to say which is active/current.
    /// </summary>
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

    /// <summary>
    /// Watchdog process to check whether a ticket has finished uploading, and if so, close/remove it.
    /// </summary>
    /// <param name="sTicketID"></param>
    private void ProcessTicketState( string sTicketID )
    {
        TicketChangeState state = GetTicketUploadState(sTicketID);
        if( !state.active )
        {
            RemoveTicket(sTicketID);
            //TODO: is this needed - if we close a ticket whilst the UI has it open - like after an upload finishes?
            //CallbackUploadState?.Invoke(sTicketID, state);
        }

    }

    /// <summary>
    /// Retrieves the ticket state from the server
    /// </summary>
    /// <param name="sTicketID"></param>
    /// <returns></returns>
    private TicketChangeState GetTicketUploadState(string sTicketID)
    {

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{gServerName}:{DAM_UPLOAD_PORT}/dynamic/change_status,{CurrentRepo.TicketID}");
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

    /// <summary>
    /// Helper function to find and return the RepoInstance corresponding to the passed in ticket
    /// </summary>
    /// <param name="sTicketID"></param>
    /// <returns></returns>
    private RepoInstance GetInstance( string sTicketID)
    {
        
        foreach (var instance in mRepoInstanceList)
        {
            if (instance.TicketID == sTicketID) { return instance; }
        }

        return null;
    }
    /// <summary>
    /// Removes from the RepoState the RepoInstance corresponding to the ticket parameter 
    /// </summary>
    /// <param name="sTicketID"></param>
    private void RemoveTicket(string sTicketID)
    {
        if (GetInstance(sTicketID).Remove())
        {
            m_dictRepoState.Remove(sTicketID);
            if (m_dictRepoState[CURRENT_REPO] == sTicketID) { m_dictRepoState[CURRENT_REPO] = ""; }
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
        config.URLServer = gServerName;
        config.URLCache = m_CacheServiceURL;
        config.GitPullInitialDelay = m_pullDelay;
        config.GitPullInterval = m_intervalPull;
        config.BaseFolder = FOLDER_ROOT + "\\" + sTicketID ;
        config.isActive = isCurrent;

        var instance = new RepoInstance(config, mRepoInstancCallbacks);
        
        mRepoInstanceList.Add(instance);
        
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

        var oldInstance = GetInstance(m_dictRepoState[CURRENT_REPO]);
      
        // tell current repo instance that it's not current
        oldInstance.MakeInactive();
        
        
        RepoInstance instance = GetInstance(sTicketID);


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
        JObject jsonissue = JObject.Parse(jsonTicket);
        string ticketID = (string)jsonissue["key"];
        

        if (mRepoCacheManager.SetupTicket(FOLDER_ROOT + "\\" + ticketID))
        {

            
            string FolderID = ServerLinkTicket(jsonissue);
            CreateRepoInstance(ticketID, FolderID, true);

            m_dictRepoState.Add(ticketID, FolderID);
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
        string sTicketID = (string)theIssue["key"];
        string sDescription = (string)theIssue["fields"]["description"];
        string sName = (string)theIssue["fields"]["name"];
        string sAssignee = (string)theIssue["fields"]["assignee"];
        /*
        if (theIssue.isNull("assignee") == false)
        {
            JSONObject assigneeObj = fieldsObj.getJSONObject("assignee");
            ji.setAssignee(assigneeObj.getString("name"));
            ji.setAssigneeemail(assigneeObj.getString("emailAddress"));
        }*/


       // string theParams = System.Net.WebUtility.HtmlEncode($"theTicket={sTicketID},theDescription={sDescription},theLead={sName},theAssignee={sAssignee}");

        string theParams = System.Net.WebUtility.HtmlEncode($"theTicket={sTicketID},theDescription=abc,theLead=JonBeeby,theAssignee=JonBeeby");


        using (WebClient client = new WebClient())
        {
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            sFolderName = client.UploadString(gServerName + ":" + DAM_UPLOAD_PORT + "/linkTicket", theParams);
            Console.WriteLine(sFolderName);

        }

        //        UpdateSchedule();
        //        GetTicketScheduleStatus();
        return sFolderName;
    }


    /// <summary>
    /// Provides the endpoint to call for the upload process and also starts a watchdog to 
    /// monitor ticket upload state (and eventually close the associated ticket, if upload completes successfully).
    /// </summary>
    /// <returns></returns>
    public string PrepareForUpload()
    {
        string folder = m_dictRepoState[m_dictRepoState[CURRENT_REPO]];
        // TODO: move to config
        
        // TODO: start timer to check status

        return $"http://ckcm.healthy.bewell.ca:10081/init,{folder},VGhpcyBpcyB0aGUgSW1wbGVtZW50YXRpb24gTm90ZQ==,am9uLmJlZWJ5,UGE1NXdvcmQ=";


    }


    /// <summary>
    /// Prepares instances for destructions - time to save state to disk and quietly manage any running threads
    /// </summary>
    public void Shutdown()
    {
        
        foreach( var instance in mRepoInstanceList)
        {
            instance.Shutdown();
        }

        SaveepositoryState();
        mRepoCacheManager.CloseDown();
    }


}
