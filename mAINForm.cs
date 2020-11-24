
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
using org.w3c.dom.html;
using System.Text;
using System.Drawing;
//using CefSharp.Winforms;

namespace DAMBuddy2
{

    public partial class MainForm : Form
    {

        
        public MainForm()
        {
            InitializeComponent();
        }

        private static string DAM_UPLOAD_URL = "http://ckcm.healthy.bewell.ca:10081/init,FOLDER4,VGhpcyBpcyB0aGUgSW1wbGVtZW50YXRpb24gTm90ZQ==,am9uLmJlZWJ5,UGE1NXdvcmQ=";

        private RepoManager m_RepoManager;
        private bool m_IsClosing = false;        
        private int mCurrentPage;
        private int mTotalItems;
        private int mPageSize = 200;

        private delegate void ControlCallback(string s);
        private System.Drawing.Point m_ptScrollPos;
        private string m_currentHTML;
        private string m_currentDocumentWIP = "";
        private string m_currentDocumentRepo = ""; // used to track whether the same document is being viewed/reviewed, if so we should keep the position

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

        private ChromiumWebBrowser m_browserUpload;
        private ChromiumWebBrowser m_browserSchedule;

        private Dictionary<string, List<string>> dictTemplateChildren;
        private Dictionary<string, string> dictIdName;
        private Dictionary<string, List<string>> dictIdArchetypes;
        private static TransformRequestBuilder m_RequestBuilder;
        private bool gSearchDocumentRep;

        public void DisplayRepoTransformedDocumentCallback(string filename)
        {
            DisplayTransformedDocumentRepo(filename);
        }

        public void DisplayTransformedDocumentWIP(string filename)
        {
            
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { this.DisplayTransformedDocumentWIP(filename); });
                return;
            }
            if (filename == null) return;
            if (filename.Trim() == "") return;

