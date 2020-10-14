using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

public class GitManager
{
	private string m_LocalPath = "";
	private string m_GitRepositoryURI = "https://github.com/ahs-ckm/ckm-mirror";
	private static string GITKEEP_INITIAL = @"\gitkeep\initial";
	private static string GITKEEP_UPDATE = @"\gitkeep\update";
    private static string WIP = @"\WIP";

    private FileSystemWatcher m_watcher = null;

	private int m_intervalPull = 5000;
	private System.Threading.Timer m_timerPull = null;

    private DateTime m_dtCloneStart ;
    private DateTime m_dtCloneEnd ;
    private Dictionary<string, string>  m_dictWIPNameID;

    public delegate void StaleCallback(string filename);
    public delegate void DisplayWIPCallback(string filename, string originalpath);

    StaleCallback m_callbackStale;
    DisplayWIPCallback m_callbackDisplayWIP;

    public GitManager( string localpath, StaleCallback callbackStale, DisplayWIPCallback callbackDisplay)
	{
        m_callbackDisplayWIP = callbackDisplay;
        m_callbackStale = callbackStale;

		m_LocalPath = localpath + @"\mgr";
    }

	public void Pull()
    {
        PullOptions options = new PullOptions();
        options.FetchOptions = new FetchOptions();
        options.MergeOptions = new MergeOptions();

        options.MergeOptions.FailOnConflict = false;
        options.MergeOptions.MergeFileFavor = MergeFileFavor.Theirs;
        options.MergeOptions.FileConflictStrategy = CheckoutFileConflictStrategy.Theirs;
        /*var fetchOptions = new FetchOptions();

        TransferProgress p;
        

        TransferProgressHandler a

        fetchOptions.OnProgress
*/
        //options.FetchOptions.OnProgress = TransferProgress;
        Repository repo = new Repository(m_LocalPath);

        /*FetchOptions fetchoptions = new FetchOptions();
        var remote = repo.Network.Remotes["origin"];
        var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
*/
        //repo.Network.Pull(new Signature("truecraft", "git@truecraft.io", new DateTimeOffset(DateTime.Now)), options);
        try
        {
            //Commands.Fetch(repo, remote.Name, refSpecs, null, "test");
            repo.Reset(ResetMode.Hard, repo.Branches.First(b => b.IsCurrentRepositoryHead).Tip);
            Commands.Pull(repo, new Signature("truecraft", "git@truecraft.io", new DateTimeOffset(DateTime.Now)), options);
        } catch (Exception e )
        {
            Console.WriteLine(e.Message);
        }
   
    }

	public bool Clone()
    {
        m_dtCloneStart = DateTime.Now;

        CloneOptions options = new CloneOptions();
        options.OnTransferProgress = TransferProgress;


        if( !File.Exists(m_LocalPath))
        {
            Directory.CreateDirectory(m_LocalPath);
        }

        Repository.Clone(m_GitRepositoryURI, m_LocalPath, options);

        m_dtCloneEnd = DateTime.Now;

        return true;
    }

	// callback to notify stale state
	public void RegisterStaleCallBack( StaleCallback callback )
    {
        m_callbackStale = callback;
    }

    public void RegisterDisplayWIPCallback( DisplayWIPCallback callback )
    {
        m_callbackDisplayWIP = callback;
    }

    public static bool TransferProgress(TransferProgress progress)
    {
        Console.WriteLine($"Objects: {progress.ReceivedObjects} of {progress.TotalObjects}");
        return true;
    }

