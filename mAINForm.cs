using CefSharp.WinForms;
using java.util;
using Saxon.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

//using CefSharp.Winforms;

namespace DAMBuddy2
{
    public partial class MainForm : Form
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public MainForm()
        {
            InitializeComponent();
        }

        private static string DAM_UPLOAD_URL = "http://ckcm.healthy.bewell.ca:10081/init,FOLDER4,VGhpcyBpcyB0aGUgSW1wbGVtZW50YXRpb24gTm90ZQ==,am9uLmJlZWJ5,UGE1NXdvcmQ=";
        private static string DAM_OVERLAP_URL = "http://ckcm:10008/dynamic/OverlapFocus,";
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
        private static string latestVersionInfoFile = "buildbuddy_version";

        private string m_PushDir = @"c:\temp\dambuddy2\togo";
        private static string m_OPTWebserviceUrl = ""; //@"http://wsckcmapp01/OptWs/OperationalTemplateBuilderService.asmx";

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

        /// <summary>
        /// Called (from RepoManager) when the transform process has completed for the Repository Doc Viewer
        /// </summary>
        /// <param name="filename"></param>
        public void callbackDisplayRepoTransformedDocument(string filename)
        {
            DisplayTransformedDocumentRepo(filename);
        }

        public void callbackUserInfoDisplay(string message)
        {
            toolStripStatusLabel1.Text = message;
        }

        /// <summary>
        /// Called (from RepoManager) when the transform process has completed for the WIP Doc Viewer
        /// </summary>
        /// <param name="filename"></param>
        public void callbackDisplayTransformedDocumentWIP(string filename)
        {
            DisplayTransformedDocumentWIP(filename);
        }

        /// <summary>
        /// Displays the transformed file in the WIP browser
        /// </summary>
        /// <param name="filename"></param>
        public void DisplayTransformedDocumentWIP(string filename)
        {
            if (m_IsClosing) return;

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
            catch (Exception ex) {
                Logger.Error(ex, ex.StackTrace);
                Logger.Error(ex, "Goodbye cruel world"); 
            }
        }

