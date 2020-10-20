﻿
using Saxon.Api;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using java.util;
using Microsoft.Office.Interop.Word;
using System.IO.Compression;
using System.Configuration;
using System.Xml.Serialization;
using net.sf.saxon.trans.rules;
using System.Runtime.InteropServices;
using System.Xml.Schema;
using CefSharp.WinForms;
using System.Diagnostics;
using System.Threading;
//using CefSharp.Winforms;

namespace DAMBuddy2
{

    public class TransformArgs
    {
        public delegate void DisplayCallback(string s);
        public delegate void StatusCallback(string s); 
        public string sTemplateName;
        public DisplayCallback callbackDisplayHTML;
        public StatusCallback callbackStatusUpdate;
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private string gServerName = "http://ckcm.healthy.bewell.ca";
        private string gDAMPort = "";
        private string gFolderName = "";
        private string gCacheName = "";
        private string gCacheDir = "";
        private RepoManager m_RepoManager;

        private delegate void ControlCallback(string s);
        private System.Drawing.Point m_ptScrollPos;
        private string m_currentHTML;
        private string m_currentDocument = ""; // used to track whether the same document is being viewed/reviewed, if so we should keep the position
        private static string m_RepoPath = "";
        private List<ListViewItem> m_masterlist;
        // The name of the file that will store the latest version. 
        private static string latestVersionInfoFile = "Preview_version";

        private string m_PushDir = @"c:\temp\dambuddy2\togo";
        private static string m_OPTWebserviceUrl = ""; //@"http://wsckcmapp01/OptWs/OperationalTemplateBuilderService.asmx";
        private string m_CacheServiceURL = ""; //@"http://ckcm.healthy.bewell.ca:8091/transform_support";


        protected String startBlock = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:opt=\"http://www.oceaninformatics.org/OPTWS\" xmlns:tem=\"openEHR/v1/Template\" xmlns:v1=\"http://schemas.openehr.org/v1\">\r\n" +
                                  "   <soapenv:Header/>\r\n" +
                                  "   <soapenv:Body>\r\n" +
                                  "<BuildOperationalTemplate xmlns=\"http://www.oceaninformatics.org/OPTWS\">";

        protected String endBlock = "<language>en</language>\r\n" +
            "  <checkIntegrity>false</checkIntegrity>\r\n" +
            "</BuildOperationalTemplate>\r\n" +
            "\r\n" +
            "   </soapenv:Body>\r\n" +
            "</soapenv:Envelope>";

        private static Dictionary<string, string> dictFileToPath;

        private ChromiumWebBrowser m_browserUpload;
        private ChromiumWebBrowser m_browserSchedule;

        private Dictionary<string, List<string>> dictTemplateChildren;
        private Dictionary<string, string> dictIdName;
        private Dictionary<string, List<string>> dictIdArchetypes;

        private static TransformRequestBuilder m_RequestBuilder;


        public void DisplayTransformedDocuumentCallback(string filename)
        {
            
            DisplayTransformedDocument(filename);
        }


        public void DisplayTransformedDocument( string filename )
        {

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { this.DisplayTransformedDocument(filename); });
                return;
            }
            if (filename == null) return;
            if (filename.Trim() == "") return;
                        