    // prepare an asset as WIP
    public void AddWIP( string filepath)
    {
        if (!File.Exists(m_LocalPath + @"\" + GITKEEP_INITIAL) ) 
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
        //C:\TD\git2\1\mgr\WIP
        File.Copy(filepath, m_LocalPath + WIP + @"\" + Path.GetFileName(filepath));

        string initialFile = m_LocalPath + GITKEEP_INITIAL + @"\" + Path.GetFileName(filepath);

        File.Move(filepath, initialFile);

        MakeMd5(initialFile);

        m_dictWIPNameID[Path.GetFileName(filepath)] = "";

        SaveExistingWip();

        //copy filepath to 
    }

    private void SaveInitialState(string filepath)
    {
        // copy to git
        string filename = Path.GetFileName(filepath);
        string gitinitpath = m_LocalPath + @"\" + GITKEEP_INITIAL;
        File.Move(filepath, gitinitpath);

        MakeMd5(gitinitpath);
    }


    public bool isAssetinWIP(string filename)
    {
        string sTID = "";
        bool exists = m_dictWIPNameID.TryGetValue(Path.GetFileName(filename), out sTID);
        return exists;
    }

    private void SaveUpdateState( string filepath )
    {
        // copy to git
        string filename = Path.GetFileName(filepath);
        string gitpath = m_LocalPath + @"\" + GITKEEP_UPDATE;
        string gitkeepfile = gitpath + @"\" + filename;
        if ( File.Exists(gitkeepfile))
        {
            File.Delete(gitkeepfile);
        }
        File.Move(filepath, gitkeepfile);

        MakeMd5(gitkeepfile);
    }


    private bool IsStale( string asset )
    {
        string WIPHash = File.ReadAllText(m_LocalPath + GITKEEP_INITIAL + @"\" + asset + ".md5");

        string UpdateHash = File.ReadAllText(m_LocalPath + GITKEEP_UPDATE + @"\" + asset + ".md5");

        if( String.Compare(WIPHash, UpdateHash) == 0 )
        {
            return false;
        }

        return true;
    }

	// called when an asset is 
	public void UpdateOnWIP( string filepath)
    {
        // move file to gitupdate
        SaveUpdateState(filepath);
        if( IsStale(Path.GetFileName(filepath)))
        {
            if (m_callbackStale != null )
            {
                m_callbackStale(Path.GetFileName(filepath));
            }
            //MessageBox.Show("Stale Asset " + Path.GetFileName(filepath));
        }
        // 
    }

	private void TimeToPull( Object info)
    {
        Console.WriteLine("TimeToPull()");
        Pull();
    }

    private void PackAsset( string filepath )
    {
        string contents = File.ReadAllText(filepath);
        string packed = System.Text.RegularExpressions.Regex.Replace(contents, @"\s+", String.Empty);
        packed = System.Text.RegularExpressions.Regex.Replace(packed, @"\s\n", String.Empty);


        File.WriteAllText(filepath, packed);

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

        m_dictWIPNameID = new Dictionary<string, string>();
        
        m_watcher = new FileSystemWatcher();
        m_watcher.Path = m_LocalPath + @"\local\templates";
        m_watcher.IncludeSubdirectories = true;
        m_watcher.NotifyFilter = NotifyFilters.LastWrite;
        m_watcher.Filter = "*.oet";
        m_watcher.Created += OnChanged;
        m_watcher.Changed += OnChanged;

        m_watcher.EnableRaisingEvents = true;

        LoadExistingWIP();

    }

    public void SaveExistingWip()
    {
        string csv = "";
        foreach (KeyValuePair<string, string> kvp in m_dictWIPNameID)
        {
            csv += kvp.Key;
            csv += ",";
            csv += kvp.Value;
            csv += "\n"; //newline to represent new pair
        }

        File.WriteAllText(m_LocalPath + @"\" + WIP + @"\WIP.csv", csv);
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
                var values = line.Split(',');

                m_dictWIPNameID.Add(values[0], values[1]);
                DisplayWIP(values[0], values[1]);
            }
        }
    }

    public bool RemoveWIP( string filename )
    {

        return false;
    }


    private void OnChanged(object source, FileSystemEventArgs e)
    {

        Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
        
        if( isAssetinWIP(e.Name) )
        {
            UpdateOnWIP(e.FullPath);
        }

        
    }
    

}
