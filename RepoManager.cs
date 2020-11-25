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
    private List<RepoInstance> mRepoInstanceList;

    RepoCallbackSettings mRepoInstancCallbacks;

    bool m_ReadyStateSetByUser = false;

    private static Dictionary<string, string> m_dictFileToPath;
    private Dictionary<string, string> m_dictRepoState;

    public RepoInstance CurrentRepo
    {
        get => GetInstance(m_dictRepoState[CURRENT_REPO]);
    }



        

    public class TicketChangeState
    {
        public bool active { get; set; }
        public bool uploading { get; set; }
        public bool ready { get; set; }

    }


    public RepoManager( RepoCallbackSettings callbacks )
    {

        mRepoInstanceList = new List<RepoInstance>();


        mRepoInstancCallbacks = callbacks;
        
        LoadRepositoryState();
        string sTicketID = m_dictRepoState[CURRENT_REPO];
        string sFolderID = m_dictRepoState[sTicketID];

        var instance = GetInstance(sTicketID);
        if( instance == null )
        {
            instance = CreateRepoInstance(sTicketID, sFolderID);
        }

        instance.MakeActive();
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
            //TODO: is this needed - if we close a ticket whilst the UI has it open - like after an upload finishes?
            //CallbackUploadState?.Invoke(sTicketID, state);
        }

    }


    private TicketChangeState GetTicketUploadState(string sTicketID)
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

    private RepoInstance GetInstance( string sTicketID)
    {
        
        foreach (var instance in mRepoInstanceList)
        {
            if (instance.TicketID == sTicketID) { return instance; }
        }

        return null;
    }

    private void RemoveTicket(string sTicketID)
    {
        if (GetInstance(sTicketID).Remove())
        {
            m_dictRepoState.Remove(sTicketID);
            if (m_dictRepoState[CURRENT_REPO] == sTicketID) { m_dictRepoState[CURRENT_REPO] = ""; }
        }
    
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

    private RepoInstance CreateRepoInstance( string sTicketID, string sFolderID )
    {

        RepoInstanceConfig config = new RepoInstanceConfig();
        config.TicketID = sTicketID;
        config.FolderID = sFolderID;
        config.ServerURL = gServerName;
        config.GitPullInitialDelay = m_pullDelay;
        config.GitPullInterval = m_intervalPull;
        config.BaseFolder = FOLDER_ROOT + "\\" + sTicketID ;

        var instance = new RepoInstance(config, mRepoInstancCallbacks);
        
        mRepoInstanceList.Add(instance);
        
        //instance.Init();

        return instance;
    }

    public bool SetCurrentRepository(string sTicketID)
    {

        var oldInstance = GetInstance(m_dictRepoState[CURRENT_REPO]);
      
        // tell current repo instance that it's not current
        oldInstance.MakeInactive();
        
        
        RepoInstance instance = GetInstance(sTicketID);


        if (instance == null )
        {
            // check if repo instance exists, if not create it 
            string sFolderID = m_dictRepoState[sTicketID];
            instance = CreateRepoInstance(sTicketID, sFolderID);
        }

        // tell new repo instance that it is now current.
        instance.MakeActive();

        m_dictRepoState[CURRENT_REPO] = sTicketID;

        return true;
    }

    
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
            CreateRepoInstance(ticketID, FolderID);

            m_dictRepoState.Add(ticketID, FolderID);
            return true;
        };

        return false;
    }


    public string ServerLinkTicket(JObject theIssue)
    {
        string sFolderName = "";
        string sTicketID = (string)theIssue["key"];
        string sDescription = (string)theIssue["fields"]["description"];

        /*
        if (theIssue.isNull("assignee") == false)
        {
            JSONObject assigneeObj = fieldsObj.getJSONObject("assignee");
            ji.setAssignee(assigneeObj.getString("name"));
            ji.setAssigneeemail(assigneeObj.getString("emailAddress"));
        }*/


        string theParams = $"theTicket={sTicketID}";

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


    public void ApplyFilter(string filtertext)
    {

        return;
    }



    public static string ReadAsset(string filepath)
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



    public static void MakeMd5(string filepath)
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

   

    public void Init(int PullDelay, int PullInterval)
    {
        mRepoCacheManager = new RepoCacheManager(FOLDER_ROOT, 3, m_GitRepositoryURI, BIN_DIR);
        
        m_intervalPull = PullInterval;
        m_pullDelay = PullDelay;

        List<string> processlist = new List<string>();

        foreach ( string ticket in m_dictRepoState.Keys)
        {
            processlist.Add(ticket);         
        }

        // check each ticket is still active on the server?
        foreach( string ticket in processlist )
        {
            if (ticket != CURRENT_REPO)
            {
                ProcessTicketState(ticket);
            }
        }

    }


    public string PrepareForUpload()
    {
        string folder = m_dictRepoState[m_dictRepoState[CURRENT_REPO]];
        // TODO: move to config
        
        // TODO: start timer to check status

        return $"http://ckcm.healthy.bewell.ca:10081/init,{folder},VGhpcyBpcyB0aGUgSW1wbGVtZW50YXRpb24gTm90ZQ==,am9uLmJlZWJ5,UGE1NXdvcmQ=";


    }

    public void Shutdown()
    {
        
        foreach( var instance in mRepoInstanceList)
        {
            instance.Shutdown();
        }

        SaveepositoryState();
        mRepoCacheManager.CloseDown();
    }

    public void TestCacheManager()
    {
        //mRepoCacheManager = new RepoCacheManager(FOLDER_ROOT, 3, m_GitRepositoryURI, BIN_DIR);
    }


}