            try
            {
                wbWIP.Url = new Uri(filename);

            }
            catch { }
        }

        public void DisplayTransformedDocumentRepo(string filename)
        {
            if (m_IsClosing) return;

            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate { this.DisplayTransformedDocumentRepo(filename); });
                    return;
                }
                if (filename == null) return;
                if (filename.Trim() == "") return;

                wbRepositoryView.Url = new Uri(filename);

            } catch (Exception e )
            {
                Console.WriteLine($"DisplayTransformedDocumentRepo() : {e.Message}");
            }

        }

        public void TicketStateChangeCallback(string ready)
        {
            if (m_IsClosing) return;

            if (ready == "True")
            {
                tslReadyState.Text = "Work: Ready";
                tsbPause.Enabled = true;
                tsbStart.Enabled = false;
            }
            else
            {
                tslReadyState.Text = "Work: Paused";
                tsbStart.Enabled = true;
                tsbPause.Enabled = false;
            }
        }


            
        public void ScheduleStateChangeCallback(string jsonStatus)
        {
            if (m_IsClosing) return;

            //MessageBox.Show(state.ScheduleState + ": Upload " + state.UploadEnabled);

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { this.ScheduleStateChangeCallback(jsonStatus); });
                return;
            }
            RepoManager.TicketScheduleState state = System.Text.Json.JsonSerializer.Deserialize<RepoManager.TicketScheduleState>(jsonStatus);

            if (state.UploadEnabled == "true")
            {
                tsbWorkUpload.Enabled = true;
            }
            else { tsbWorkUpload.Enabled = false; }

            tslScheduleStatus2.Text = state.ScheduleState;

            if (state.ScheduleState == "In Progress")
            {
                toolStrip2.BackColor = Color.FromArgb(0, 185, 97);
                foreach (ToolStripItem c in toolStrip2.Items)
                {
                    Type sType = c.GetType();
                    if ( sType.Name == "ToolStripTextBox"  )
                    {
                        continue;
                    }
                    c.ForeColor = Color.White;

                }
            }
            if (state.ScheduleState == "Blocked")
            {

                toolStrip2.BackColor = Color.FromArgb(244, 206, 70);
                
            }

            if (state.ScheduleState == "Not Yet Ready")
            {

                toolStrip2.BackColor = Color.FromArgb(197, 196, 193);

                foreach (ToolStripItem c in toolStrip2.Items)
                {
                    c.ForeColor = Color.Black;

                }
            }

        }

        public void StaleCallback(string filename)
        {
            if (m_IsClosing) return;
            SetAssetStale(filename);
        }

        public void UpdateWorkViewTitle()
        {
            string AssetCount = lvWork.Items.Count.ToString();
            int stalecount = 0;
            foreach (ListViewItem item in lvWork.Items)
            {
                if (item.SubItems[1].Text == "STALE") stalecount++;
            }

            if (stalecount == 0)
            {
                tpWIP.Text = "Work View (" + AssetCount + ")";
            }
            else
            {
                tpWIP.Text = "Work View (" + lvWork.Items.Count.ToString() + " - " + stalecount.ToString() + "!)";
            }

        }

        public void RemoveWIPCallback(string filename)
        {
            if (m_IsClosing) return;
            foreach (ListViewItem item in lvWork.Items)
            {
                if (item.Text == filename)
                {
                    lvWork.Items.Remove(item);
                    break;
                }
            }

            UpdateWorkViewTitle();
                    }

        public void DisplayWIPCallback(string filename)//, string originalpath)
        {
            if (m_IsClosing) return;

            ListViewItem newitem = new ListViewItem(filename);
            newitem.SubItems.Add("Fresh");
            newitem.SubItems.Add("Unchanged");
            lvWork.Items.Add(newitem);
            lvWork.Columns[0].Width = -1;

            UpdateWorkViewTitle();
        }


        public void WIPModifiedCallback(string filename)
        {
            if (m_IsClosing) return;

            SetAssetModified(filename);
        
        }

        public void TicketUpdateStateCallback( string TicketId, RepoManager.TicketChangeState state )
        {
            MessageBox.Show("mainform : received state update " + TicketId);
        }

        private void InitializeApp()
        {
            var appsettings = ConfigurationManager.AppSettings;
            AppSettingsSection settings = (AppSettingsSection)ConfigurationManager.GetSection("PreviewView.Properties.Settings");

            mCurrentPage = 0;
            tsbStart.Enabled = true;
            tsbPause.Enabled = false;
            tsbWord.Enabled = false;
            tstbRepositoryFilter.Text = "";

            m_RepoManager = new RepoManager( StaleCallback, DisplayWIPCallback, RemoveWIPCallback, WIPModifiedCallback);
            m_RepoManager.CallbackScheduleState = ScheduleStateChangeCallback;
            m_RepoManager.CallbackTicketState = TicketStateChangeCallback;
            m_RepoManager.CallbackUploadState = TicketUpdateStateCallback;


            m_browserSchedule = new ChromiumWebBrowser("http://ckcm:10008/scheduler-plan.html"); // TODO:Fix port
            m_browserUpload = new ChromiumWebBrowser("https://google.ca");

            m_browserUpload.CreateControl();

            tpUpload.Controls.Add(m_browserUpload);
            tpSchedule.Controls.Add(m_browserSchedule);
            m_OPTWebserviceUrl = appsettings["OPTServiceUrl"] ?? "App Settings not found";
            m_CacheServiceURL = appsettings["CacheServiceUrl"] ?? "App Settings not found";



            m_RepoManager.Init(30000 * 1, 60000 * 1);
            m_RepoManager.GetTicketScheduleStatus();

            this.Text = "BuildBuddy v" + GetLocalVersionNumber();

            m_RequestBuilder = new TransformRequestBuilder(m_RepoManager, appsettings["QueryServiceUrl"] ?? "App Settings not found");

            InitAvailableRepos();


            PrepareTransformSupport();
            LoadRepositoryTemplates();
        }


        private void InitAvailableRepos()
        {
            var listRepos = m_RepoManager.GetAvailableRepositories();
            tsddbRepository.DropDownItems.Clear();
            FontFamily fontFamily = new FontFamily("Arial Unicode MS");
            System.Drawing.Font theFont = new System.Drawing.Font(
               fontFamily,
               9.75f,
               FontStyle.Regular,
               GraphicsUnit.Point);

            foreach ( var item in listRepos)
            {
                var option = tsddbRepository.DropDownItems.Add(item);

                option.Font = theFont;
                option.Tag = item;
                option.Height = 30;
                option.ImageScaling = ToolStripItemImageScaling.None;
                
                option.Click += tsmiAvailableRepo_Click;
                
            }

            ToolStripSeparator sep = new ToolStripSeparator();
            tsddbRepository.DropDownItems.Add(sep);

            ToolStripMenuItem addNew = new ToolStripMenuItem("Setup New Ticket...", DAMBuddy2.Properties.Resources.outline_confirmation_number_black_24dp);
            addNew.Click += setupNewTicketToolStripMenuItem_Click;
            addNew.Font = theFont;
            addNew.Height = 30;
            addNew.ImageScaling = ToolStripItemImageScaling.None;
            tsddbRepository.DropDownItems.Add(addNew);

            tsddbRepository.Text = m_RepoManager.GetCurrentRepository();
            tslWorkRepository.Text = m_RepoManager.GetCurrentRepository();

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
                string entryfullname = Path.Combine(m_RepoManager.TicketFolder, entry.FullName);
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



        private static string GetLocalVersionNumber()
        {

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, latestVersionInfoFile);

            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return null;
        }

        public static HttpWebRequest CreateSOAPWebRequest()
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



        private void DisplayTemplates2()
        {

            tstbRepositoryFilter.Focus();
            lvRepository.Items.Clear();
            lblPageCount.Text = "";
            // This filters and adds your filtered items to listView1

            int availablePages = -1;

            try
            {
                //lvRepository.SuspendLayout();
                
                int count = 0;
                //    ..LockWindowUpdate(this.Handle);
                //foreach (ListViewItem item in m_masterlist.Where(lvi => lvi.Text.ToLower().Contains(tstbRepositoryFilter.Text.ToLower().Trim())))
                //var aPage = m_RepoManager.Masterlist.Page(mCurrentPage);

                int numberOfObjectsPerPage = 200;
                int pageNumber = mCurrentPage;
                mTotalItems  = m_RepoManager.Masterlist.Where(lvi => lvi.Text.ToLower().Contains(tstbRepositoryFilter.Text.ToLower().Trim())).Count();


            
                availablePages = mTotalItems / mPageSize;
                lblPageCount.Text = $"{mCurrentPage + 1}/{availablePages + 1}";

                var RepoPage = m_RepoManager.Masterlist.Where(lvi => lvi.Text.ToLower().Contains(tstbRepositoryFilter.Text.ToLower().Trim())).Skip(numberOfObjectsPerPage * pageNumber).Take(numberOfObjectsPerPage);

                //var RepoPage = m_RepoManager.Masterlist.Skip(numberOfObjectsPerPage * pageNumber).Take(numberOfObjectsPerPage);

                Console.WriteLine($"count {RepoPage.Count()}");

                //foreach ( var page in aPage)
                {
                    foreach (var item in RepoPage)
                       // foreach ( var item in page.Where(lvi => lvi.Text.ToLower().Contains(tstbRepositoryFilter.Text.ToLower().Trim())))
                    {

                        try
                        {
                            lvRepository.Items.Add(item);

                            // clbRepository.Items.Add(item.Text);
                        }
                        catch { }

                        count++;
                        //if (count > 1000) break;
                    }
                }
           }
            finally
            {
                //  LockWindowUpdate(0);
                //lvRepository.ResumeLayout();
                lvRepository.Columns[0].Width = -1;
            }

            if (mCurrentPage == 0) btnPrev.Visible = false;

            if( mCurrentPage == availablePages )
            {
                btnNext.Visible = false;
                
            }
            else
            {
                btnNext.Visible = true;
            }

        }

        private void DisplayRepoStatusUpdateCallback(string sStatus)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { this.DisplayRepoStatusUpdateCallback(sStatus); });
                return;
            }
            if (sStatus == null) return;
            if (sStatus.Trim() == "") return;


            toolStripProgressBar2.PerformStep();



            tspStatusLabel.Text = sStatus;
        }

        private void DisplayWIPStatusUpdateCallback(string sStatus)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { this.DisplayWIPStatusUpdateCallback(sStatus); });
                return;
            }
            //            if (sStatus == null) return;
            //            if (sStatus.Trim() == "") return;

            //tsPBWIPTransform.PerformStep();
            tsStatusLabel.Text = sStatus;
        }

        private void RunThreadedTransformWIP(string sTemplateName)
        {

            var args = new TransformArgs();
            args.sTemplateName = sTemplateName;
            args.sTemplateFilepath = m_RepoManager.GetTemplateFilepath(sTemplateName);
            args.callbackDisplayHTML = DisplayTransformedDocumentWIP;
            args.callbackStatusUpdate = DisplayWIPStatusUpdateCallback;
            tsPBWIPTransform.Step = 1;
            tsPBWIPTransform.Value = 0;
            tsPBWIPTransform.Minimum = 0;
            tsPBWIPTransform.Maximum = 5;
            tsPBWIPTransform.Visible = true;

            DisplayWIPStatusUpdateCallback("Transforming " + sTemplateName + ": Building SOAP Request...");

            Thread thread1 = new Thread(RunTransform);
            thread1.Start(args);
            m_currentDocumentWIP = sTemplateName;
        }


        private void RunThreadedTransformRepo(string sTemplateName)
        {
            var args = new TransformArgs();
            args.sTemplateName = sTemplateName;
            args.sTemplateFilepath = m_RepoManager.GetTemplateFilepath(sTemplateName);
            args.callbackDisplayHTML = DisplayRepoTransformedDocumentCallback;
            args.callbackStatusUpdate = DisplayRepoStatusUpdateCallback;
            toolStripProgressBar2.Step = 1;
            toolStripProgressBar2.Value = 0;
            toolStripProgressBar2.Minimum = 0;
            toolStripProgressBar2.Maximum = 5;
            toolStripProgressBar2.Visible = true;

            toolStripProgressBar1.Step = 1;
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Minimum = 0;
            toolStripProgressBar1.Maximum = 5;
            toolStripProgressBar1.Visible = false;

            DisplayRepoStatusUpdateCallback("Transforming " + m_currentDocumentRepo + ": Building SOAP Request...");

            Thread thread1 = new Thread(RunTransform);
            thread1.Start(args);
            m_currentDocumentRepo = sTemplateName;
        }

        private void RunTransform(object oArgs)
        {
            TransformArgs theArgs = (TransformArgs)oArgs;
            
            string sTemplateName = theArgs.sTemplateName;
            Cursor.Current = Cursors.WaitCursor;
            string sSelectedTransform = m_RepoManager.TicketFolder + @"\XSLT\OrderItem.xsl";
            string sProcessedName = sTemplateName.Replace(".oet", "").TrimEnd();



            if (sProcessedName.EndsWith("Panel", StringComparison.OrdinalIgnoreCase) ||
                 sProcessedName.EndsWith("Protocol", StringComparison.OrdinalIgnoreCase) ||
                 sProcessedName.EndsWith("Set", StringComparison.OrdinalIgnoreCase) ||
                 sProcessedName.EndsWith("Template", StringComparison.OrdinalIgnoreCase) ||
                 sProcessedName.EndsWith("Group", StringComparison.OrdinalIgnoreCase))
            {
                sSelectedTransform = m_RepoManager.TicketFolder + @"\XSLT\OrderSet.xsl";
            }

            try
            {
                string sTempHTML = @"c:\temp\" + Guid.NewGuid().ToString() + @".html";

                HttpWebRequest wr = CreateSOAPWebRequest();
                XmlDocument SOAPReqBody = new XmlDocument();
                String optContents = "";

                try
                {
                    //string sSOAPRequest = BuildSOAPRequest3(dictFileToPath[sTemplateName]);
                    string sSOAPRequest = BuildSOAPRequest3(theArgs.sTemplateFilepath);
                    SOAPReqBody.LoadXml(sSOAPRequest);
                    //File.WriteAllText(@"C:\temp\SOAPRequest.xml", sSOAPRequest);

                    using (Stream stream = wr.GetRequestStream())
                    {
                        SOAPReqBody.Save(stream);
                    }

                    theArgs.callbackStatusUpdate("Transforming " + sTemplateName + ": Requesting OPT document..." + "( " + (System.Text.ASCIIEncoding.ASCII.GetByteCount(sSOAPRequest) / 1024).ToString() + "KB )");

                    System.Windows.Forms.Application.DoEvents();
                    try
                    {


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
                    catch { return; }
                    if (String.IsNullOrEmpty(optContents))
                        return;

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
                    return;
                    //   throw ex;
                }

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

        private static string BuildSOAPRequest3(string sTemplateFilepath)
        {
            string theRequest = "";
            m_RequestBuilder.BuildRequest(sTemplateFilepath, ref theRequest);
            return theRequest;
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

            if (m_IsClosing) return;

            m_currentHTML = wbRepositoryView.Url.ToString();
            tsbWord.Enabled = true;
            Cursor.Current = Cursors.Default;

            tspStatusLabel.Text = "Viewing " + m_currentDocumentRepo;

            toolStripProgressBar2.Visible = false;

            if (!String.IsNullOrEmpty(tstbRepositorySearch.Text))
            {
                if (!gSearchDocumentRep)
                {
                    string highlightedHtml = HighlightHtml(tstbRepositorySearch.Text, wbRepositoryView.Document);
                    wbRepositoryView.DocumentText = highlightedHtml;
                    gSearchDocumentRep = true; // avoids re-triggering the highlight in a loop 
                }
                else
                {
                    gSearchDocumentRep = false;
                }

                //wbRepositoryView.Document.Body.OuterHtml = highlightedHtml;

            }
        }

        private void OpenInWord()
        {
            var word = new Microsoft.Office.Interop.Word.Application();
            word.Visible = true;
            string TemplateFilename = dictIdName[lvRepository.SelectedItems[0].Text];
            string newFilename = m_RepoManager.WIPPath + Path.GetFileNameWithoutExtension(TemplateFilename) + ".html";
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
                string filename = lvRepository.SelectedItems[0].Text;
                //string filepath = dictFileToPath[filename];
                string filepath = m_RepoManager.GetTemplateFilepath( filename );
                RunThreadedTransformRepo(filename);
                LoadRepoWUR(filename);
            }
        }

        private void TransformSelectedRepositoryTemplate(string templatename)
        {

            RunThreadedTransformRepo(templatename);
            tspTime.Text = "Generated @ " + DateTime.Now.ToString();


        }

        private void toolStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }


        private void LoadWURWIP(string filename)
        {
            //http://ckcm:8011/WhereUsed,0f3e3fc2-6dbe-4f6f-b292-e8ef0501c163
            string sTID = m_RepoManager.GetTemplateID(filename);
            wbWIPWUR.ScriptErrorsSuppressed = true;
            wbWIPWUR.Url = new Uri("http://ckcm:8011/WhereUsed," + sTID);

        }

        private void LoadRepoWUR(string filename)
        {
            //http://ckcm:8011/WhereUsed,0f3e3fc2-6dbe-4f6f-b292-e8ef0501c163
            string sTID = m_RepoManager.GetTemplateID(filename);
            wbRepoWUR.ScriptErrorsSuppressed = true;
            wbRepoWUR.Url = new Uri("http://ckcm:8011/WhereUsed," + sTID);

        }

        private void lvWork_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (lvWork.SelectedItems.Count > 0)
            {
                string filename = lvWork.SelectedItems[0].Text;
                //string filepath = m_RepoManager.GetTemplateFilepath(filename);//dictFileToPath[filename];
                RunThreadedTransformWIP(filename);
                LoadWURWIP(filename);
            }


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



        private void tsbWorkUpload_Click(object sender, EventArgs e)
        {
            m_RepoManager.PostWIP();
            //PostCache();
            StartUpload();

            
        }

        private bool StartUpload()
        {
            tabControl1.SelectedTab = tabControl1.TabPages[3];

            string url = m_RepoManager.PrepareForUpload();

            m_browserUpload.Load(url);


            return true;
        }



        private void timer2_Tick(object sender, EventArgs e)
        {
            timerRepoFilter.Enabled = false;
            string filter = tstbRepositoryFilter.Text;


            m_RepoManager.ApplyFilter(filter);
            
            if( string.IsNullOrEmpty(filter ))
            {
                lblFilter.Text = "";
            } 
            else   lblFilter.Text = $"Filter: Name contains {filter}";
            mCurrentPage = 0;
            DisplayTemplates2();

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
        {

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


        private void SetAssetModified(string filename)
        {

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { this.SetAssetModified(filename); });
                return;
            }
            if (filename == null) return;
            if (filename.Trim() == "") return;

            foreach (ListViewItem item in lvWork.Items)
            {
                if (item.Text == filename)
                {
                    item.SubItems[2].Text = "CHANGED";
                }
            }

        }

        private void SetAssetStale(string filename)
        {

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { this.SetAssetStale(filename); });
                return;
            }
            if (filename == null) return;
            if (filename.Trim() == "") return;
            //lvRepoSearchResults.Items.Add(new ListViewItem(Path.GetFileName(filename)));

            foreach (ListViewItem item in lvWork.Items)
            {
                if (item.Text == filename)
                {
                    item.SubItems[1].Text = "STALE";
                }
            }
            UpdateWorkViewTitle();
        }


        private void SearchThreaded(string sSearchTerms)
        {
            if (sSearchTerms.Equals("")) return;

            tstbRepositorySearch.Enabled = false;
            tsbRepoSearch.Enabled = false;
            lvRepoSearchResults.Items.Clear();
            tcRepoResults.SelectedIndex = 1;
            tcRepoResults.TabPages[1].Text = "Searching... - " + tstbRepositorySearch.Text;

            SearchArgs args = new SearchArgs();

            args.callbackFinishedSearch = SearchFinished;
            args.callbackAddResult = AddSearchResult;
            args.sSearchTerm = sSearchTerms;

            Thread thread1 = new Thread(RunThreadedSearch);
            thread1.Start(args);

        }

        private void SearchFinished(string sSearchTerms)
        {


            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { this.SearchFinished(sSearchTerms); });
                return;
            }

            tsbRepoSearch.Enabled = true;
            tstbRepositorySearch.Enabled = true;
            tcRepoResults.TabPages[1].Text = "Search Results - " + sSearchTerms;

            lvRepoSearchResults.Columns[0].Width = -1;
        }

        private void RunThreadedSearch(object oArgs)
        {
            SearchArgs theArgs = (SearchArgs)oArgs;

            string assetpath = m_RepoManager.AssetPath;
            if (assetpath.EndsWith(@"\"))
            {
                assetpath = assetpath.Remove(assetpath.Length - 1);
            }

            //if ( theArgs == null) return;

            string path = @"C:\Users\jonbeeby\source\repos\DamBuddy2\packages\grep\";
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {

                        FileName = path + "grep.exe",
                        Arguments = theArgs.sSearchTerm + " " + assetpath + " -Rli --include=\"*.oet\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };


                process.OutputDataReceived += new DataReceivedEventHandler((s, eData) =>
                {
                    Console.WriteLine(eData.Data);

                    theArgs.callbackAddResult(eData.Data); //AddSearchResult(eData.Data);

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

            theArgs.callbackFinishedSearch(theArgs.sSearchTerm);
        }

        private void tsbRepoSearch_Click(object sender, EventArgs e)
        {
            SearchThreaded(tstbRepositorySearch.Text);
        }
        private void lvRepoSearchResults_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (lvRepoSearchResults.SelectedItems.Count > 0)
            {
                string filename = lvRepoSearchResults.SelectedItems[0].Text;
                string filepath = m_RepoManager.GetTemplateFilepath(filename);//dictFileToPath[filename];
                RunThreadedTransformRepo(filename);
            }
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

            if (items.Length > 1)
            {
                m_RepoManager.RemoveWIP(items[0]);//, items[1]);
            }
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
                if (match)
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

        private void LoadRepositoryTemplates()
        {
            m_RepoManager.LoadRepositoryTemplates();
            mCurrentPage = 0;
            DisplayTemplates2();
        }

        private void tsbRepositoryReload_Click(object sender, EventArgs e)
        {
            LoadRepositoryTemplates();
        }

        private void wbWIP_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (m_IsClosing) return;

            //m_currentHTML = wbRepositoryView.Url.ToString();
            tsbWordWIP.Enabled = true;
            Cursor.Current = Cursors.Default;

            tsStatusLabel.Text = "Viewing " + m_currentDocumentWIP;

            tsPBWIPTransform.Visible = false;

        }

        private string HighlightHtml(string SearchText, HtmlDocument doc2)
        {
            //mshtml.IHTMLDocument2 doc2 = WebBrowser.Document.DomDocument;
            string ReplacementTag = "<span style='background-color: rgb(255, 255, 0);'>";
            StringBuilder strBuilder = new StringBuilder(doc2.Body.OuterHtml);
            string HTMLString = strBuilder.ToString();
            //if (this.m_NoteType == ExtractionNoteType.SearchResult)
            {
                List<string> SearchWords = new List<string>();

                SearchText = SearchText.Replace('"', ' ');
                SearchWords.AddRange(SearchText.Trim().Split(' '));
                foreach (string item in SearchWords)
                {
                    int index = HTMLString.IndexOf(item, 0, StringComparison.InvariantCultureIgnoreCase);
                    // 'If index > 0 Then
                    while ((index > 0 && index < HTMLString.Length))
                    {
                        HTMLString = HTMLString.Insert(index, ReplacementTag);
                        HTMLString = HTMLString.Insert(index + item.Length + ReplacementTag.Length, "</span>");
                        index = HTMLString.IndexOf(item, index + item.Length + ReplacementTag.Length + 7, StringComparison.InvariantCultureIgnoreCase);
                    }
                }
            }
            //else
            // {
            // }
            return HTMLString;
        }


        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (tabControl1.SelectedTab.Name == "tpOverlaps")
            {
                wbOverlaps.Url = new Uri("http://ckcm:10008/dynamic/OverlapFocus,CSDFK-1489");
            }
        }

        public class SearchArgs
        {
            public delegate void AddResultCallback(string s);
            public delegate void FinishedSearch(string s);

            public string sSearchTerm;
            public AddResultCallback callbackAddResult;
            public FinishedSearch callbackFinishedSearch;
        }

        public class TransformArgs
        {
            public delegate void DisplayCallback(string s);
            public delegate void StatusCallback(string s);
            public string sTemplateName;
            public string sTemplateFilepath;
            public DisplayCallback callbackDisplayHTML;
            public StatusCallback callbackStatusUpdate;
        }

        private void tsbRepositoryViewDocument_Click(object sender, EventArgs e)
        {
            if( tcRepoResults.SelectedTab == tcRepoResults.TabPages[0] )
            {

                if (lvRepository.SelectedItems.Count > 0)
                {
                    string filename = lvRepository.SelectedItems[0].Text;
                    string filepath = m_RepoManager.GetTemplateFilepath(filename); //dictFileToPath[filename];
                    RunThreadedTransformRepo(filename);
                }

            } else
            {

                if (lvRepoSearchResults.SelectedItems.Count > 0)
                {
                    string filename = lvRepoSearchResults.SelectedItems[0].Text;
                    string filepath = m_RepoManager.GetTemplateFilepath(filename); //dictFileToPath[filename];
                    RunThreadedTransformRepo(filename);
                }

            }

        }





        private void btnNext_Click(object sender, EventArgs e)
        {
            mCurrentPage++;
            if (mCurrentPage > 0) btnPrev.Visible = true;
            lblPageCount.Text = $"{mCurrentPage+1}";
            DisplayTemplates2();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            mCurrentPage--;
            if (mCurrentPage == 0) btnPrev.Visible = false;
            lblPageCount.Text = $"{mCurrentPage + 1}";
            DisplayTemplates2();

        }

        private void tsbStart_Click(object sender, EventArgs e)
        {
            tslReadyState.Text = "Work: Ready";

            tsbStart.Enabled = false;
            tsbPause.Enabled = true;
            m_RepoManager.SetTicketReadiness(true);
        }

        private void tsbPause_Click(object sender, EventArgs e)
        {
            tslReadyState.Text = "Work: Paused";

            tsbStart.Enabled = true;
            tsbPause.Enabled = false;
            m_RepoManager.SetTicketReadiness(false);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_IsClosing = true;
            m_RepoManager.Closedown();
            //m_RepoManager.SaveExistingWip();
        }

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_RepoManager.TestCacheManager();
            
            //m_RepoManager.TestJira( "CKCMFK-1989");
        }

        private void tsbLaunchTD_Click(object sender, EventArgs e)
        {
            // setup config

            // start process
            LaunchTD();

        }


        private void LaunchTD()
        {
            m_RepoManager.ConfigureAndLaunchTD();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            LaunchTD();
        }



       


        private void setupNewTicketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetupTicketForm ticketform = new SetupTicketForm();
            if (ticketform.ShowDialog() == DialogResult.OK)
            {
                m_RepoManager.PrepareNewTicket(ticketform.m_TicketJSON);
            };
        }

        private void tsbHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TODO: Info on Scheduler State and implications...");
        }

        private void lvRepository_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void lvWork_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void tsmiAvailableRepo_Click(object sender, EventArgs e)
        {

            ToolStripMenuItem item = (ToolStripMenuItem )sender;

            string newRepo = item.Text;

            if (m_RepoManager.SetCurrentRepository(newRepo))
            {


                tsddbRepository.Text = newRepo;


//                InitAvailableRepos();


                PrepareTransformSupport();
                LoadRepositoryTemplates();

            }

        }
    }


}