        /// <summary>
        /// Displays the transformed file in the Repository browser
        /// </summary>
        /// <param name="filename"></param>
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
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "$DisplayTransformedDocumentRepo() : { e.Message}");
            }
        }

        /// <summary>
        /// called (from RepoManager) when the "ready state" of the ticket changes
        /// </summary>
        /// <param name="ready"></param>
        public void callbackTicketStateChange(bool isReady)
        {
            if (m_IsClosing) return;

            if (isReady)
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

        /// <summary>
        /// called (from Repomanager) when the "schedule state" of the ticket changes.
        /// </summary>
        /// <remarks>
        /// This menthod updates the WIP toolbar color to match the state of the schedule and manages the state of the upload button
        /// </remarks>
        /// <param name="jsonStatus"></param>
        public void callbackScheduleStateChange(string jsonStatus)
        {
            if (m_IsClosing) return;

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { this.callbackScheduleStateChange(jsonStatus); });
                return;
            }
            RepoInstance.TicketScheduleState state = System.Text.Json.JsonSerializer.Deserialize<RepoInstance.TicketScheduleState>(jsonStatus);

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
                    if (sType.Name == "ToolStripTextBox")
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

        /// <summary>
        /// called (by the RepoManger) when a WIP asset becomes stale, i.e. has been changed in the master Repository after the asset was added to WIP
        /// </summary>
        /// <param name="filename"></param>
        public void callbackStale(string filename)
        {
            if (m_IsClosing) return;
            SetAssetStale(filename);
        }

        /// <summary>
        /// manages the title text of the WIP tab to give useful summary information on the state of WIP
        /// </summary>
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

        /// <summary>
        /// called (by RepoManager?) when an asset has been removed from WIP
        /// </summary>
        /// <param name="filename"></param>
        public void callbackRemoveWIP(string filename)
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

        /// <summary>
        /// called (by RepoManager?) when an asset has been added to WIP
        /// </summary>
        /// <param name="filename"></param>
        public void callbackDisplayWIP(string filename)
        {
            if (m_IsClosing) return;
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { this.callbackDisplayWIP(filename); });
                return;
            }

            ListViewItem newitem = new ListViewItem(filename);
            newitem.SubItems.Add("Fresh");
            newitem.SubItems.Add("Unchanged");
            lvWork.Items.Add(newitem);
            lvWork.Columns[0].Width = -1;

            UpdateWorkViewTitle();
        }

        /// <summary>
        /// called (by RepoManager) when an asset in WIP has been modified (by the editor)
        /// </summary>
        /// <param name="filename"></param>
        public void callbackWIPModified(string filename, string state)
        {
            if (m_IsClosing) return;

            SetAssetModified(filename, state);
        }

        public void callbackTicketUpdateState(string TicketId, RepoManager.TicketChangeState state)
        {
            
            InitAvailableRepos();
            InitView();
            
            //MessageBox.Show("mainform : received state update " + TicketId);
        }

        /// <summary>
        /// Initialization block: reads config, instantiates embedded objs, creates chromium browsers, sets callbacks for, and, creates RepoManager instance
        /// </summary>
        private void InitializeApp()
        {
            var appsettings = ConfigurationManager.AppSettings;
            AppSettingsSection settings = (AppSettingsSection)ConfigurationManager.GetSection("PreviewView.Properties.Settings");

            mCurrentPage = 0;
            tsbStart.Enabled = true;
            tsbPause.Enabled = false;
            tsbWord.Enabled = false;
            tstbRepositoryFilter.Text = "";

            RepoCallbackSettings callbacks = new RepoCallbackSettings();
            callbacks.callbackStale = callbackStale;
            callbacks.callbackDisplayWIP = callbackDisplayWIP;
            callbacks.callbackRemoveWIP = callbackRemoveWIP;
            callbacks.callbackModifiedWIP = callbackWIPModified;
            callbacks.callbackScheduleState = callbackScheduleStateChange;
            callbacks.callbackUploadState = callbackTicketUpdateState;
            callbacks.callbackTicketState = callbackTicketStateChange;
            callbacks.callbackInfo = callbackUserInfoDisplay;

            m_RepoManager = new RepoManager(callbacks);

            m_browserSchedule = new ChromiumWebBrowser("http://ckcm:10008/scheduler-plan.html"); // TODO:Fix port
            m_browserUpload = new ChromiumWebBrowser("about:blank");

            m_browserUpload.CreateControl();

            tpUpload.Controls.Add(m_browserUpload);
            tpSchedule.Controls.Add(m_browserSchedule);
            m_OPTWebserviceUrl = appsettings["OPTServiceUrl"] ?? "App Settings not found";
            
            toolStripStatusLabel1.Text = "";

            m_RepoManager.Init();//30000 * 1, 60000 * 1);
            
            this.Text = "BuildBuddy v" + GetLocalVersionNumber();

            m_RequestBuilder = new TransformRequestBuilder(m_RepoManager, appsettings["QueryServiceUrl"] ?? "App Settings not found");

            InitAvailableRepos();

            LoadRepositoryTemplates();
        }

        /// <summary>
        /// Manages the UI for switching current repository, loads the repository list from RepoManager
        /// </summary>
        private void InitAvailableRepos()
        {
            if( InvokeRequired ) 
            {
                BeginInvoke((MethodInvoker)delegate { this.InitAvailableRepos(); });
                return;
            }

            var listRepos = m_RepoManager.GetAvailableRepositories();
            tsddbRepository.DropDownItems.Clear();
            FontFamily fontFamily = new FontFamily("Arial Unicode MS");
            System.Drawing.Font theFont = new System.Drawing.Font(
               fontFamily,
               9.75f,
               FontStyle.Regular,
               GraphicsUnit.Point);

            foreach (var item in listRepos)
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



            if (m_RepoManager.CurrentRepo == null)
            {
                SetRepositoryTitle("Select a Ticket");
            }
            else
            {
                SetRepositoryTitle(m_RepoManager.CurrentRepo.TicketID);
            }
        }

        private void SetRepositoryTitle(string sTitle)
        {
            tsddbRepository.Text = sTitle;
            tslWorkRepository.Text = tsddbRepository.Text;
        }

        /// <summary>
        /// Downloads supporting files used in Transform process.
        /// </summary>
        /// <returns></returns>
        /// <remarks>files are archetype xml files which are managed/created on the server</remarks>

        /// <summary>
        /// Loads version from a local file (not assemmbly versions)
        /// </summary>
        /// <returns></returns>
        private static string GetLocalVersionNumber()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, latestVersionInfoFile);

            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return null;
        }

        /// <summary>
        /// Creates Listitems in Repostory listview using paged list from RepoManager and applies filtering based on user filtertext
        /// </summary>
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
                //foreach (ListViewItem item in m_MasterListAssets.Where(lvi => lvi.Text.ToLower().Contains(tstbRepositoryFilter.Text.ToLower().Trim())))
                //var aPage = m_RepoManager.MasterListAssets.Page(mCurrentPage);

                int numberOfObjectsPerPage = 200;
                int pageNumber = mCurrentPage;
                if (m_RepoManager.CurrentRepo == null)
                {
                    //MessageBox.Show("CurrentRepo is NULL");
                    return;
                }

                
                mTotalItems = m_RepoManager.CurrentRepo.Masterlist.Where(lvi => lvi.Text.ToLower().Contains(tstbRepositoryFilter.Text.ToLower().Trim())).Count();

                availablePages = mTotalItems / mPageSize;
                lblPageCount.Text = $"{mCurrentPage + 1}/{availablePages + 1}";

                var RepoPage = m_RepoManager.CurrentRepo.Masterlist.Where(lvi => lvi.Text.ToLower().Contains(tstbRepositoryFilter.Text.ToLower().Trim())).Skip(numberOfObjectsPerPage * pageNumber).Take(numberOfObjectsPerPage);

                //var RepoPage = m_RepoManager.MasterListAssets.Skip(numberOfObjectsPerPage * pageNumber).Take(numberOfObjectsPerPage);

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
                        catch (Exception ex) { Logger.Error(ex, "Goodbye cruel world"); }

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

            if (mCurrentPage == availablePages)
            {
                btnNext.Visible = false;
            }
            else
            {
                btnNext.Visible = true;
            }
        }

        /// <summary>
        /// Displays the progress of the Repository Document Transform
        /// </summary>
        /// <param name="sStatus"></param>
        private void callbackDisplayRepoStatusUpdate(string sStatus)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { this.callbackDisplayRepoStatusUpdate(sStatus); });
                return;
            }
            if (sStatus == null) return;
            if (sStatus.Trim() == "") return;

            toolStripProgressBar2.PerformStep();

            tspStatusLabel.Text = sStatus;
        }

        /// <summary>
        /// Displays the progress of the WIP Document Transform
        /// </summary>
        /// <param name="sStatus"></param>

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

        /// <summary>
        /// Creates a thread to execute the WIP Document transformation, passing callbacks for status and completion notifications
        /// </summary>
        /// <param name="sTemplateName"></param>
        private void RunThreadedTransformWIP(string sTemplateName)
        {
            var args = new TransformArgs();
            args.sTemplateName = sTemplateName;
            args.sTemplateFilepath = m_RepoManager.CurrentRepo.GetTemplateFilepath(sTemplateName);
            args.callbackDisplayHTML = callbackDisplayTransformedDocumentWIP;
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

        /// <summary>
        /// Creates a thread to execute the Repository Document transformation, passing callbacks for status and completion notifications
        /// </summary>
        /// <param name="sTemplateName"></param>
        private void RunThreadedTransformRepo(string sTemplateName)
        {
            var args = new TransformArgs();
            args.sTemplateName = sTemplateName;
            args.sTemplateFilepath = m_RepoManager.CurrentRepo.GetTemplateFilepath(sTemplateName);
            args.callbackDisplayHTML = callbackDisplayRepoTransformedDocument;
            args.callbackStatusUpdate = callbackDisplayRepoStatusUpdate;
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

            callbackDisplayRepoStatusUpdate("Transforming " + m_currentDocumentRepo + ": Building SOAP Request...");

            Thread thread1 = new Thread(RunTransform);
            thread1.Start(args);
            m_currentDocumentRepo = sTemplateName;
        }

        private void RunTransform(object oArgs)
        {
            TransformArgs theArgs = (TransformArgs)oArgs;

            string sTemplateName = theArgs.sTemplateName;
            Cursor.Current = Cursors.WaitCursor;
            string sSelectedTransform = m_RepoManager.CurrentRepo.TicketFolder + @"\XSLT\OrderItem.xsl";
            string sProcessedName = sTemplateName.Replace(".oet", "").TrimEnd();

            if (sProcessedName.EndsWith("Panel", StringComparison.OrdinalIgnoreCase) ||
                 sProcessedName.EndsWith("Protocol", StringComparison.OrdinalIgnoreCase) ||
                 sProcessedName.EndsWith("Set", StringComparison.OrdinalIgnoreCase) ||
                 sProcessedName.EndsWith("Template", StringComparison.OrdinalIgnoreCase) ||
                 sProcessedName.EndsWith("Group", StringComparison.OrdinalIgnoreCase))
            {
                sSelectedTransform = m_RepoManager.CurrentRepo.TicketFolder + @"\XSLT\OrderSet.xsl";
            }

            try
            {
                string sTempHTML = @"c:\temp\" + Guid.NewGuid().ToString() + @".html";

                HttpWebRequest wr = TransformRequestBuilder.CreateSOAPWebRequest(m_OPTWebserviceUrl);
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
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Still executing...."); return;
                    }
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
                    Logger.Error(ex, "Still Executing ...");
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

            //tspStatusLabel.Text = "Viewing " + m_currentDocumentRepo;


            if (!string.IsNullOrEmpty(m_currentDocumentRepo))
            {
                tspStatusLabel.Text = "Viewing " + m_currentDocumentRepo;
            }
            else
            {
                tspStatusLabel.Text = "";

            }


            toolStripProgressBar2.Visible = false;

            if (!String.IsNullOrEmpty(tstbRepositorySearch.Text))
            {
                if (!gSearchDocumentRep)
                {
                    string highlightedHtml = Utility.HighlightHtml(tstbRepositorySearch.Text, wbRepositoryView.Document);
                    wbRepositoryView.DocumentText = highlightedHtml;
                    gSearchDocumentRep = true; // avoids re-triggering the highlight in a loop
                }
                else
                {
                    gSearchDocumentRep = false;
                }
            }
        }

        private void OpenInWord()
        {
            var word = new Microsoft.Office.Interop.Word.Application();
            word.Visible = true;
            string TemplateFilename = dictIdName[lvRepository.SelectedItems[0].Text];
            string newFilename = m_RepoManager.CurrentRepo.WIPPath + Path.GetFileNameWithoutExtension(TemplateFilename) + ".html";
            string oldFilename = m_currentHTML.Replace("file:///", "");

            if (File.Exists(newFilename))
            {
                File.Delete(newFilename);
            }

            File.Copy(oldFilename, newFilename);
            word.Documents.Open(newFilename);
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
                string filepath = m_RepoManager.CurrentRepo.GetTemplateFilepath(filename);
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
            string sTID = m_RepoManager.CurrentRepo.GetTemplateID(filename);
            wbWIPWUR.ScriptErrorsSuppressed = true;
            wbWIPWUR.Url = new Uri("http://ckcm:8011/WhereUsed," + sTID);
        }

        private void LoadRepoWUR(string filename)
        {
            //http://ckcm:8011/WhereUsed,0f3e3fc2-6dbe-4f6f-b292-e8ef0501c163
            string sTID = m_RepoManager.CurrentRepo.GetTemplateID(filename);
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

        private void tsbWorkUpload_Click(object sender, EventArgs e)
        {
            m_RepoManager.CurrentRepo.PostWIP();
            //PostCache();
            StartUpload();
        }

        private bool StartUpload()
        {
            tabControl1.SelectedTab = tabControl1.TabPages[2];

            string url = m_RepoManager.PrepareForUpload();

            m_browserUpload.Load(url);

            return true;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timerRepoFilter.Enabled = false;
            string filter = tstbRepositoryFilter.Text;

            //m_RepoManager.ApplyFilter(filter);

            if (string.IsNullOrEmpty(filter))
            {
                lblFilter.Text = "";
            }
            else lblFilter.Text = $"Filter: Name contains {filter}";
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

        private void SetAssetModified(string filename, string state)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { this.SetAssetModified(filename, state); });
                return;
            }
            if (filename == null) return;
            if (filename.Trim() == "") return;

            foreach (ListViewItem item in lvWork.Items)
            {
                if (item.Text == filename)
                {
                    item.SubItems[2].Text = state;
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

            string assetpath = m_RepoManager.CurrentRepo.AssetPath;
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
                Logger.Error(ex.StackTrace);
                Logger.Info(ex, "Still Exectuing....");
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
                string filepath = m_RepoManager.CurrentRepo.GetTemplateFilepath(filename);//dictFileToPath[filename];
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
                m_RepoManager.CurrentRepo.AddWIP(itemdata);
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
                m_RepoManager.CurrentRepo.RemoveWIP(items[0]);//, items[1]);
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

                        var bEnabled = !m_RepoManager.CurrentRepo.isAssetinWIP(item.Text);
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
            lvWork.Items.Clear();
            m_RepoManager.CurrentRepo.LoadExistingWIP();
        }

        private void LoadRepositoryTemplates()
        {
            if (m_RepoManager.CurrentRepo == null) return;

            m_RepoManager.CurrentRepo.LoadRepositoryTemplates();
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

            if( !string.IsNullOrEmpty(m_currentDocumentWIP))
            {
                tsStatusLabel.Text = "Viewing " + m_currentDocumentWIP;
            }
            else 
            {
                tsStatusLabel.Text = "";

            }

            tsPBWIPTransform.Visible = false;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            
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
            if (tcRepoResults.SelectedTab == tcRepoResults.TabPages[0])
            {
                if (lvRepository.SelectedItems.Count > 0)
                {
                    string filename = lvRepository.SelectedItems[0].Text;
                    string filepath = m_RepoManager.CurrentRepo.GetTemplateFilepath(filename);
                    RunThreadedTransformRepo(filename);
                }
            }
            else
            {
                if (lvRepoSearchResults.SelectedItems.Count > 0)
                {
                    string filename = lvRepoSearchResults.SelectedItems[0].Text;
                    string filepath = m_RepoManager.CurrentRepo.GetTemplateFilepath(filename);
                    RunThreadedTransformRepo(filename);
                }
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            mCurrentPage++;
            if (mCurrentPage > 0) btnPrev.Visible = true;
            lblPageCount.Text = $"{mCurrentPage + 1}";
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
            m_RepoManager.CurrentRepo.SetTicketReadiness(true);
        }

        private void tsbPause_Click(object sender, EventArgs e)
        {
            tslReadyState.Text = "Work: Paused";

            tsbStart.Enabled = true;
            tsbPause.Enabled = false;
            m_RepoManager.CurrentRepo.SetTicketReadiness(false);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_IsClosing = true;
            m_RepoManager.Shutdown();
            //m_RepoManager.SaveExistingWip();
        }

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //m_RepoManager.TestJira( "CKCMFK-1989");
        }

        private void tsbLaunchTD_Click(object sender, EventArgs e)
        {
            // setup config

            // start process
            LaunchTD("");
        }

        private void LaunchTD( string assetfilepath)
        {
            if (m_RepoManager.CurrentRepo == null) return;
            m_RepoManager.CurrentRepo.ConfigureAndLaunchTD( assetfilepath );
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            LaunchTD("");
        }

        private void setupNewTicketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetupTicketForm ticketform = new SetupTicketForm();
            if (ticketform.ShowDialog() == DialogResult.OK)
            {
                BusyForm bf = new BusyForm();
                
                
                try
                {
                    bf.StartPosition = FormStartPosition.CenterScreen;

                    bf.Show();

                    if (m_RepoManager.PrepareNewTicket(ticketform.m_TicketJSON))
                    {
                        InitAvailableRepos();
                        InitView();
                        LoadRepositoryTemplates();
                    } else
                    {
                        MessageBox.Show("Unable to create ticket at this time");
                    }

                }
                finally
                {
                    bf.Close();
                }
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
            if (lvWork.SelectedItems.Count < 1) return;

            string filename= lvWork.SelectedItems[0].Text;

            
            LaunchTD(m_RepoManager.CurrentRepo.GetTemplateFilepath(filename));
        }

        private void tsmiAvailableRepo_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            string newRepo = item.Text;

            BusyForm busy = new BusyForm();
            busy.StartPosition = FormStartPosition.CenterScreen;
            //busy.Parent = this;

            try
            {
                this.UseWaitCursor = true;
                busy.Show();
                InitView();

                if (m_RepoManager.SetCurrentRepository(newRepo))
                {
                    SetRepositoryTitle(newRepo);
                    LoadRepositoryTemplates();
                }
            }
            finally
            {
                busy.Hide();
                this.UseWaitCursor = false;
            }
        }

        private void InitView()
        {
            if( InvokeRequired )
            {
                BeginInvoke((MethodInvoker)delegate { this.InitView(); });
                return;
            }


            lvWork.Items.Clear();
            UpdateWorkViewTitle();
            tstbRepositoryFilter.Text = "";
            wbRepositoryView.Navigate(new Uri("about:blank"));
            wbRepoWUR.Navigate(new Uri("about:blank"));
            wbWIP.Navigate(new Uri("about:blank"));
            wbWIPWUR.Navigate(new Uri("about:blank"));
            wbOverlaps.Navigate(new Uri("about:blank"));
            m_browserUpload.Load("about:blank");

            tsStatusLabel.Text = "";
            m_currentDocumentWIP = "";
            m_currentDocumentRepo = "";
            tspStatusLabel.Text = "";


//            throw new NotImplementedException();
        }

        private void tbWIPViews_Selected(object sender, TabControlEventArgs e)
        {

            if (tbWIPViews.SelectedTab.Name == "tpOverlaps2")
            {
                wbOverlaps.Url = new Uri($"{DAM_OVERLAP_URL}{m_RepoManager.CurrentRepo.TicketID}");
            }
        }

        private void closeTicketToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RepoInstance current = m_RepoManager.CurrentRepo;
            if( current != null)
            {
                MessageBox.Show("About to remove the current ticket..");
                m_RepoManager.RemoveTicket(current.TicketID);
                LoadRepositoryTemplates();
            }

        }
    }
}