            wbRepositoryView.Url = new Uri(filename);

        }


        public void StaleCallback(string filename) {
            //MessageBox.Show("StaleCallback :" + filename);

            SetAssetStale(filename);
        }

        public void UpdateWorkViewTitle( )
        {
            string AssetCount = lvWork.Items.Count.ToString();
            int stalecount = 0;
            foreach ( ListViewItem item in lvWork.Items )
            {
                if (item.SubItems[1].Text == "STALE") stalecount++;
            }

            if( stalecount == 0 )
            {
                tpWIP.Text = "Work View (" + AssetCount + ")";
            }
            else 
            { 
                tpWIP.Text = "Work View (" + lvWork.Items.Count.ToString() + " - " + stalecount.ToString() +"!)"; 
            }
            
        }

        public void RemoveWIPCallback(string filename)
        {
            foreach( ListViewItem item in lvWork.Items )
            {
                if( item.Text == filename)
                {
                    lvWork.Items.Remove(item);
                    break;
                }
            }

            UpdateWorkViewTitle();
            //tpWIP.Text = "Work View (" + lvWork.Items.Count.ToString() + ")";
        }

        public void DisplayWIPCallback(string filename, string originalpath)
        {
            ListViewItem newitem = new ListViewItem(filename);
            newitem.Tag = originalpath;
            newitem.SubItems.Add("Fresh");
            lvWork.Items.Add(newitem);

            UpdateWorkViewTitle();
            //tpWIP.Text = "Work View (" + lvWork.Items.Count.ToString() + ")";
        }



        private void InitializeApp()
        {
            if (SetCurrentRepo() != "")
            //if (true)
            {
                //listView1.SelectedIndexChanged += listView1_SelectedIndexChanged();
                var appsettings = ConfigurationManager.AppSettings;
                //listView1.LostFocus += (s, e) => listView1.SelectedIndices.Clear();
                AppSettingsSection settings = (AppSettingsSection)ConfigurationManager.GetSection("PreviewView.Properties.Settings");


                
                m_RepoManager = new RepoManager(m_RepoPath, StaleCallback, DisplayWIPCallback, RemoveWIPCallback);
                m_RepoManager.Init(30000*1, 60000*1);

                m_browserSchedule = new ChromiumWebBrowser("http://ckcm:8008/scheduler-plan.html"); // TODO:Fix port
                m_browserUpload = new ChromiumWebBrowser("about:blank");
                tpUpload.Controls.Add(m_browserUpload);
                tpSchedule.Controls.Add(m_browserSchedule);
                

                m_masterlist = new List<ListViewItem>();
                this.Text = "BuildBuddy v" + GetLocalVersionNumber();

                m_OPTWebserviceUrl = appsettings["OPTServiceUrl"] ?? "App Settings not found";
                m_CacheServiceURL = appsettings["CacheServiceUrl"] ?? "App Settings not found";
                

                m_RequestBuilder = new TransformRequestBuilder(m_RepoManager, appsettings["QueryServiceUrl"] ?? "App Settings not found");

                //string test = settings.Settings["CacheServiceUrl"] ?? "";


                PrepareTransformSupport();

                LoadRepositoryTemplates();
                //LoadTransforms();
                //            webBrowser1.Url =

                tsbWord.Enabled = false;
                //webBrowser1.Url = new Uri(@"C:\TD\Blank.html");
            }
            else
            {
                MessageBox.Show("No repository detected - is DamBuddy running?");
            }

        }


        private bool PrepareTransformSupport()
        {

            string remoteUri = m_CacheServiceURL;

            string fileName = "transform_support.zip", myStringWebResource = null;
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
                string entryfullname = Path.Combine(m_RepoPath, entry.FullName);
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

        private static string BuildSOAPRequest3(string sTemplateFilepath)
        {
            string theRequest = "";
            m_RequestBuilder.BuildRequest(sTemplateFilepath, ref theRequest);
            return theRequest;
        }



        private static string GetLocalVersionNumber()
        {

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, latestVersionInfoFile);

            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return null;
        }

        public static HttpWebRequest CreateSOAPWebRequest( )
        {
            //Making Web Request  
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(m_OPTWebserviceUrl);
            //SOAPAction  

            Req.Headers.Add(@"SOAP:Action");

            //Content_type  
            Req.ContentType = "text/xml;charset=\"utf-8\"";
            Req.Accept = "text/xml";
            //HTTP method  
            Req.Method = "POST";
            //return HttpWebRequest  
            return Req;
        }


        private void LoadRepositoryTemplates()
        {
            m_masterlist.Clear();
            ListViewItem newAsset = null;
            if (dictFileToPath == null) dictFileToPath = new Dictionary<string, string>();

            tstbRepositoryFilter.Text = "";


            //cbTemplateName.Items.Clear();

            string[] templates = Directory.GetFiles(m_RepoPath, "*.oet", SearchOption.AllDirectories);
            foreach (string template in templates)
            {
                //Console.WriteLine(template);
                //var title = BuildDictionaries(template);

                //  cbTemplateName.Items.Add(title);
                //  cbTemplateName.SelectedIndex = 0;
                //listView1.Items.Add(title);

                string filename = Path.GetFileName(template);

                dictFileToPath[filename] = template;

                newAsset = new ListViewItem(filename);
                newAsset.Tag = template;

                m_masterlist.Add(newAsset);
            }


            DisplayTemplates();

        }

        [DllImport("user32.dll")]
        private static extern long LockWindowUpdate(long Handle);

        private void DisplayTemplates()
        {
            tstbRepositoryFilter.Focus();
            //listView1.se
            lvRepository.Items.Clear();
            // This filters and adds your filtered items to listView1

            try
            {
                lvRepository.SuspendLayout();
                int count = 0;
                //    ..LockWindowUpdate(this.Handle);
                foreach (ListViewItem item in m_masterlist.Where(lvi => lvi.Text.ToLower().Contains(tstbRepositoryFilter.Text.ToLower().Trim())))
                {
                    try
                    {
                        lvRepository.Items.Add(item);

                        // clbRepository.Items.Add(item.Text);
                    }
                    catch { }

                    count++;
                    if (count > 1000) break;
                }
            }
            finally
            {
                //  LockWindowUpdate(0);
                lvRepository.ResumeLayout();
                lvRepository.Columns[0].Width = -1;
            }
        }

        private void DisplayStatusUpdate( string sStatus )
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { this.DisplayStatusUpdate(sStatus); });
                return;
            }
            if (sStatus == null) return;
            if (sStatus.Trim() == "") return;

            toolStripProgressBar1.PerformStep();
            tspStatusLabel.Text = sStatus;
        }

        private void RunThreadedTransform( string sTemplateName )
        {
            var args = new TransformArgs();
            args.sTemplateName = sTemplateName;
            args.callbackDisplayHTML = DisplayTransformedDocuumentCallback;
            args.callbackStatusUpdate = DisplayStatusUpdate; 

            toolStripProgressBar1.Step = 1;
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Minimum = 0;
            toolStripProgressBar1.Maximum = 5;
            toolStripProgressBar1.Visible = true;

            DisplayStatusUpdate("Transforming " + m_currentDocument + ": Building SOAP Request...");

            Thread thread1 = new Thread(RunTransform);
            thread1.Start(args);
            m_currentDocument = sTemplateName;
        }

        private static void RunTransform(object oArgs)
        {
            TransformArgs theArgs = (TransformArgs)oArgs;

            string sTemplateName = theArgs.sTemplateName;
            //  if (tscbTransforms.Text == "") return;
            Cursor.Current = Cursors.WaitCursor;
            string sSelectedTransform = m_RepoPath + @"\XSLT\OrderItem.xsl";
            
            if (sTemplateName.TrimEnd().EndsWith("Panel", StringComparison.OrdinalIgnoreCase) ||
                 sTemplateName.TrimEnd().EndsWith("Protocol", StringComparison.OrdinalIgnoreCase) ||
                 sTemplateName.TrimEnd().EndsWith("Set", StringComparison.OrdinalIgnoreCase) ||
                 sTemplateName.TrimEnd().EndsWith("Group", StringComparison.OrdinalIgnoreCase))
            {
                sSelectedTransform = m_RepoPath + @"\XSLT\OrderSet.xsl";
            }

            try
            {
                string sTempHTML = @"c:\temp\" + Guid.NewGuid().ToString() + @".html";
                string sTempXML = @"c:\temp\" + Guid.NewGuid().ToString() + @"\.xml";

                if (File.Exists(@"c:\temp\generated.xml"))
                {
                    File.Delete(@"c:\temp\generated.xml");
                }

                HttpWebRequest wr = CreateSOAPWebRequest();
                XmlDocument SOAPReqBody = new XmlDocument();
                String optContents = "";

                try
                {
                    string sSOAPRequest = BuildSOAPRequest3(dictFileToPath[sTemplateName]);
                    SOAPReqBody.LoadXml(sSOAPRequest);
                    File.WriteAllText(@"C:\temp\SOAPRequest.xml", sSOAPRequest);

                    using (Stream stream = wr.GetRequestStream())
                    {
                        SOAPReqBody.Save(stream);
                    }

                    /* Status Update
                    tspStatusLabel.Text = "Transforming " + m_currentDocument + ": Requesting OPT document..." + "( " + (System.Text.ASCIIEncoding.ASCII.GetByteCount(sSOAPRequest) / 1024).ToString() + "KB )";
                    toolStripProgressBar1.PerformStep();
                    */
                    theArgs.callbackStatusUpdate("Transforming " + sTemplateName+ ": Requesting OPT document..." + "( " + (System.Text.ASCIIEncoding.ASCII.GetByteCount(sSOAPRequest) / 1024).ToString() + "KB )" );

                    System.Windows.Forms.Application.DoEvents();

                    using (WebResponse Serviceres = wr.GetResponse())
                    {

                        using (StreamReader rd = new StreamReader(Serviceres.GetResponseStream()))
                        {
                            var ServiceResult = rd.ReadToEnd();
                            optContents = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
                            Date now = new Date();
                            optContents += "<!--Operational template XML automatically generated by the DAM Tool at " + now + " calling the OPT Web Service-->";
                            optContents += "<template xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.openehr.org/v1\">";

                            string beginning = "<template xmlns=\"http://schemas.openehr.org/v1\">";
                            string end = "</BuildOperationalTemplateResponse>";

                            int pFrom = ServiceResult.IndexOf(beginning) + beginning.Length;
                            int pTo = ServiceResult.LastIndexOf(end);

                            optContents += ServiceResult.Substring(pFrom, pTo - pFrom);

                        }
                    }

                }
                catch (WebException webExcp)
                {
                    // If you reach this point, an exception has been caught.  
                    Console.WriteLine("A WebException has been caught.");
                    // Write out the WebException message.  
                    Console.WriteLine(webExcp.ToString());
                    // Get the WebException status code.  
                    WebExceptionStatus status = webExcp.Status;
                    // If status is WebExceptionStatus.ProtocolError,
                    //   there has been a protocol error and a WebResponse
                    //   should exist. Display the protocol error.  
                    if (status == WebExceptionStatus.ProtocolError)
                    {
                        Console.Write("The server returned protocol error ");
                        // Get HttpWebResponse so that you can check the HTTP status code.  
                        HttpWebResponse httpResponse = (HttpWebResponse)webExcp.Response;
                        Console.WriteLine((int)httpResponse.StatusCode + " - "
                           + httpResponse.StatusCode);
                    }

                    throw webExcp;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Generic Exception Handler: {ex}");
                    throw ex;
                }

                /* status update
                toolStripProgressBar1.PerformStep();
                tspStatusLabel.Text = "Transforming " + m_currentDocument + ": Generating Final Document...";
                System.Windows.Forms.Application.DoEvents();
                */
                theArgs.callbackStatusUpdate("Transforming " + theArgs.sTemplateName + ": Generating Final Document...");

                var newDocument = new XDocument();

                Processor processor = new Processor(false);
                TextReader sr = new StringReader(optContents);
                DocumentBuilder db = processor.NewDocumentBuilder();
                db.BaseUri = new Uri(@"http://blank.org/");
                XdmNode input = db.Build(sr);
                XsltTransformer transformer = processor.NewXsltCompiler().Compile(new Uri(sSelectedTransform)).Load();
                transformer.InitialContextNode = input;

                String outfile = sTempHTML;
                Serializer serializer = processor.NewSerializer();
                serializer.SetOutputStream(new FileStream(outfile, FileMode.Create, FileAccess.Write));

                transformer.Run(serializer);
                transformer.Close();
                serializer.CloseAndNotify();
                
                
                theArgs.callbackDisplayHTML(sTempHTML);
                theArgs.callbackStatusUpdate("");

            }
            finally
            {
                // Cursor.Current = Cursors.Default;

            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeApp();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // if (cbTemplateName.Text == m_currentDocument)
            // {
            //     webBrowser1.Document.Window.ScrollTo(m_ptScrollPos);
            // }

            //m_currentDocument = cbTemplateName.Text;
            m_currentHTML = wbRepositoryView.Url.ToString();
            tsbWord.Enabled = true;
            Cursor.Current = Cursors.Default;

            tspStatusLabel.Text = "Viewing " + m_currentDocument;

            toolStripProgressBar1.Visible = false;

        }

        private void OpenInWord()
        {
            var word = new Microsoft.Office.Interop.Word.Application();
            word.Visible = true;
            string TemplateFilename = dictIdName[lvRepository.SelectedItems[0].Text];
            string newFilename = m_RepoPath + Path.GetFileNameWithoutExtension(TemplateFilename) + ".html";
            string oldFilename = m_currentHTML.Replace("file:///", "");

            if (File.Exists(newFilename))
            {
                File.Delete(newFilename);
            }

            File.Copy(oldFilename, newFilename);
            //Object filepath = m_currentHTML;
            //var filePath = Server.MapPath("~/MyFiles/Html2PdfTest.html");
            //var savePathPdf = Server.MapPath("~/MyFiles/Html2PdfTest.pdf");
            word.Documents.Open(newFilename);
        }



        private string GetTDConfig()
        {
            string filename = "";

            string OceanDir = "/Ocean_Informatics";

            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string Ocean = appdata + OceanDir;

            DirectoryInfo diOcean = new DirectoryInfo(Ocean);
            WalkDirectoryTree(diOcean, ref filename);

            return filename;
            //MessageBox.Show(filename);
            //messages += ("Storing config in " + filename + "\n");
        }

        private string SetCurrentRepo()
        {
            string ConfigFile = "";
            string TicketDir = "";
            ConfigFile = GetTDConfig();

            //MessageBox.Show("TD Config file :" + ConfigFile);

            if (ConfigFile != "")
            {
                XmlDocument TDConfigDoc = new XmlDocument();

                TDConfigDoc.Load(ConfigFile);

                XmlNode selectedrepo = TDConfigDoc.DocumentElement.SelectSingleNode("/configuration/userSettings/TemplateTool.Properties.Settings/setting[@name='SelectedRepository']/value");
                if (selectedrepo is null)
                {

                }
                else
                {
                    TicketDir = selectedrepo.FirstChild.Value;

                }

                XmlNode TDRepositoryData = TDConfigDoc.DocumentElement.SelectSingleNode("/configuration/userSettings/TemplateTool.Properties.Settings/setting[@name='RepositoryList']/value/ArrayOfRepositoryData");
                List<XmlNode> toRemove = new List<XmlNode>();

                // strip out any existing data for this incoming repository
                foreach (XmlElement confignode in TDRepositoryData)
                {
                    if (confignode.FirstChild.InnerText == TicketDir)
                    {
                        foreach (XmlNode theElement in confignode.ChildNodes)
                        {
                            if (theElement.Name == "TemplatesPath")
                            {

                                m_RepoPath = theElement.InnerText.Replace("Templates", "");
                                tslRepositoryRepo.Text = TicketDir;
                                tslWorkRepository.Text = TicketDir;
                            }
                        }
                    }
                }
                foreach (XmlElement confignode in toRemove)
                {
                    XmlNode node = confignode.ParentNode;
                    node.RemoveChild(confignode);
                }

                //TDConfigDoc.Save(TDConfigFile);

            }

            return m_RepoPath;



        }



        static public string WalkDirectoryTree(System.IO.DirectoryInfo root, ref string filename)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;
            DateTime lastwrite = DateTime.MinValue;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse.
                // You may decide to do something different here. For example, you
                // can try to elevate your privileges and access the file again.
                //log.Add(e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                //  messages += (e.Message) + "\n";
            }

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    //     messages += (fi.FullName);
                    //   messages += (fi.LastWriteTime);

                    if (fi.LastAccessTime > lastwrite && fi.Name.Equals("user.config"))
                    {

                        filename = fi.FullName;
                        lastwrite = fi.LastAccessTime;
                    }
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTree(dirInfo, ref filename);
                }
            }
            return filename;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //TransformSelectedTemplate();


        }

        private void tstbFilter_TextChanged(object sender, EventArgs e)
        {
            timerRepoFilter.Enabled = true;

        }

        private void lvRepository_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (lvRepository.SelectedItems.Count > 0)
            {

                //RunTransform();
                string filename = lvRepository.SelectedItems[0].Text;
                string filepath = dictFileToPath[filename];
                RunThreadedTransform(filename);



                //string templatename = BuildDictionaries(filepath);

                //TransformSelectedTemplate();

                //TransformSelectedRepositoryTemplate(templatename);
            }
        }

        private void TransformSelectedRepositoryTemplate(string templatename)
        {

            RunThreadedTransform(templatename);
            tspTime.Text = "Generated @ " + DateTime.Now.ToString();


        }

        private void toolStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void lvWork_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        

        private void lvRepository_ItemChecked(object sender, ItemCheckedEventArgs e)
        {


            if (e.Item.Checked)
            {
                m_RepoManager.AddWIP((string)e.Item.Tag);

            //    ListViewItem itemWIP = new ListViewItem(e.Item.Text);
              //  itemWIP.Tag = e.Item.Tag;

                //lvWork.Items.Add(itemWIP).SubItems.Add("-") ;

                //lvRepository.Items.Remove(e.Item);
                //lvWork.Items.Add(e.Item);
            }
        }

        private void lvWork_ItemChecked(object sender, ItemCheckedEventArgs e)
        {

        }

        private void tsbWorkUpload_Click(object sender, EventArgs e)
        {
            if (CopyWorkToPost())
            {
                PostCache();
                StartUpload();
            }
        }

        private bool StartUpload()
        {
            tabControl1.SelectedTab = tabControl1.TabPages[3];
            m_browserUpload.Load("http://ckcm.healthy.bewell.ca:10081/init,FOLDER4,VGhpcyBpcyB0aGUgSW1wbGVtZW50YXRpb24gTm90ZQ==,am9uLmJlZWJ5,UGE1NXdvcmQ=");

            /*
                        wbUpload.Url = new Uri("http://ckcm.healthy.bewell.ca:10081/init,FOLDER4,VGhpcyBpcyB0aGUgSW1wbGVtZW50YXRpb24gTm90ZQ==,am9uLmJlZWJ5,UGE1NXdvcmQ=");
                        wbUpload.Dock = DockStyle.Fill;
                        wbUpload.Visible = true;
            */


            return true;
        }


        private void Empty(System.IO.DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        private bool CopyWorkToPost()
        {
            DirectoryInfo pushdir = new DirectoryInfo(m_PushDir);
            Empty(pushdir);


            string filepath = "";
            try
            {

                foreach (ListViewItem item in lvWork.Items)
                {
                    filepath = dictFileToPath[item.Text];
                    File.Copy(filepath, m_PushDir + @"\" + Path.GetFileName(filepath));
                }
            }
            catch (Exception e)
            {
                throw e;
            }


            return true;
        }


        private bool PostCache()
        {
            bool result = false;

            gDAMPort = "10091"; // DEV
            gFolderName = "FOLDER4";
            gCacheName = "LOCAL3";

            string zipname = @"c:\temp\dambuddy2\togo-" + gCacheName + ".zip";
            //try
            {
                //string directory = gCacheDir + "\\" + gFolderName;
                string directory = m_PushDir;

                if (File.Exists(zipname))
                {
                    File.Delete(zipname);
                }

                ZipFile.CreateFromDirectory(directory, zipname);

                //Directory.Move(directory, directory + "-posted");

                long length = new System.IO.FileInfo(zipname).Length;
                Console.WriteLine("\nSending file length: {0}", length);

                using (WebClient client = new WebClient())
                {
                    byte[] responseArray = client.UploadFile(gServerName + ":" + gDAMPort + "/upload," + gFolderName, "POST", zipname);
                    // Decode and display the response.
                    Console.WriteLine("\nResponse Received. The contents of the file uploaded are:\n{0}",
                        System.Text.Encoding.ASCII.GetString(responseArray));
                }
                result = true;
            }

            return result;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timerRepoFilter.Enabled = false;
            DisplayTemplates();

        }




        private void tstbRepositoryFilter_KeyDown(object sender, KeyEventArgs e)
        {
            timerRepoFilter.Enabled = false;
        }

        private void tsbRepositoryFilterClear_Click(object sender, EventArgs e)
        {
            tstbRepositoryFilter.Text = "";
        }


        private void AddSearchResult(string sResult)
        {/*
            if (lvRepoSearchResults.InvokeRequired)
            {
                ControlCallback callback = new ControlCallback((s) =>
                {
                    Console.WriteLine("callback add result: " + s);
                    lvRepoSearchResults.Items.Add(new ListViewItem(s));
                });

                Console.WriteLine("invoke add result: " + sResult);
                lvRepoSearchResults.Invoke(callback, new object[] { sResult });
            }
            else
            {
                Console.WriteLine("direct add result: " + sResult);
                lvRepoSearchResults.Items.Add(new ListViewItem(sResult));

            }
*/

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { this.AddSearchResult(sResult); });
                return;
            }
            if (sResult == null) return;
            if (sResult.Trim() == "") return;
            lvRepoSearchResults.Items.Add(new ListViewItem(Path.GetFileName(sResult)));
            //lvRepoSearchResults.Columns[0].Width = -1;

        }

        private void SetAssetStale( string filename )
        {

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { this.SetAssetStale(filename); });
                return;
            }
            if (filename == null) return;
            if (filename.Trim() == "") return;
            lvRepoSearchResults.Items.Add(new ListViewItem(Path.GetFileName(filename)));

            foreach( ListViewItem item in lvWork.Items)
            {
                if ( item.Text == filename ) {
                    item.SubItems[1].Text = "STALE";
                }
            }
            UpdateWorkViewTitle();
        }


        private void DoSearch()
        {

        }


        private void tsbRepoSearch_Click(object sender, EventArgs e)
        {
            if (tstbRepositorySearch.Text == "") return;
            tstbRepositorySearch.Enabled = false;
            tsbRepoSearch.Enabled = false;
            lvRepoSearchResults.Items.Clear();
            tcRepoResults.SelectedIndex = 1;
            tcRepoResults.TabPages[1].Text = "Searching... - " + tstbRepositorySearch.Text;

            string path = @"C:\Users\jonbeeby\source\repos\DamBuddy2\packages\grep\";
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {

                        FileName = path + "fgrep.exe",
                        Arguments = tstbRepositorySearch.Text + " " + m_RepoPath + " -Rli",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };


                process.OutputDataReceived += new DataReceivedEventHandler((s, eData) =>
                   {
                       Console.WriteLine(eData.Data);

                       AddSearchResult(eData.Data);
                       
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

            tsbRepoSearch.Enabled = true;
            tstbRepositorySearch.Enabled = true;
            tcRepoResults.TabPages[1].Text = "Search Results - " + tstbRepositorySearch.Text;

        }

        private void lvRepoSearchResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvRepoSearchResults.SelectedItems.Count > 0)
            {

                //RunTransform();
                string filename = lvRepoSearchResults.SelectedItems[0].Text;
                string filepath = dictFileToPath[filename];


                //string templatename = BuildDictionaries(filepath);

                //TransformSelectedTemplate();
                RunThreadedTransform(filename);
                //TransformSelectedRepositoryTemplate(templatename);
            }
        }

        private void toolStripButton1_Click_4(object sender, EventArgs e)
        {
            Form2 test = new Form2();
            test.Ticket = m_RepoPath;            
            test.ShowDialog();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("");
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            

        }

        private void tsmWIPAdd_Click(object sender, EventArgs e)
        {
            SendKeys.Send("{ESC}"); // 
            MenuItem item = (MenuItem)sender;
            if (item.Tag != null)
            {
                string itemdata = (string)item.Tag;
                m_RepoManager.AddWIP(itemdata);

            }
        }

        private void tsmWIPRemove_Click(object sender, EventArgs e)
        {
            SendKeys.Send("{ESC}"); // 
            MenuItem item = (MenuItem)sender;
            //MessageBox.Show(item.Text);
            string itemData = item.Tag.ToString();

            var items = itemData.Split('~');
            
            if ( items.Length > 1 )
            {
                m_RepoManager.RemoveWIP(items[0], items[1]);
            }
            /*string filename = itemData.Substring(0, seperator);
            string originalpath = itemData.Substring( seperator + 1, itemData.Length - seperator);

            m_RepoManager.RemoveWIP(filename, originalpath);*/
        }

        private void lvWork_MouseUp(object sender, MouseEventArgs e)
        {
            bool match = false;


            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {

                Console.WriteLine(sender.ToString());

                foreach (ListViewItem item in lvWork.Items)
                {
                    if (item.Bounds.Contains(new System.Drawing.Point(e.X, e.Y)))
                    {
                        //MenuItem[] mi = new MenuItem[1];//{ new MenuItem("Remove " + item.Text), new MenuItem("World"), new MenuItem(item.Text) };

                        var Remove = new MenuItem(("Remove " + item.Text));
                        Remove.Tag = item.Text + "~" + item.Tag;
                        Remove.Enabled = true;
                        Remove.Click += tsmWIPRemove_Click;



                        MenuItem[] mi = { Remove };

                        //mi.Append(Remove);
                        lvWork.ContextMenu = new ContextMenu(mi);
                        match = true;
                        break;
                    }
                }
                if (match )
                {
                    Console.WriteLine("showing context menu for lvWork");
                    lvWork.ContextMenu.Show(lvWork, new System.Drawing.Point(e.X, e.Y));
                }
                else
                {
                    //Show listViews context menu
                }

            }
        }

        private void lvRepository_MouseUp(object sender, MouseEventArgs e)
        {
            bool match = false;


            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {

                Console.WriteLine(sender.ToString());

                foreach (ListViewItem item in lvRepository.Items)
                {
                    if (item.Bounds.Contains(new System.Drawing.Point(e.X, e.Y)))
                    {
                        //MenuItem[] mi = new MenuItem[1];//{ new MenuItem("Remove " + item.Text), new MenuItem("World"), new MenuItem(item.Text) };

                        var bEnabled = !m_RepoManager.isAssetinWIP(item.Text);
                        string prefix = "";

                        if (!bEnabled) prefix = "Already in WIP : ";
                        else prefix = "Add ";
                        
                        var Add = new MenuItem((prefix + item.Text));
                        Add.Tag = item.Tag;

                        Add.Enabled = bEnabled;
                        Add.Click += tsmWIPAdd_Click;



                        MenuItem[] mi = { Add };

                        //mi.Append(Remove);
                        lvRepository.ContextMenu = new ContextMenu(mi);
                        match = true;
                        break;
                    }
                }
                if (match)
                {
                    
                    lvRepository.ContextMenu.Show(lvRepository, new System.Drawing.Point(e.X, e.Y));
                }
                else
                {
                    //Show listViews context menu
                }

            }

        }

        private void tsWorkReload_Click(object sender, EventArgs e)
        {

        }

        private void tsbRepositoryReload_Click(object sender, EventArgs e)
        {
            LoadRepositoryTemplates();
        }
    }
}
