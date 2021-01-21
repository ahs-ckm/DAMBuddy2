﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace DAMBuddy2
{
    public delegate void ModifiedCallback(string filename, string state);
    public delegate void RootEditCallback(string filename, bool state);
    public delegate void ReadyStateCallback(bool isReady);
    public delegate void GenericCallback(string filename);
    public delegate void UploadStateCallback(string Ticket, RepoManager.TicketChangeState state);
    public delegate void UserInfoCallback(string message, int nCacheCount);

    public class RepoInstanceConfig { 
    
        /// <summary>
        /// the path to the folder for the repository
        /// </summary>
        public string BaseFolder;

        /// <summary>
        /// the ticket id (e.g. CSDFK-1234)
        /// </summary>
        public string TicketID;

        /// <summary>
        /// the folder name (from the server)
        /// </summary>
        public string FolderID;

        /// <summary>
        /// How many milliseconds to delay the first git pull for the repo
        /// </summary>
        public int GitPullInitialDelay;

        /// <summary>
        /// How many milliseconds between git pulls
        /// </summary>
        public int GitPullInterval;

        /// <summary>
        /// Denotes if this repository is the current/active in the UI
        /// </summary>
        public bool isActive = false;

        /// <summary>
        /// Collection of URL endpoints
        /// </summary>
        /// <summary>
        /// The URL of the server....?
        /// </summary>
        public string URLServer;

        /// <summary>
        /// the URL for the transform support endpoint
        /// </summary>
        public string URLCache;
    }

    public class RepoCallbackSettings
    {
        public GenericCallback callbackScheduleState;
        public ReadyStateCallback callbackTicketState;
        public ModifiedCallback callbackModifiedWIP;
        public RootEditCallback callbackRootEditWIP;
        public GenericCallback callbackRemoveWIP;
        public GenericCallback callbackStale;
        public GenericCallback callbackDisplayWIP;
        public UploadStateCallback callbackUploadState;
        public UserInfoCallback callbackInfo;
    }

    public class RepoInstance
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


        private static string DAM_SCHEDULER_PORT = "10008";
        private List<ListViewItem> m_masterlist;

        private bool m_ReadyStateSetByUser = false;
        private static Dictionary<string, string> m_dictFileToPath;
        private Dictionary<string, string> m_dictID2Gitpath;
        private Dictionary<string, string> m_dictWIPName2Path;
        private Dictionary<string, bool> m_dictWIPRootNodeEdits;
        private Dictionary<string, string> m_dictWIPID2Path;
        private FileSystemWatcher m_watcherNewAssets;
        private RepoCallbackSettings mCallbacks;
        private FileSystemWatcher m_watcherRepo = null;
        private FileSystemWatcher m_watcherWIP = null;

        private System.Threading.Timer m_timerPull = null;
        public List<ListViewItem> Masterlist { get => m_masterlist; set => m_masterlist = value; }

        public string TicketID { get => mConfig.TicketID; }

        public string TicketFolder { get => mConfig.BaseFolder; }

        private RepoInstanceConfig mConfig;
        private readonly string INFO_RESCHEDULE_WARNING = "Adding assets when the ticket is ready will require the ticket to be rescheduled. \nIf the new asset is being changed by another user, it may result in your ticket becoming blocked.\nTo avoid this warning, pause the work whilst you add the necessary assets.";

        public string WIPPath
        {
            get => mConfig.BaseFolder + Utility.GetSettingString("WorkInProgress");//WIP;
        }

        public string AssetPath
        {
            get => mConfig.BaseFolder + Utility.GetSettingString("Assets");
        }

        private void EnableBackgroundActivities()
        {
            m_timerPull.Change(mConfig.GitPullInitialDelay, mConfig.GitPullInterval);
            m_watcherRepo.EnableRaisingEvents = true;
            m_watcherWIP.EnableRaisingEvents = true;
        }

        private void DisableBackgroundActivities()
        {
            m_timerPull.Change(Timeout.Infinite, Timeout.Infinite);
            m_watcherRepo.EnableRaisingEvents = false;
            m_watcherWIP.EnableRaisingEvents = false;
        }

        public void MakeInactive() // no longer the current repo
        {
            mConfig.isActive = false;
            DisableBackgroundActivities();
        }

        public void MakeActive() // this is the current repository now
        {
            mConfig.isActive = true;
            LoadExistingWIP();
            EnableBackgroundActivities();
            mCallbacks.callbackTicketState?.Invoke(m_ReadyStateSetByUser);
        }

        public bool UploadForReview(string filepathWipHTML)
        {
            var result = false;

            string html = File.ReadAllText(filepathWipHTML);

            try
            {
                using (WebClient client = new WebClient())
                {
                    string sPort = Utility.GetSettingString("DAMUploadPort");
                    string response = client.UploadString(mConfig.URLServer + ":" + sPort + "/ReviewDocument," + mConfig.TicketID + "," + "test.html", html);
                    Console.WriteLine("\nResponse Received. The contents of the file uploaded are:\n{0}", response);
                }
                result = true;
            }
            catch
            {
            }
            return result;
        }

        public RepoInstance(RepoInstanceConfig config, RepoCallbackSettings callbacks)
        {
            m_masterlist = new List<ListViewItem>();
            mConfig = config;
            mCallbacks = callbacks;

            Init();
            LoadRepositoryTemplates();
        }

        public void Shutdown()
        {
            m_watcherRepo.EnableRaisingEvents = false;
            m_watcherWIP.EnableRaisingEvents = false;

            m_watcherWIP.Dispose();
            m_watcherRepo.Dispose();
            m_timerPull.Dispose();

            SaveExistingWip();
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

        public void ConfigureAndLaunchTD(string assetfilepath)
        {
            string config = GetTicketConfigForOcean();

            OceanUtils.ConfigureTD(TicketID, config);

            OceanUtils.LaunchTD(assetfilepath);
        }

        public void LoadRepositoryTemplates()
        {
            m_masterlist.Clear();
            ListViewItem newAsset = null;
            if (m_dictFileToPath == null) m_dictFileToPath = new Dictionary<string, string>();

            string[] templates = Directory.GetFiles(mConfig.BaseFolder, "*.oet", SearchOption.AllDirectories);
            foreach (string template in templates)
            {
                string filename = Path.GetFileName(template);

                m_dictFileToPath[filename] = template;

                newAsset = new ListViewItem(filename);
                newAsset.Tag = template;

                m_masterlist.Add(newAsset);
            }
        }

        private bool PrepareTransformSupport()
        {
            string remoteUri = mConfig.URLCache;

            string fileName = mConfig.BaseFolder + "\\" + "transform_support.zip", myStringWebResource = null;
            // Create a new WebClient instance.
            System.Net.WebClient myWebClient = new WebClient();
            // Concatenate the domain with the Web resource filename.
            myStringWebResource = remoteUri;
            Console.WriteLine("Downloading File \"{0}\" from \"{1}\" .......\n\n", fileName, myStringWebResource);
            // Download the Web resource and save it into the current filesystem folder.
            myWebClient.DownloadFile(myStringWebResource, fileName);
            Console.WriteLine("Successfully Downloaded File \"{0}\" from \"{1}\"", fileName, myStringWebResource);
            Console.WriteLine("\nDownloaded file saved in the following file system folder:\n\t" + System.Windows.Forms.Application.StartupPath);

            ZipArchive archive = ZipFile.OpenRead(fileName);
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string entryfullname = Path.Combine(TicketFolder, entry.FullName);
                string entryPath = Path.GetDirectoryName(entryfullname);
                if (!Directory.Exists(entryPath))
                {
                    Directory.CreateDirectory(entryPath);
                }

                string entryFn = Path.GetFileName(entryfullname);
                if (!String.IsNullOrEmpty(entryFn))
                {
                    entry.ExtractToFile(entryfullname, true);
                }
            }
            return true;
        }

        public bool GetTicketScheduleStatus()
        {
            if (!mConfig.isActive) return false;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{mConfig.URLServer}:{DAM_SCHEDULER_PORT}/dynamic/TicketStatus,{TicketID}");
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonStatus = reader.ReadToEnd();
                RepoInstance.TicketScheduleState state = System.Text.Json.JsonSerializer.Deserialize<RepoInstance.TicketScheduleState>(jsonStatus);

                if (state.UploadEnabled == "true")
                {
                    LockFiles(false);
                }
                else
                {
                    LockFiles(true);
                }

                if (!mConfig.isActive) return false;

                mCallbacks.callbackScheduleState?.Invoke(jsonStatus);
            }

            return true;
        }

        private bool LockFiles(bool Lock)
        {
            try
            {
                string[] wiptemplates = System.IO.Directory.GetFiles(WIPPath, "*.oet");
                foreach (string wiptemplate in wiptemplates)
                {
                    var attr = File.GetAttributes(wiptemplate);

                    if (Lock)
                    {
                        File.SetAttributes(wiptemplate, FileAttributes.ReadOnly);
                    }
                    else
                    {
                        File.SetAttributes(wiptemplate, FileAttributes.Normal);
                    }
                }
            }
            catch
            { return false; }
            return true;
        }

        private static bool UpdateSchedule(string URLServer)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{URLServer}:{DAM_SCHEDULER_PORT}/dynamic/BuildPlan.json");
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
            string damFolder = mConfig.FolderID;

            using (WebClient client = new WebClient())
            {
                string theParams = $"theFolder={damFolder}&theTemplateID={sTID}";
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string sPort = Utility.GetSettingString("DAMUploadPort");

                string response = client.UploadString(mConfig.URLServer + ":" + sPort + "/RemoveWIP", theParams);
                Console.WriteLine(response);
            }
            result = true;

            return result;
        }

        // prepare an asset as WIP
        public void AddWIP(string gitfilepath)
        {
            // If ticket is ready (and so in the schedule), it needs to be rescheduled.
            // This means setting the ticket to not ready, then back to ready after the asset has been added.

            bool bResetSchedule = false;

            if (m_ReadyStateSetByUser)
            {
                if (MessageBox.Show(INFO_RESCHEDULE_WARNING,
                    "Schedule Warning",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning) == DialogResult.Cancel) { return; }

                SetTicketReadiness(false);
                bResetSchedule = true;
            }
            try
            {


                string sGitKeepInitial = Utility.GetSettingString("GitKeepInitial");
                string sGitKeepUpdate = Utility.GetSettingString("GitKeepUpdate");
                string sGitKeepSuffix = Utility.GetSettingString("GitKeepSuffix");

                string filename = Path.GetFileName(gitfilepath);
                string filepathWIP = mConfig.BaseFolder + Utility.GetSettingString("WorkInProgress") + @"\" + filename;

                //string filepathWIP = mConfig.BaseFolder + WIP + @"\" + filename;
                string initialFile = mConfig.BaseFolder + sGitKeepInitial + @"\" + filename + sGitKeepSuffix;

                // copy the asset to the working folder
                File.Copy(gitfilepath, filepathWIP);

                File.SetAttributes(filepathWIP, FileAttributes.ReadOnly);

                // move asset from repository folder into the intitial git folder (which is used to flag for stale/updated assets)
                File.Move(gitfilepath, initialFile);

                Utility.MakeMd5(initialFile);

                string sTID = Utility.GetTemplateID(filepathWIP);

                m_dictID2Gitpath[sTID] = gitfilepath; // id -> original directory in git repo
                m_dictWIPName2Path[filename] = filepathWIP; // name -> filepath
                m_dictWIPID2Path[sTID] = filepathWIP; // id -> filepath

                WIPToServer(filename, sTID);

                SaveExistingWip();

                mCallbacks.callbackDisplayWIP?.Invoke(filename);
            }
            finally
            {
                if (bResetSchedule)
                {
                    SetTicketReadiness(true);
                }
            }
        }

        private void SaveInitialState(string filepath)
        {
            // copy to git
            string filename = Path.GetFileName(filepath);
            string gitinitpath = mConfig.BaseFolder + @"\" + Utility.GetSettingString("GitKeepInitial");
            File.Move(filepath, gitinitpath);

            Utility.MakeMd5(gitinitpath);
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
            string gitpath = mConfig.BaseFolder + @"\" + Utility.GetSettingString("GitKeepInitial");// GITKEEP_UPDATE;
            string gitkeepfile = gitpath + @"\" + filename + Utility.GetSettingString("GitKeepSuffix");// GITKEEP_SUFFIX;
            if (File.Exists(gitkeepfile))
            {
                File.Delete(gitkeepfile);
            }
            File.Move(filepath, gitkeepfile);

            Utility.MakeMd5(gitkeepfile);
        }

        public string GetTemplateID(string filename)
        {
            if (isAssetinWIP(filename))
            {
                return Utility.GetTemplateID(m_dictWIPName2Path[filename]);

                // return Utility.GetTemplateID(mConfig.BaseFolder + @"\" + WIP + @"\" + filename);
            }

            return Utility.GetTemplateID(m_dictFileToPath[filename]);
        }

        private bool IsStale(string asset)
        {

            string sGitKeepInitial = Utility.GetSettingString("GitKeepInitial");
            string sGitKeepUpdate = Utility.GetSettingString("GitKeepUpdate");
            string sGitKeepSuffix = Utility.GetSettingString("GitKeepSuffix");

            if (!File.Exists(mConfig.BaseFolder + sGitKeepInitial + @"\" + asset + sGitKeepSuffix + ".md5"))
                return false;

            if (!File.Exists(mConfig.BaseFolder + sGitKeepUpdate + @"\" + asset + sGitKeepSuffix + ".md5"))
                return false;

            string WIPHash = File.ReadAllText(mConfig.BaseFolder + sGitKeepInitial + @"\" + asset + sGitKeepSuffix + ".md5");
            string UpdateHash = File.ReadAllText(mConfig.BaseFolder + sGitKeepUpdate + @"\" + asset + sGitKeepSuffix + ".md5");

            if (String.Compare(WIPHash, UpdateHash) == 0)
            {
                return false;
            }

            return true;
        }

        private void VerifyFreshness()
        {
            if (!mConfig.isActive) return;

            foreach (var item in m_dictWIPID2Path)
            {
                string filename = item.Key;
                if (IsStale(filename)) mCallbacks.callbackStale?.Invoke(filename);
            }
        }

        public bool SetRootNodeEdit( bool bDoRootNodeEdit, string sTemplateName )
        {
            bool result = true;
            string sURL = "";
            
            if (bDoRootNodeEdit)
            {
                sURL = Utility.GetSettingString("SetRootNodeEditURL");
            }
            else
            {
                sURL = Utility.GetSettingString("CancelRootNodeEditURL");

            }
            string sTemplateID = GetTemplateID(sTemplateName);

               
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{sURL},{TicketID},{sTemplateID}");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                if( response.StatusCode != HttpStatusCode.OK )
                {
                    Logger.Error("Failed to manage RootNodeEdit on the server");
                    result = false;
                }
                    
            } catch (Exception e)
            {
                Logger.Error(e, "An error occurred which is going to cause the application to close.");
                result = false;
            }

            if( result )
            {
                if (bDoRootNodeEdit)
                {
                    m_dictWIPRootNodeEdits[sTemplateName] = true;
                } else
                {
                    m_dictWIPRootNodeEdits.Remove(sTemplateName);
                }
                mCallbacks.callbackRootEditWIP?.Invoke(sTemplateName, bDoRootNodeEdit);

            }

            return result;
        }

        public bool PostWIP()
        {
            bool result = false;

            string zipname = @"c:\temp\dambuddy2\togo-" + TicketID + ".zip";

            string sWIP = Utility.GetSettingString("WorkInProgress");
            string directory = mConfig.BaseFolder + sWIP;

            if (File.Exists(zipname))
            {
                File.Delete(zipname);
            }

            ZipFile.CreateFromDirectory(directory, zipname);

            // need to add the files in the ticket folder, which may contain brand new templates (not in WIP folder).
            using (FileStream zipToOpen = new FileStream(zipname, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    string[] newtemplates = System.IO.Directory.GetFiles(AssetPath, "*.oet");
                    foreach (string newtemplate in newtemplates)
                    {
                        ZipArchiveEntry newentry = archive.CreateEntry(Path.GetFileName(newtemplate));
                        using (StreamWriter writer = new StreamWriter(newentry.Open()))
                        {
                            writer.Write(File.ReadAllText(newtemplate));
                        }
                    }
                }
            }

            //Directory.Move(directory, directory + "-posted");

            long length = new System.IO.FileInfo(zipname).Length;
            Console.WriteLine("\nSending file length: {0}", length);

            string damfolder = mConfig.FolderID;

            using (WebClient client = new WebClient())
            {
                string sPort = Utility.GetSettingString("DAMUploadPort");

                byte[] responseArray = client.UploadFile(mConfig.URLServer + ":" + sPort + "/upload," + damfolder, "POST", zipname);
                // Decode and display the response.
                Console.WriteLine("\nResponse Received. The contents of the file uploaded are:\n{0}",
                    System.Text.Encoding.ASCII.GetString(responseArray));
            }
            result = true;

            return result;
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

            File.WriteAllText(mConfig.BaseFolder + @"\WIP.csv", csv);

            csv = "";
            foreach (KeyValuePair<string, string> kvp in m_dictID2Gitpath)
            {
                csv += kvp.Key;
                csv += ",";
                csv += kvp.Value;
                csv += "\n"; //newline to represent new pair
            }

            File.WriteAllText(mConfig.BaseFolder + @"\ID2Gitpath.csv", csv);

            csv = "";
            foreach (KeyValuePair<string, string> kvp in m_dictWIPID2Path)
            {
                csv += kvp.Key;
                csv += ", ";
                csv += kvp.Value;
                csv += "\n"; //newline to represent new pair
            }

            File.WriteAllText(mConfig.BaseFolder + @"\WIPID.csv", csv);

            csv = "";
            foreach (KeyValuePair<string, bool> kvp in m_dictWIPRootNodeEdits)
            {
                csv += kvp.Key;
                csv += ", ";
                csv += kvp.Value;
                csv += "\n"; //newline to represent new pair
            }

            File.WriteAllText(mConfig.BaseFolder + @"\RootNodeEdits.csv", csv);



            File.WriteAllText(mConfig.BaseFolder + @"\ReadyState.txt", m_ReadyStateSetByUser.ToString());
        }

        public void LoadExistingWIP()
        {
            m_dictWIPName2Path.Clear();
            m_dictID2Gitpath.Clear();
            m_dictWIPID2Path.Clear();
            m_dictWIPRootNodeEdits.Clear();

            string filepath = mConfig.BaseFolder + @"\WIP.csv";
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
                    mCallbacks.callbackDisplayWIP?.Invoke(filename);

                    string sWIP = Utility.GetSettingString("WorkInProgress");
                    string filepathWIP = mConfig.BaseFolder + sWIP + @"\" + filename;

                    CompareWIP2Initial(filepathWIP); // to ensure tracking of modifications
                }
            }

            filepath = mConfig.BaseFolder + @"\ID2Gitpath.csv";
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

            filepath = mConfig.BaseFolder + @"\WIPID.csv";
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


            filepath = mConfig.BaseFolder + @"\RootNodeEdits.csv";
            if (File.Exists(filepath))
            {
                var reader = new StreamReader(File.OpenRead(filepath));

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == "") break;
                    var values = line.Split(',');

                    bool state = (values[1] == " True") ? true : false;

                    m_dictWIPRootNodeEdits.Add(values[0], state);
                    mCallbacks.callbackRootEditWIP?.Invoke(values[0], state);
                }
            }



            filepath = mConfig.BaseFolder + @"\ReadyState.txt";

            if (File.Exists(filepath))
            {
                var reader = new StreamReader(File.OpenRead(filepath));

                var line = reader.ReadLine();
                if (line == "True") { m_ReadyStateSetByUser = true; }
                SetTicketReadiness(m_ReadyStateSetByUser);
                mCallbacks.callbackTicketState?.Invoke(m_ReadyStateSetByUser);
            }
        }

        public bool RemoveWIP(string filename)
        {
            string assetfilepath = m_dictWIPName2Path[filename];

            if (!File.Exists(assetfilepath))
            {
                Console.WriteLine($"RemoveWIP() : {assetfilepath} doesn't exist");
                return false;
            }

            string sTID = Utility.GetTemplateID(assetfilepath);
            string trashDir = mConfig.BaseFolder + Utility.GetSettingString("KeepTrash");
            string dirname = Path.GetDirectoryName(assetfilepath);

            if( Path.GetDirectoryName(dirname) == Path.GetDirectoryName(WIPPath ) )
            {
                // if modified message the user

                // delete file in WIP
                string gitpath = m_dictID2Gitpath[sTID];
                
                string initialFile = mConfig.BaseFolder + @"\" + Utility.GetSettingString("GitKeepInitial") + @"\" + filename + Utility.GetSettingString("GitKeepSuffix");
                string updateFile = mConfig.BaseFolder + @"\" + Utility.GetSettingString("GitKeepUpdate") + @"\" + filename + Utility.GetSettingString("GitKeepSuffix");
                try
                {
                    if (File.Exists(assetfilepath))
                    {
                        File.SetAttributes(assetfilepath, FileAttributes.Normal);
                        File.Delete(assetfilepath);
                    }

                    if (File.Exists(assetfilepath + ".md5"))
                    {
                        File.Delete(assetfilepath + ".md5");
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

                    WIPRemoveFromServer(filename, GetTemplateID(filename));

                    m_dictWIPName2Path.Remove(filename);
                    m_dictWIPName2Path.Remove(sTID);
                    m_dictWIPID2Path.Remove(sTID);
                    m_dictID2Gitpath.Remove(sTID);


                    if( m_dictWIPRootNodeEdits.ContainsKey(filename ) )
                    {
                        SetRootNodeEdit(false, filename);
                        m_dictWIPRootNodeEdits.Remove(filename);
                    }

                    

                    //  TODO: put back in
                    /*                    if (File.Exists(assetfilepath))
                                        {
                                            if (!File.Exists(trashDir))
                                                Directory.CreateDirectory(trashDir);

                                            if (File.Exists(trashDir + "\\" + filename + ".old"))
                                            {
                                                    File.SetAttributes(trashDir + "\\" + filename + ".old", FileAttributes.Normal);
                                            }
                                            File.Delete(trashDir + "\\" + filename + ".old");

                                            File.Move(assetfilepath, trashDir + "\\" + filename + ".old");
                                            File.SetAttributes(trashDir + "\\" + filename + ".old", FileAttributes.Normal);

                                        }*/

                    mCallbacks.callbackRemoveWIP?.Invoke(filename);
                    SaveExistingWip();
                }
                catch (Exception e)
                {
                    Console.WriteLine("RemoveWIP() : " + e.Message);
                }
            }
            else
            {
                MessageBox.Show("Will delete new asset " + filename);
                if (File.Exists(assetfilepath))
                {
                    if (!File.Exists(trashDir))
                        Directory.CreateDirectory(trashDir);

                    if (File.Exists(trashDir + "\\" + filename + ".old"))
                        File.Delete(trashDir + "\\" + filename + ".old");

                    File.Move(assetfilepath, trashDir + "\\" + filename + ".old");
                }
                if (File.Exists(assetfilepath + ".md5"))
                {
                    File.Delete(assetfilepath + ".md5");
                }

                WIPRemoveFromServer(filename, GetTemplateID(filename));

                m_dictWIPName2Path.Remove(filename);
                m_dictWIPName2Path.Remove(sTID);
                m_dictWIPID2Path.Remove(sTID);
                m_dictID2Gitpath.Remove(sTID);

                mCallbacks.callbackRemoveWIP?.Invoke(filename);
                SaveExistingWip();
            }
            return true;
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

        private void CompareWIP2Initial(string filepath)
        {
            string asset = Path.GetFileName(filepath);

            string initialasset = mConfig.BaseFolder + Utility.GetSettingString("GitKeepInitial") + @"\" + asset + Utility.GetSettingString("GitKeepSuffix");
            byte[] WIPHashBytes = new byte[16];

            //string WIPHashHex = "";

            string wipContents = Utility.ReadAsset(filepath);
            string initialContents = Utility.ReadAsset(initialasset);

            if (string.IsNullOrEmpty(initialContents))
                return;

            if (String.IsNullOrEmpty(wipContents))
                return;

            if (!wipContents.Equals(initialContents))
            {
                mCallbacks.callbackModifiedWIP?.Invoke(asset, "CHANGED");
            }
        }

        private void OnChangedNewAsset(object source, FileSystemEventArgs e)
        {
            m_dictFileToPath[e.Name] = e.FullPath;

            string sTID = Utility.GetTemplateID(e.FullPath);
            string filepath = "";

            if (m_dictWIPName2Path.TryGetValue(e.Name, out filepath)) return; ;

            m_dictWIPName2Path[e.Name] = e.FullPath; // name -> filepath
            m_dictWIPID2Path[sTID] = e.FullPath; // id -> filepath

            WIPToServer(e.FullPath, sTID);

            SaveExistingWip();

            Console.WriteLine($"OnChangedNewAsset File: {e.FullPath} {e.ChangeType}");

            mCallbacks.callbackDisplayWIP?.Invoke(e.Name);
            mCallbacks.callbackModifiedWIP?.Invoke(e.Name, "NEW");
        }

        private void OnChangedWIP(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"OnChangedWIP File: {e.FullPath} {e.ChangeType}");
            CompareWIP2Initial(e.FullPath);

            // TODO: should check whether the md5 of the file is actually different to the initial md5
        }

        public void Init()
        {
            m_timerPull = new System.Threading.Timer(TimeToPull, null, mConfig.GitPullInitialDelay, mConfig.GitPullInterval);

            m_dictID2Gitpath = new Dictionary<string, string>();
            m_dictWIPName2Path = new Dictionary<string, string>();
            m_dictWIPID2Path = new Dictionary<string, string>();
            m_dictWIPRootNodeEdits = new Dictionary<string, bool>();

            m_watcherNewAssets = new FileSystemWatcher();
            m_watcherNewAssets.Path = AssetPath;
            m_watcherNewAssets.IncludeSubdirectories = false;
            m_watcherNewAssets.NotifyFilter = NotifyFilters.LastWrite;
            m_watcherNewAssets.Filter = "*.oet";
            m_watcherNewAssets.Changed += OnChangedNewAsset;

            m_watcherNewAssets.EnableRaisingEvents = true;

            m_watcherRepo = new FileSystemWatcher();
            m_watcherRepo.Path = mConfig.BaseFolder + @"\" + Utility.GetSettingString("Assets") + @"\templates";
            m_watcherRepo.IncludeSubdirectories = true;
            m_watcherRepo.NotifyFilter = NotifyFilters.LastWrite;
            m_watcherRepo.Filter = "*.oet";
            m_watcherRepo.Created += OnChangedRepo;
            m_watcherRepo.Changed += OnChangedRepo;

            m_watcherRepo.EnableRaisingEvents = true;

            m_watcherWIP = new FileSystemWatcher();
            string sWIP = Utility.GetSettingString("WorkInProgress");

            m_watcherWIP.Path = mConfig.BaseFolder + sWIP;
            m_watcherWIP.IncludeSubdirectories = true;
            m_watcherWIP.NotifyFilter = NotifyFilters.LastWrite;
            m_watcherWIP.Filter = "*.oet";
            m_watcherWIP.Changed += OnChangedWIP;
            m_watcherWIP.EnableRaisingEvents = true;

            PrepareTransformSupport();

            LoadExistingWIP();

            VerifyFreshness();
        }

        // called when an asset is
        public void UpdateOnWIP(string filepath)
        {
            // move file to gitupdate
            SaveUpdateState(filepath);
            if (IsStale(Path.GetFileName(filepath)))
            {
                if (!mConfig.isActive) return;

                mCallbacks.callbackStale?.Invoke(Path.GetFileName(filepath));
            }
        }

        private void TimeToPull(Object info)
        {
            Console.WriteLine("TimeToPull()");
            RepoCacheManager.Pull2(TicketFolder);

            // TODO: move this process & timer to RepoCacheHelper
            GetTicketScheduleStatus();
        }

        private bool WIPToServer(string sTemplateName, string sTID)
        {
            return PostWIP();
        }

        public static void CloseTicketOnServer(string sTicketID, string sFolder, string URLServer)
        {
            string sPort = Utility.GetSettingString("DAMUploadPort");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{URLServer}:{sPort}/dynamic/removeTicket,{sTicketID},{sFolder}");
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonState = reader.ReadToEnd();
            }

            UpdateSchedule(URLServer);
        }

        public bool SetTicketReadiness(bool bReady)
        {
            m_ReadyStateSetByUser = bReady;

            string ReadyParam = "notready";
            if (bReady)
            {
                ReadyParam = "ready";
            }

            string damFolder = mConfig.FolderID;

            string theParams = $"theState={ReadyParam}&theFolder={damFolder}";

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    string sPort = Utility.GetSettingString("DAMUploadPort");

                    string response = client.UploadString(mConfig.URLServer + ":" + sPort + "/ready", theParams);
                    Console.WriteLine(response);
                }
            }
            catch
            {
                MessageBox.Show("Failed to set ticket to Ready");
                return false;
            }

            UpdateSchedule(mConfig.URLServer);
            GetTicketScheduleStatus();
            return true;
        }

        public string GetTicketConfigForOcean()
        {
            string description = "the description";
            string ticket = TicketID;

            string config = $@"<DAM><RepositoryData><RepositoryName>{ticket}</RepositoryName><Description>{description}</Description><TemplatesPath>{AssetPath}</TemplatesPath><ArchetypesPath>{AssetPath}\archetypes</ArchetypesPath><WorkingArchetypesPath/><CkmApiUrl>https://ahsckm.ca/ckm/rest/v1/</CkmApiUrl><CkmApiBatchSize>300</CkmApiBatchSize></RepositoryData></DAM>";

            return config;
        }

        internal bool isAssetinWIPByID(string sEmbeddedId, ref string filepath)
        {
            filepath = "";
            bool exists = m_dictWIPID2Path.TryGetValue(sEmbeddedId, out filepath);
            return exists;
        }
    }
}