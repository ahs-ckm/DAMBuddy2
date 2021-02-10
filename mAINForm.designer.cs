namespace DAMBuddy2
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpRepo = new System.Windows.Forms.TabPage();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsddbRepository = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsmiAvailableRepo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmSetupTicket = new System.Windows.Forms.ToolStripMenuItem();
            this.tsbRepositoryReload = new System.Windows.Forms.ToolStripButton();
            this.tsbLaunch2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tstbRepositoryFilter = new System.Windows.Forms.ToolStripTextBox();
            this.tsbRepositoryFilterClear = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
            this.tstbRepositorySearch = new System.Windows.Forms.ToolStripTextBox();
            this.tsbRepoSearch = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tspTime = new System.Windows.Forms.ToolStripLabel();
            this.tsbWord = new System.Windows.Forms.ToolStripButton();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsmiUserAccount = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.setupNewTicketToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tcRepoResults = new System.Windows.Forms.TabControl();
            this.tpRepoAll = new System.Windows.Forms.TabPage();
            this.toolStrip5 = new System.Windows.Forms.ToolStrip();
            this.tsbAddWIP = new System.Windows.Forms.ToolStripButton();
            this.tsbNext = new System.Windows.Forms.ToolStripButton();
            this.tslPageCount = new System.Windows.Forms.ToolStripLabel();
            this.tsbPrev = new System.Windows.Forms.ToolStripButton();
            this.tsbRepositoryViewDocument = new System.Windows.Forms.ToolStripButton();
            this.lblFilter = new System.Windows.Forms.Label();
            this.lvRepository = new System.Windows.Forms.ListView();
            this.Asset = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tpRepoSearch = new System.Windows.Forms.TabPage();
            this.toolStrip3 = new System.Windows.Forms.ToolStrip();
            this.tsbAddFromSearch = new System.Windows.Forms.ToolStripButton();
            this.lvRepoSearchResults = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button2 = new System.Windows.Forms.Button();
            this.tcRepository = new System.Windows.Forms.TabControl();
            this.tpRepoPreview = new System.Windows.Forms.TabPage();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tspStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar2 = new System.Windows.Forms.ToolStripProgressBar();
            this.wbRepositoryView = new System.Windows.Forms.WebBrowser();
            this.tpWUR = new System.Windows.Forms.TabPage();
            this.wbRepoWUR = new System.Windows.Forms.WebBrowser();
            this.cbTransforms = new System.Windows.Forms.ComboBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tpWIP = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.toolStrip4 = new System.Windows.Forms.ToolStrip();
            this.tsbRemoveWIP = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbRefreshStale = new System.Windows.Forms.ToolStripButton();
            this.tsbRootNodeEdit = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbWordWIP = new System.Windows.Forms.ToolStripButton();
            this.tsWorkViewDocument = new System.Windows.Forms.ToolStripButton();
            this.lvWork = new System.Windows.Forms.ListView();
            this.chFilename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chStale = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chModified = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chRootNode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tbWIPViews = new System.Windows.Forms.TabControl();
            this.tpPreviewWIP = new System.Windows.Forms.TabPage();
            this.statusStrip2 = new System.Windows.Forms.StatusStrip();
            this.tsStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsPBWIPTransform = new System.Windows.Forms.ToolStripProgressBar();
            this.wbWIP = new System.Windows.Forms.WebBrowser();
            this.tpOverlapsWIP = new System.Windows.Forms.TabPage();
            this.wbWIPWUR = new System.Windows.Forms.WebBrowser();
            this.tpOverlaps2 = new System.Windows.Forms.TabPage();
            this.wbOverlaps = new System.Windows.Forms.WebBrowser();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.tsddbRepositoryWIP = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsWorkReload = new System.Windows.Forms.ToolStripButton();
            this.tsbLaunchTD = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbDocReview = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbPause = new System.Windows.Forms.ToolStripButton();
            this.tsbStart = new System.Windows.Forms.ToolStripButton();
            this.tsbHelp = new System.Windows.Forms.ToolStripButton();
            this.tslReadyState = new System.Windows.Forms.ToolStripLabel();
            this.tslScheduleState = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbWorkUpload = new System.Windows.Forms.ToolStripButton();
            this.tpUpload = new System.Windows.Forms.TabPage();
            this.tpSchedule = new System.Windows.Forms.TabPage();
            this.tpDocReview = new System.Windows.Forms.TabPage();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.timerRepoFilter = new System.Windows.Forms.Timer(this.components);
            this.cmsRemove = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmWIPRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.statusForm = new System.Windows.Forms.StatusStrip();
            this.tslMainStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.logToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslFolder = new System.Windows.Forms.ToolStripStatusLabel();
            this.tslScheduleStatus = new System.Windows.Forms.ToolStripLabel();
            this.tabControl1.SuspendLayout();
            this.tpRepo.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tcRepoResults.SuspendLayout();
            this.tpRepoAll.SuspendLayout();
            this.toolStrip5.SuspendLayout();
            this.tpRepoSearch.SuspendLayout();
            this.toolStrip3.SuspendLayout();
            this.tcRepository.SuspendLayout();
            this.tpRepoPreview.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tpWUR.SuspendLayout();
            this.tpWIP.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.toolStrip4.SuspendLayout();
            this.tbWIPViews.SuspendLayout();
            this.tpPreviewWIP.SuspendLayout();
            this.statusStrip2.SuspendLayout();
            this.tpOverlapsWIP.SuspendLayout();
            this.tpOverlaps2.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.cmsRemove.SuspendLayout();
            this.statusForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(-221, -19);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(10, 20);
            this.textBox3.TabIndex = 4;
            this.textBox3.Text = "C:\\TD\\COVID2";
            this.textBox3.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // timer1
            // 
            this.timer1.Interval = 60000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpRepo);
            this.tabControl1.Controls.Add(this.tpWIP);
            this.tabControl1.Controls.Add(this.tpUpload);
            this.tabControl1.Controls.Add(this.tpSchedule);
            this.tabControl1.Controls.Add(this.tpDocReview);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Arial Unicode MS", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(26, 6);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1265, 601);
            this.tabControl1.TabIndex = 21;
            this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
            // 
            // tpRepo
            // 
            this.tpRepo.Controls.Add(this.toolStrip1);
            this.tpRepo.Controls.Add(this.splitContainer1);
            this.tpRepo.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.tpRepo.Location = new System.Drawing.Point(4, 35);
            this.tpRepo.Name = "tpRepo";
            this.tpRepo.Padding = new System.Windows.Forms.Padding(3);
            this.tpRepo.Size = new System.Drawing.Size(1257, 562);
            this.tpRepo.TabIndex = 0;
            this.tpRepo.Text = "Repository View";
            this.tpRepo.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.BackColor = System.Drawing.Color.PowderBlue;
            this.toolStrip1.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F);
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(48, 48);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsddbRepository,
            this.tsbRepositoryReload,
            this.tsbLaunch2,
            this.toolStripSeparator6,
            this.toolStripLabel1,
            this.tstbRepositoryFilter,
            this.tsbRepositoryFilterClear,
            this.toolStripSeparator1,
            this.toolStripLabel5,
            this.tstbRepositorySearch,
            this.tsbRepoSearch,
            this.toolStripSeparator2,
            this.tspTime,
            this.tsbWord,
            this.toolStripProgressBar1,
            this.toolStripDropDownButton2});
            this.toolStrip1.Location = new System.Drawing.Point(3, 3);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1251, 45);
            this.toolStrip1.TabIndex = 18;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsddbRepository
            // 
            this.tsddbRepository.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAvailableRepo,
            this.toolStripSeparator10,
            this.tsmSetupTicket});
            this.tsddbRepository.Font = new System.Drawing.Font("Arial Unicode MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tsddbRepository.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tsddbRepository.Image = global::DAMBuddy2.Properties.Resources.outline_confirmation_number_black_24dp;
            this.tsddbRepository.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsddbRepository.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsddbRepository.Name = "tsddbRepository";
            this.tsddbRepository.Size = new System.Drawing.Size(164, 42);
            this.tsddbRepository.Text = "< no repository >";
            this.tsddbRepository.ToolTipText = "The current ticket being worked on, click to change.";
            // 
            // tsmiAvailableRepo
            // 
            this.tsmiAvailableRepo.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F);
            this.tsmiAvailableRepo.Name = "tsmiAvailableRepo";
            this.tsmiAvailableRepo.Size = new System.Drawing.Size(162, 30);
            this.tsmiAvailableRepo.Text = "CSDFK-1989";
            this.tsmiAvailableRepo.Click += new System.EventHandler(this.tsmiAvailableRepo_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(159, 6);
            // 
            // tsmSetupTicket
            // 
            this.tsmSetupTicket.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F);
            this.tsmSetupTicket.Image = global::DAMBuddy2.Properties.Resources.outline_confirmation_number_black_24dp;
            this.tsmSetupTicket.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmSetupTicket.Name = "tsmSetupTicket";
            this.tsmSetupTicket.Size = new System.Drawing.Size(162, 30);
            // 
            // tsbRepositoryReload
            // 
            this.tsbRepositoryReload.AutoSize = false;
            this.tsbRepositoryReload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbRepositoryReload.Image = global::DAMBuddy2.Properties.Resources.outline_cached_black_24dp;
            this.tsbRepositoryReload.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbRepositoryReload.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRepositoryReload.Name = "tsbRepositoryReload";
            this.tsbRepositoryReload.Size = new System.Drawing.Size(48, 42);
            this.tsbRepositoryReload.Text = "Reload Available Assets";
            this.tsbRepositoryReload.Click += new System.EventHandler(this.tsbRepositoryReload_Click);
            // 
            // tsbLaunch2
            // 
            this.tsbLaunch2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tsbLaunch2.Image = global::DAMBuddy2.Properties.Resources.outline_account_tree_black_18dp;
            this.tsbLaunch2.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbLaunch2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLaunch2.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
            this.tsbLaunch2.Name = "tsbLaunch2";
            this.tsbLaunch2.Size = new System.Drawing.Size(93, 42);
            this.tsbLaunch2.Text = "View in TD";
            this.tsbLaunch2.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 45);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripLabel1.ForeColor = System.Drawing.Color.Black;
            this.toolStripLabel1.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(131, 42);
            this.toolStripLabel1.Text = "Filter by Asset Name";
            // 
            // tstbRepositoryFilter
            // 
            this.tstbRepositoryFilter.AutoSize = false;
            this.tstbRepositoryFilter.AutoToolTip = true;
            this.tstbRepositoryFilter.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tstbRepositoryFilter.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F);
            this.tstbRepositoryFilter.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tstbRepositoryFilter.Name = "tstbRepositoryFilter";
            this.tstbRepositoryFilter.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.tstbRepositoryFilter.Size = new System.Drawing.Size(175, 45);
            this.tstbRepositoryFilter.ToolTipText = "Narrow down the assets : only assets with this text in their name will be display" +
    "ed.";
            this.tstbRepositoryFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tstbRepositoryFilter_KeyDown);
            this.tstbRepositoryFilter.TextChanged += new System.EventHandler(this.tstbFilter_TextChanged);
            // 
            // tsbRepositoryFilterClear
            // 
            this.tsbRepositoryFilterClear.AutoSize = false;
            this.tsbRepositoryFilterClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbRepositoryFilterClear.Image = global::DAMBuddy2.Properties.Resources.outline_backspace_black_24dp;
            this.tsbRepositoryFilterClear.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbRepositoryFilterClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRepositoryFilterClear.Name = "tsbRepositoryFilterClear";
            this.tsbRepositoryFilterClear.Size = new System.Drawing.Size(48, 42);
            this.tsbRepositoryFilterClear.Text = "Clear Filter";
            this.tsbRepositoryFilterClear.Click += new System.EventHandler(this.tsbRepositoryFilterClear_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.BackColor = System.Drawing.Color.Olive;
            this.toolStripSeparator1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 45);
            this.toolStripSeparator1.Visible = false;
            // 
            // toolStripLabel5
            // 
            this.toolStripLabel5.ForeColor = System.Drawing.Color.Black;
            this.toolStripLabel5.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
            this.toolStripLabel5.Name = "toolStripLabel5";
            this.toolStripLabel5.Size = new System.Drawing.Size(143, 42);
            this.toolStripLabel5.Text = "Search Asset Contents";
            // 
            // tstbRepositorySearch
            // 
            this.tstbRepositorySearch.AutoSize = false;
            this.tstbRepositorySearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tstbRepositorySearch.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F);
            this.tstbRepositorySearch.Name = "tstbRepositorySearch";
            this.tstbRepositorySearch.Size = new System.Drawing.Size(175, 45);
            this.tstbRepositorySearch.ToolTipText = "Search within asset contents, this will take a few seconds to complete.";
            // 
            // tsbRepoSearch
            // 
            this.tsbRepoSearch.AutoSize = false;
            this.tsbRepoSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbRepoSearch.Image = global::DAMBuddy2.Properties.Resources.outline_search_black_24dp;
            this.tsbRepoSearch.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbRepoSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRepoSearch.Name = "tsbRepoSearch";
            this.tsbRepoSearch.Size = new System.Drawing.Size(48, 42);
            this.tsbRepoSearch.Text = "toolStripButton1";
            this.tsbRepoSearch.ToolTipText = "Start Search of Asset Contents";
            this.tsbRepoSearch.Click += new System.EventHandler(this.tsbRepoSearch_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 45);
            // 
            // tspTime
            // 
            this.tspTime.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tspTime.Name = "tspTime";
            this.tspTime.Size = new System.Drawing.Size(0, 42);
            // 
            // tsbWord
            // 
            this.tsbWord.AutoSize = false;
            this.tsbWord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbWord.Image = global::DAMBuddy2.Properties.Resources.outline_text_snippet_black_24dp;
            this.tsbWord.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbWord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbWord.Name = "tsbWord";
            this.tsbWord.Size = new System.Drawing.Size(52, 42);
            this.tsbWord.Text = "Open In Word";
            this.tsbWord.Visible = false;
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.AutoSize = false;
            this.toolStripProgressBar1.Maximum = 4;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 36);
            this.toolStripProgressBar1.ToolTipText = "Transform Progress";
            this.toolStripProgressBar1.Visible = false;
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripDropDownButton2.AutoSize = false;
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiUserAccount,
            this.toolStripMenuItem5,
            this.setupNewTicketToolStripMenuItem});
            this.toolStripDropDownButton2.Image = global::DAMBuddy2.Properties.Resources.outline_settings_black_24dp;
            this.toolStripDropDownButton2.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(52, 42);
            this.toolStripDropDownButton2.Text = "toolStripDropDownButton2";
            // 
            // tsmiUserAccount
            // 
            this.tsmiUserAccount.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F);
            this.tsmiUserAccount.Image = global::DAMBuddy2.Properties.Resources.outline_face_black_24dp;
            this.tsmiUserAccount.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsmiUserAccount.Name = "tsmiUserAccount";
            this.tsmiUserAccount.Size = new System.Drawing.Size(215, 30);
            this.tsmiUserAccount.Text = "User Account...";
            this.tsmiUserAccount.Click += new System.EventHandler(this.tsmiUserAccount_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(212, 6);
            // 
            // setupNewTicketToolStripMenuItem
            // 
            this.setupNewTicketToolStripMenuItem.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F);
            this.setupNewTicketToolStripMenuItem.Image = global::DAMBuddy2.Properties.Resources.outline_confirmation_number_black_24dp;
            this.setupNewTicketToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.setupNewTicketToolStripMenuItem.Name = "setupNewTicketToolStripMenuItem";
            this.setupNewTicketToolStripMenuItem.Size = new System.Drawing.Size(215, 30);
            this.setupNewTicketToolStripMenuItem.Text = "Close Current Ticket...";
            this.setupNewTicketToolStripMenuItem.Click += new System.EventHandler(this.closeTicketToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(3, 51);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel1.Controls.Add(this.tcRepoResults);
            this.splitContainer1.Panel1.Controls.Add(this.button2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tcRepository);
            this.splitContainer1.Panel2.Controls.Add(this.cbTransforms);
            this.splitContainer1.Panel2.Controls.Add(this.textBox2);
            this.splitContainer1.Panel2.Controls.Add(this.textBox1);
            this.splitContainer1.Size = new System.Drawing.Size(1251, 495);
            this.splitContainer1.SplitterDistance = 428;
            this.splitContainer1.TabIndex = 15;
            // 
            // tcRepoResults
            // 
            this.tcRepoResults.Controls.Add(this.tpRepoAll);
            this.tcRepoResults.Controls.Add(this.tpRepoSearch);
            this.tcRepoResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcRepoResults.Font = new System.Drawing.Font("Arial Unicode MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.tcRepoResults.Location = new System.Drawing.Point(0, 0);
            this.tcRepoResults.Multiline = true;
            this.tcRepoResults.Name = "tcRepoResults";
            this.tcRepoResults.Padding = new System.Drawing.Point(26, 3);
            this.tcRepoResults.SelectedIndex = 0;
            this.tcRepoResults.Size = new System.Drawing.Size(428, 495);
            this.tcRepoResults.TabIndex = 15;
            // 
            // tpRepoAll
            // 
            this.tpRepoAll.Controls.Add(this.toolStrip5);
            this.tpRepoAll.Controls.Add(this.lblFilter);
            this.tpRepoAll.Controls.Add(this.lvRepository);
            this.tpRepoAll.Location = new System.Drawing.Point(4, 25);
            this.tpRepoAll.Name = "tpRepoAll";
            this.tpRepoAll.Padding = new System.Windows.Forms.Padding(3);
            this.tpRepoAll.Size = new System.Drawing.Size(420, 466);
            this.tpRepoAll.TabIndex = 0;
            this.tpRepoAll.Text = "All";
            // 
            // toolStrip5
            // 
            this.toolStrip5.BackColor = System.Drawing.Color.White;
            this.toolStrip5.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip5.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbAddWIP,
            this.tsbNext,
            this.tslPageCount,
            this.tsbPrev,
            this.tsbRepositoryViewDocument});
            this.toolStrip5.Location = new System.Drawing.Point(3, 3);
            this.toolStrip5.Name = "toolStrip5";
            this.toolStrip5.Size = new System.Drawing.Size(414, 31);
            this.toolStrip5.TabIndex = 21;
            // 
            // tsbAddWIP
            // 
            this.tsbAddWIP.BackColor = System.Drawing.Color.White;
            this.tsbAddWIP.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tsbAddWIP.Image = global::DAMBuddy2.Properties.Resources.outline_add_circle_outline_black_24dp;
            this.tsbAddWIP.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbAddWIP.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAddWIP.Name = "tsbAddWIP";
            this.tsbAddWIP.Size = new System.Drawing.Size(102, 28);
            this.tsbAddWIP.Text = "Add to Work";
            this.tsbAddWIP.ToolTipText = "Add this asset to the work view, so that it can be edited.";
            this.tsbAddWIP.Click += new System.EventHandler(this.tsbAddWIP_Click);
            // 
            // tsbNext
            // 
            this.tsbNext.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbNext.AutoSize = false;
            this.tsbNext.BackColor = System.Drawing.Color.White;
            this.tsbNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbNext.Image = global::DAMBuddy2.Properties.Resources.outline_navigate_next_black_24dp;
            this.tsbNext.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbNext.Name = "tsbNext";
            this.tsbNext.Size = new System.Drawing.Size(28, 28);
            this.tsbNext.Text = "toolStripButton1";
            this.tsbNext.ToolTipText = "Next Page";
            this.tsbNext.Click += new System.EventHandler(this.tsbNext_Click);
            // 
            // tslPageCount
            // 
            this.tslPageCount.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tslPageCount.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tslPageCount.Name = "tslPageCount";
            this.tslPageCount.Size = new System.Drawing.Size(13, 28);
            this.tslPageCount.Text = "1";
            this.tslPageCount.ToolTipText = "Current and Total number of pages";
            // 
            // tsbPrev
            // 
            this.tsbPrev.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbPrev.AutoSize = false;
            this.tsbPrev.BackColor = System.Drawing.Color.White;
            this.tsbPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbPrev.Image = global::DAMBuddy2.Properties.Resources.outline_navigate_before_black_24dp;
            this.tsbPrev.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbPrev.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPrev.Name = "tsbPrev";
            this.tsbPrev.Size = new System.Drawing.Size(28, 28);
            this.tsbPrev.Text = "toolStripButton1";
            this.tsbPrev.ToolTipText = "Previous Page";
            this.tsbPrev.Click += new System.EventHandler(this.tsbPrev_Click);
            // 
            // tsbRepositoryViewDocument
            // 
            this.tsbRepositoryViewDocument.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tsbRepositoryViewDocument.Image = global::DAMBuddy2.Properties.Resources.outline_preview_black_24dp;
            this.tsbRepositoryViewDocument.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbRepositoryViewDocument.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRepositoryViewDocument.Name = "tsbRepositoryViewDocument";
            this.tsbRepositoryViewDocument.Size = new System.Drawing.Size(131, 28);
            this.tsbRepositoryViewDocument.Text = "Refresh Transform";
            this.tsbRepositoryViewDocument.Visible = false;
            this.tsbRepositoryViewDocument.Click += new System.EventHandler(this.tsbRepositoryViewDocument_Click);
            // 
            // lblFilter
            // 
            this.lblFilter.AutoSize = true;
            this.lblFilter.Font = new System.Drawing.Font("Arial Unicode MS", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblFilter.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblFilter.Location = new System.Drawing.Point(7, 9);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(0, 16);
            this.lblFilter.TabIndex = 18;
            // 
            // lvRepository
            // 
            this.lvRepository.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lvRepository.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvRepository.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvRepository.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Asset});
            this.lvRepository.Font = new System.Drawing.Font("Arial Unicode MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.lvRepository.FullRowSelect = true;
            this.lvRepository.GridLines = true;
            this.lvRepository.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvRepository.HideSelection = false;
            this.lvRepository.Location = new System.Drawing.Point(3, 37);
            this.lvRepository.MultiSelect = false;
            this.lvRepository.Name = "lvRepository";
            this.lvRepository.Size = new System.Drawing.Size(414, 426);
            this.lvRepository.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvRepository.TabIndex = 15;
            this.lvRepository.UseCompatibleStateImageBehavior = false;
            this.lvRepository.View = System.Windows.Forms.View.Details;
            this.lvRepository.SelectedIndexChanged += new System.EventHandler(this.lvRepository_SelectedIndexChanged_1);
            this.lvRepository.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvRepository_MouseDoubleClick);
            this.lvRepository.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lvRepository_MouseUp);
            // 
            // Asset
            // 
            this.Asset.Text = "Asset";
            this.Asset.Width = 420;
            // 
            // tpRepoSearch
            // 
            this.tpRepoSearch.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tpRepoSearch.Controls.Add(this.toolStrip3);
            this.tpRepoSearch.Controls.Add(this.lvRepoSearchResults);
            this.tpRepoSearch.Location = new System.Drawing.Point(4, 25);
            this.tpRepoSearch.Name = "tpRepoSearch";
            this.tpRepoSearch.Padding = new System.Windows.Forms.Padding(3);
            this.tpRepoSearch.Size = new System.Drawing.Size(420, 466);
            this.tpRepoSearch.TabIndex = 1;
            this.tpRepoSearch.Text = "Search Results";
            // 
            // toolStrip3
            // 
            this.toolStrip3.BackColor = System.Drawing.Color.White;
            this.toolStrip3.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbAddFromSearch});
            this.toolStrip3.Location = new System.Drawing.Point(3, 3);
            this.toolStrip3.Name = "toolStrip3";
            this.toolStrip3.Size = new System.Drawing.Size(414, 31);
            this.toolStrip3.TabIndex = 17;
            // 
            // tsbAddFromSearch
            // 
            this.tsbAddFromSearch.BackColor = System.Drawing.Color.White;
            this.tsbAddFromSearch.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tsbAddFromSearch.Image = global::DAMBuddy2.Properties.Resources.outline_add_circle_outline_black_24dp;
            this.tsbAddFromSearch.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbAddFromSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAddFromSearch.Name = "tsbAddFromSearch";
            this.tsbAddFromSearch.Size = new System.Drawing.Size(102, 28);
            this.tsbAddFromSearch.Text = "Add to Work";
            this.tsbAddFromSearch.ToolTipText = "Add this asset to the work view, so that it can be edited.";
            this.tsbAddFromSearch.Click += new System.EventHandler(this.tsbAddFromSearch_Click);
            // 
            // lvRepoSearchResults
            // 
            this.lvRepoSearchResults.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lvRepoSearchResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvRepoSearchResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvRepoSearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.lvRepoSearchResults.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvRepoSearchResults.FullRowSelect = true;
            this.lvRepoSearchResults.GridLines = true;
            this.lvRepoSearchResults.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvRepoSearchResults.HideSelection = false;
            this.lvRepoSearchResults.Location = new System.Drawing.Point(3, 35);
            this.lvRepoSearchResults.MultiSelect = false;
            this.lvRepoSearchResults.Name = "lvRepoSearchResults";
            this.lvRepoSearchResults.Size = new System.Drawing.Size(411, 431);
            this.lvRepoSearchResults.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvRepoSearchResults.TabIndex = 16;
            this.lvRepoSearchResults.UseCompatibleStateImageBehavior = false;
            this.lvRepoSearchResults.View = System.Windows.Forms.View.Details;
            this.lvRepoSearchResults.SelectedIndexChanged += new System.EventHandler(this.lvRepoSearchResults_SelectedIndexChanged);
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Asset";
            this.columnHeader2.Width = 420;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(79, -29);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(137, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "Refresh Templates";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // tcRepository
            // 
            this.tcRepository.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.tcRepository.Controls.Add(this.tpRepoPreview);
            this.tcRepository.Controls.Add(this.tpWUR);
            this.tcRepository.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcRepository.Font = new System.Drawing.Font("Arial Unicode MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.tcRepository.Location = new System.Drawing.Point(0, 0);
            this.tcRepository.Multiline = true;
            this.tcRepository.Name = "tcRepository";
            this.tcRepository.Padding = new System.Drawing.Point(26, 6);
            this.tcRepository.SelectedIndex = 0;
            this.tcRepository.Size = new System.Drawing.Size(819, 495);
            this.tcRepository.TabIndex = 22;
            // 
            // tpRepoPreview
            // 
            this.tpRepoPreview.Controls.Add(this.statusStrip1);
            this.tpRepoPreview.Controls.Add(this.wbRepositoryView);
            this.tpRepoPreview.Location = new System.Drawing.Point(32, 4);
            this.tpRepoPreview.Name = "tpRepoPreview";
            this.tpRepoPreview.Padding = new System.Windows.Forms.Padding(3);
            this.tpRepoPreview.Size = new System.Drawing.Size(783, 487);
            this.tpRepoPreview.TabIndex = 0;
            this.tpRepoPreview.Text = "Document Preview";
            this.tpRepoPreview.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.Color.White;
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.Top;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tspStatusLabel,
            this.toolStripProgressBar2});
            this.statusStrip1.Location = new System.Drawing.Point(3, 3);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(777, 26);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 22;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tspStatusLabel
            // 
            this.tspStatusLabel.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F);
            this.tspStatusLabel.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tspStatusLabel.Image = global::DAMBuddy2.Properties.Resources.outline_info_black_24dp;
            this.tspStatusLabel.Name = "tspStatusLabel";
            this.tspStatusLabel.Size = new System.Drawing.Size(16, 21);
            // 
            // toolStripProgressBar2
            // 
            this.toolStripProgressBar2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgressBar2.Name = "toolStripProgressBar2";
            this.toolStripProgressBar2.Size = new System.Drawing.Size(200, 20);
            // 
            // wbRepositoryView
            // 
            this.wbRepositoryView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.wbRepositoryView.Location = new System.Drawing.Point(3, 32);
            this.wbRepositoryView.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbRepositoryView.Name = "wbRepositoryView";
            this.wbRepositoryView.Size = new System.Drawing.Size(774, 449);
            this.wbRepositoryView.TabIndex = 5;
            this.wbRepositoryView.Url = new System.Uri("", System.UriKind.Relative);
            this.wbRepositoryView.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.WebBrowser1_DocumentCompleted);
            // 
            // tpWUR
            // 
            this.tpWUR.Controls.Add(this.wbRepoWUR);
            this.tpWUR.Location = new System.Drawing.Point(32, 4);
            this.tpWUR.Name = "tpWUR";
            this.tpWUR.Padding = new System.Windows.Forms.Padding(3);
            this.tpWUR.Size = new System.Drawing.Size(783, 487);
            this.tpWUR.TabIndex = 1;
            this.tpWUR.Text = "Where Used Report";
            this.tpWUR.UseVisualStyleBackColor = true;
            // 
            // wbRepoWUR
            // 
            this.wbRepoWUR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wbRepoWUR.Location = new System.Drawing.Point(3, 3);
            this.wbRepoWUR.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbRepoWUR.Name = "wbRepoWUR";
            this.wbRepoWUR.Size = new System.Drawing.Size(777, 481);
            this.wbRepoWUR.TabIndex = 0;
            // 
            // cbTransforms
            // 
            this.cbTransforms.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTransforms.FormattingEnabled = true;
            this.cbTransforms.Location = new System.Drawing.Point(517, 292);
            this.cbTransforms.Name = "cbTransforms";
            this.cbTransforms.Size = new System.Drawing.Size(339, 28);
            this.cbTransforms.Sorted = true;
            this.cbTransforms.TabIndex = 9;
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox2.Location = new System.Drawing.Point(571, 322);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(244, 52);
            this.textBox2.TabIndex = 16;
            this.textBox2.Visible = false;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.Location = new System.Drawing.Point(612, 339);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(244, 52);
            this.textBox1.TabIndex = 15;
            this.textBox1.Visible = false;
            // 
            // tpWIP
            // 
            this.tpWIP.Controls.Add(this.splitContainer2);
            this.tpWIP.Controls.Add(this.toolStrip2);
            this.tpWIP.Location = new System.Drawing.Point(4, 35);
            this.tpWIP.Name = "tpWIP";
            this.tpWIP.Padding = new System.Windows.Forms.Padding(3);
            this.tpWIP.Size = new System.Drawing.Size(1257, 562);
            this.tpWIP.TabIndex = 1;
            this.tpWIP.Text = "Work View";
            this.tpWIP.UseVisualStyleBackColor = true;
            this.tpWIP.Click += new System.EventHandler(this.tabPage2_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer2.Location = new System.Drawing.Point(3, 50);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.splitContainer2.Panel1.Controls.Add(this.toolStrip4);
            this.splitContainer2.Panel1.Controls.Add(this.lvWork);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer2.Panel2.Controls.Add(this.tbWIPViews);
            this.splitContainer2.Panel2.Controls.Add(this.comboBox1);
            this.splitContainer2.Panel2.Controls.Add(this.textBox4);
            this.splitContainer2.Panel2.Controls.Add(this.textBox5);
            this.splitContainer2.Size = new System.Drawing.Size(1475, 530);
            this.splitContainer2.SplitterDistance = 600;
            this.splitContainer2.TabIndex = 20;
            this.splitContainer2.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer2_SplitterMoved);
            // 
            // toolStrip4
            // 
            this.toolStrip4.BackColor = System.Drawing.Color.White;
            this.toolStrip4.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbRemoveWIP,
            this.toolStripSeparator12,
            this.tsbRefreshStale,
            this.tsbRootNodeEdit,
            this.toolStripSeparator13,
            this.tsbWordWIP,
            this.tsWorkViewDocument});
            this.toolStrip4.Location = new System.Drawing.Point(0, 0);
            this.toolStrip4.Name = "toolStrip4";
            this.toolStrip4.Size = new System.Drawing.Size(600, 31);
            this.toolStrip4.TabIndex = 23;
            // 
            // tsbRemoveWIP
            // 
            this.tsbRemoveWIP.BackColor = System.Drawing.Color.White;
            this.tsbRemoveWIP.Image = global::DAMBuddy2.Properties.Resources.outline_remove_circle_outline_black_24dp;
            this.tsbRemoveWIP.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbRemoveWIP.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRemoveWIP.Name = "tsbRemoveWIP";
            this.tsbRemoveWIP.Size = new System.Drawing.Size(138, 28);
            this.tsbRemoveWIP.Text = "Remove from Work";
            this.tsbRemoveWIP.ToolTipText = "Remove from Work";
            this.tsbRemoveWIP.Click += new System.EventHandler(this.tsbRemoveWIP_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbRefreshStale
            // 
            this.tsbRefreshStale.BackColor = System.Drawing.Color.White;
            this.tsbRefreshStale.Image = global::DAMBuddy2.Properties.Resources.outline_text_snippet_black_24dp;
            this.tsbRefreshStale.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbRefreshStale.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRefreshStale.Name = "tsbRefreshStale";
            this.tsbRefreshStale.Size = new System.Drawing.Size(102, 28);
            this.tsbRefreshStale.Text = "Refresh Stale";
            this.tsbRefreshStale.Click += new System.EventHandler(this.tsbRefreshStale_Click);
            // 
            // tsbRootNodeEdit
            // 
            this.tsbRootNodeEdit.BackColor = System.Drawing.Color.White;
            this.tsbRootNodeEdit.Image = global::DAMBuddy2.Properties.Resources.rootnode_outline_call_split_black_24dp;
            this.tsbRootNodeEdit.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbRootNodeEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRootNodeEdit.Name = "tsbRootNodeEdit";
            this.tsbRootNodeEdit.Size = new System.Drawing.Size(110, 28);
            this.tsbRootNodeEdit.Text = "Rootnode Edit";
            this.tsbRootNodeEdit.Click += new System.EventHandler(this.toolStripButton1_Click_1);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbWordWIP
            // 
            this.tsbWordWIP.AutoSize = false;
            this.tsbWordWIP.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbWordWIP.Enabled = false;
            this.tsbWordWIP.Image = global::DAMBuddy2.Properties.Resources.outline_text_snippet_black_24dp;
            this.tsbWordWIP.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbWordWIP.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbWordWIP.Name = "tsbWordWIP";
            this.tsbWordWIP.Size = new System.Drawing.Size(48, 42);
            this.tsbWordWIP.Text = "Open In Word";
            this.tsbWordWIP.Visible = false;
            this.tsbWordWIP.Click += new System.EventHandler(this.tsbWordWIP_Click);
            // 
            // tsWorkViewDocument
            // 
            this.tsWorkViewDocument.Image = global::DAMBuddy2.Properties.Resources.outline_preview_black_24dp;
            this.tsWorkViewDocument.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsWorkViewDocument.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsWorkViewDocument.Name = "tsWorkViewDocument";
            this.tsWorkViewDocument.Size = new System.Drawing.Size(131, 28);
            this.tsWorkViewDocument.Text = "Refresh Transform";
            this.tsWorkViewDocument.Click += new System.EventHandler(this.tsWorkViewDocument_Click_1);
            // 
            // lvWork
            // 
            this.lvWork.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lvWork.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvWork.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvWork.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chFilename,
            this.chStale,
            this.chModified,
            this.chRootNode});
            this.lvWork.Font = new System.Drawing.Font("Arial Unicode MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lvWork.FullRowSelect = true;
            this.lvWork.GridLines = true;
            this.lvWork.HideSelection = false;
            this.lvWork.Location = new System.Drawing.Point(0, 34);
            this.lvWork.MultiSelect = false;
            this.lvWork.Name = "lvWork";
            this.lvWork.Size = new System.Drawing.Size(597, 475);
            this.lvWork.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvWork.TabIndex = 14;
            this.lvWork.UseCompatibleStateImageBehavior = false;
            this.lvWork.View = System.Windows.Forms.View.Details;
            this.lvWork.SelectedIndexChanged += new System.EventHandler(this.lvWork_SelectedIndexChanged);
            this.lvWork.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvWork_MouseDoubleClick);
            this.lvWork.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lvWork_MouseUp);
            // 
            // chFilename
            // 
            this.chFilename.Text = "Asset";
            this.chFilename.Width = 100;
            // 
            // chStale
            // 
            this.chStale.Text = "Freshness";
            this.chStale.Width = 100;
            // 
            // chModified
            // 
            this.chModified.Text = "Edited?";
            this.chModified.Width = 100;
            // 
            // chRootNode
            // 
            this.chRootNode.Text = "Rootnode Edit?";
            this.chRootNode.Width = 100;
            // 
            // tbWIPViews
            // 
            this.tbWIPViews.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.tbWIPViews.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbWIPViews.Controls.Add(this.tpPreviewWIP);
            this.tbWIPViews.Controls.Add(this.tpOverlapsWIP);
            this.tbWIPViews.Controls.Add(this.tpOverlaps2);
            this.tbWIPViews.Font = new System.Drawing.Font("Arial Unicode MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.tbWIPViews.Location = new System.Drawing.Point(0, 0);
            this.tbWIPViews.Multiline = true;
            this.tbWIPViews.Name = "tbWIPViews";
            this.tbWIPViews.Padding = new System.Drawing.Point(26, 6);
            this.tbWIPViews.SelectedIndex = 0;
            this.tbWIPViews.Size = new System.Drawing.Size(871, 530);
            this.tbWIPViews.TabIndex = 17;
            this.tbWIPViews.Selected += new System.Windows.Forms.TabControlEventHandler(this.tbWIPViews_Selected);
            // 
            // tpPreviewWIP
            // 
            this.tpPreviewWIP.Controls.Add(this.statusStrip2);
            this.tpPreviewWIP.Controls.Add(this.wbWIP);
            this.tpPreviewWIP.Location = new System.Drawing.Point(32, 4);
            this.tpPreviewWIP.Name = "tpPreviewWIP";
            this.tpPreviewWIP.Padding = new System.Windows.Forms.Padding(3);
            this.tpPreviewWIP.Size = new System.Drawing.Size(835, 522);
            this.tpPreviewWIP.TabIndex = 0;
            this.tpPreviewWIP.Text = "Document Preview";
            this.tpPreviewWIP.UseVisualStyleBackColor = true;
            // 
            // statusStrip2
            // 
            this.statusStrip2.BackColor = System.Drawing.Color.White;
            this.statusStrip2.Dock = System.Windows.Forms.DockStyle.Top;
            this.statusStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsStatusLabel,
            this.tsPBWIPTransform});
            this.statusStrip2.Location = new System.Drawing.Point(3, 3);
            this.statusStrip2.Name = "statusStrip2";
            this.statusStrip2.Size = new System.Drawing.Size(829, 26);
            this.statusStrip2.SizingGrip = false;
            this.statusStrip2.TabIndex = 22;
            this.statusStrip2.Text = "statusStrip2";
            // 
            // tsStatusLabel
            // 
            this.tsStatusLabel.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F);
            this.tsStatusLabel.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tsStatusLabel.Image = global::DAMBuddy2.Properties.Resources.outline_info_black_24dp;
            this.tsStatusLabel.Name = "tsStatusLabel";
            this.tsStatusLabel.Size = new System.Drawing.Size(62, 21);
            this.tsStatusLabel.Text = "Status";
            // 
            // tsPBWIPTransform
            // 
            this.tsPBWIPTransform.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsPBWIPTransform.Name = "tsPBWIPTransform";
            this.tsPBWIPTransform.Size = new System.Drawing.Size(200, 20);
            // 
            // wbWIP
            // 
            this.wbWIP.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.wbWIP.Location = new System.Drawing.Point(3, 32);
            this.wbWIP.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbWIP.Name = "wbWIP";
            this.wbWIP.Size = new System.Drawing.Size(615, 455);
            this.wbWIP.TabIndex = 5;
            this.wbWIP.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.wbWIP_DocumentCompleted);
            // 
            // tpOverlapsWIP
            // 
            this.tpOverlapsWIP.Controls.Add(this.wbWIPWUR);
            this.tpOverlapsWIP.Location = new System.Drawing.Point(32, 4);
            this.tpOverlapsWIP.Name = "tpOverlapsWIP";
            this.tpOverlapsWIP.Padding = new System.Windows.Forms.Padding(3);
            this.tpOverlapsWIP.Size = new System.Drawing.Size(835, 522);
            this.tpOverlapsWIP.TabIndex = 1;
            this.tpOverlapsWIP.Text = "Where Used Report";
            this.tpOverlapsWIP.UseVisualStyleBackColor = true;
            // 
            // wbWIPWUR
            // 
            this.wbWIPWUR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wbWIPWUR.Location = new System.Drawing.Point(3, 3);
            this.wbWIPWUR.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbWIPWUR.Name = "wbWIPWUR";
            this.wbWIPWUR.Size = new System.Drawing.Size(829, 516);
            this.wbWIPWUR.TabIndex = 6;
            // 
            // tpOverlaps2
            // 
            this.tpOverlaps2.Controls.Add(this.wbOverlaps);
            this.tpOverlaps2.Location = new System.Drawing.Point(32, 4);
            this.tpOverlaps2.Name = "tpOverlaps2";
            this.tpOverlaps2.Padding = new System.Windows.Forms.Padding(3);
            this.tpOverlaps2.Size = new System.Drawing.Size(835, 522);
            this.tpOverlaps2.TabIndex = 2;
            this.tpOverlaps2.Text = "Overlaps";
            this.tpOverlaps2.UseVisualStyleBackColor = true;
            // 
            // wbOverlaps
            // 
            this.wbOverlaps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wbOverlaps.Location = new System.Drawing.Point(3, 3);
            this.wbOverlaps.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbOverlaps.Name = "wbOverlaps";
            this.wbOverlaps.Size = new System.Drawing.Size(829, 516);
            this.wbOverlaps.TabIndex = 1;
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(517, 322);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(339, 28);
            this.comboBox1.Sorted = true;
            this.comboBox1.TabIndex = 9;
            // 
            // textBox4
            // 
            this.textBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox4.Location = new System.Drawing.Point(571, 387);
            this.textBox4.Multiline = true;
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(244, 52);
            this.textBox4.TabIndex = 16;
            this.textBox4.Visible = false;
            // 
            // textBox5
            // 
            this.textBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox5.Location = new System.Drawing.Point(612, 404);
            this.textBox5.Multiline = true;
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(244, 52);
            this.textBox5.TabIndex = 15;
            this.textBox5.Visible = false;
            // 
            // toolStrip2
            // 
            this.toolStrip2.AutoSize = false;
            this.toolStrip2.BackColor = System.Drawing.Color.Transparent;
            this.toolStrip2.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.ImageScalingSize = new System.Drawing.Size(48, 48);
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsddbRepositoryWIP,
            this.tsWorkReload,
            this.tsbLaunchTD,
            this.toolStripSeparator3,
            this.tsbDocReview,
            this.toolStripSeparator8,
            this.tsbPause,
            this.tsbStart,
            this.tsbHelp,
            this.tslReadyState,
            this.tslScheduleState,
            this.toolStripSeparator7,
            this.tsbWorkUpload});
            this.toolStrip2.Location = new System.Drawing.Point(3, 3);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStrip2.Size = new System.Drawing.Size(1251, 45);
            this.toolStrip2.TabIndex = 19;
            this.toolStrip2.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStrip2_ItemClicked);
            // 
            // tsddbRepositoryWIP
            // 
            this.tsddbRepositoryWIP.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.toolStripSeparator11,
            this.toolStripMenuItem4});
            this.tsddbRepositoryWIP.Font = new System.Drawing.Font("Arial Unicode MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tsddbRepositoryWIP.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tsddbRepositoryWIP.Image = global::DAMBuddy2.Properties.Resources.outline_confirmation_number_black_24dp;
            this.tsddbRepositoryWIP.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsddbRepositoryWIP.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsddbRepositoryWIP.Name = "tsddbRepositoryWIP";
            this.tsddbRepositoryWIP.Size = new System.Drawing.Size(164, 42);
            this.tsddbRepositoryWIP.Text = "< no repository >";
            this.tsddbRepositoryWIP.ToolTipText = "The current ticket being worked on, click to change.";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F);
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(201, 30);
            this.toolStripMenuItem3.Text = "CSDFK-1989";
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(198, 6);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F);
            this.toolStripMenuItem4.Image = global::DAMBuddy2.Properties.Resources.outline_confirmation_number_black_24dp;
            this.toolStripMenuItem4.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(201, 30);
            this.toolStripMenuItem4.Text = "Setup New Ticket...";
            // 
            // tsWorkReload
            // 
            this.tsWorkReload.AutoSize = false;
            this.tsWorkReload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsWorkReload.Image = global::DAMBuddy2.Properties.Resources.outline_cached_black_24dp;
            this.tsWorkReload.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsWorkReload.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsWorkReload.Name = "tsWorkReload";
            this.tsWorkReload.Size = new System.Drawing.Size(48, 42);
            this.tsWorkReload.Text = "Reload Available Assets";
            this.tsWorkReload.Click += new System.EventHandler(this.tsWorkReload_Click);
            // 
            // tsbLaunchTD
            // 
            this.tsbLaunchTD.Font = new System.Drawing.Font("Arial Unicode MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tsbLaunchTD.Image = global::DAMBuddy2.Properties.Resources.outline_account_tree_black_18dp;
            this.tsbLaunchTD.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbLaunchTD.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLaunchTD.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
            this.tsbLaunchTD.Name = "tsbLaunchTD";
            this.tsbLaunchTD.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.tsbLaunchTD.Size = new System.Drawing.Size(100, 42);
            this.tsbLaunchTD.Text = "Launch TD";
            this.tsbLaunchTD.ToolTipText = "Launch Template Designer";
            this.tsbLaunchTD.Click += new System.EventHandler(this.tsbLaunchTD_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 45);
            // 
            // tsbDocReview
            // 
            this.tsbDocReview.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tsbDocReview.Image = global::DAMBuddy2.Properties.Resources.outline_rate_review_black_24dp;
            this.tsbDocReview.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbDocReview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDocReview.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
            this.tsbDocReview.Name = "tsbDocReview";
            this.tsbDocReview.Size = new System.Drawing.Size(141, 42);
            this.tsbDocReview.Text = "Document Review";
            this.tsbDocReview.ToolTipText = "Open the Document Review Page";
            this.tsbDocReview.Click += new System.EventHandler(this.tsbDocReview_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 45);
            // 
            // tsbPause
            // 
            this.tsbPause.AutoSize = false;
            this.tsbPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbPause.Enabled = false;
            this.tsbPause.Image = global::DAMBuddy2.Properties.Resources.outline_pause_circle_outline_black_24dp;
            this.tsbPause.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPause.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
            this.tsbPause.Name = "tsbPause";
            this.tsbPause.Size = new System.Drawing.Size(42, 42);
            this.tsbPause.Text = "toolStripButton3";
            this.tsbPause.ToolTipText = "Pause Work";
            this.tsbPause.Click += new System.EventHandler(this.tsbPause_Click);
            // 
            // tsbStart
            // 
            this.tsbStart.AutoSize = false;
            this.tsbStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbStart.Enabled = false;
            this.tsbStart.Image = global::DAMBuddy2.Properties.Resources.outline_play_circle_outline_black_24dp;
            this.tsbStart.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbStart.Name = "tsbStart";
            this.tsbStart.Size = new System.Drawing.Size(42, 40);
            this.tsbStart.Text = "toolStripButton3";
            this.tsbStart.ToolTipText = "Start Work";
            this.tsbStart.Click += new System.EventHandler(this.tsbStart_Click);
            // 
            // tsbHelp
            // 
            this.tsbHelp.AutoSize = false;
            this.tsbHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbHelp.Image = global::DAMBuddy2.Properties.Resources.outline_help_outline_black_24dp;
            this.tsbHelp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbHelp.Name = "tsbHelp";
            this.tsbHelp.Size = new System.Drawing.Size(48, 42);
            this.tsbHelp.Text = "toolStripButton6";
            this.tsbHelp.ToolTipText = "Info on Scheduler States";
            this.tsbHelp.Click += new System.EventHandler(this.tsbHelp_Click);
            // 
            // tslReadyState
            // 
            this.tslReadyState.BackColor = System.Drawing.Color.White;
            this.tslReadyState.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tslReadyState.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
            this.tslReadyState.Name = "tslReadyState";
            this.tslReadyState.Size = new System.Drawing.Size(102, 42);
            this.tslReadyState.Text = "Work: Paused";
            this.tslReadyState.ToolTipText = "Whether the work is ready for scheduling";
            // 
            // tslScheduleState
            // 
            this.tslScheduleState.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tslScheduleState.Name = "tslScheduleState";
            this.tslScheduleState.Size = new System.Drawing.Size(135, 42);
            this.tslScheduleState.Text = "Schedule Position:";
            this.tslScheduleState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 45);
            // 
            // tsbWorkUpload
            // 
            this.tsbWorkUpload.Enabled = false;
            this.tsbWorkUpload.Image = global::DAMBuddy2.Properties.Resources.outline_cloud_upload_black_24dp;
            this.tsbWorkUpload.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbWorkUpload.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbWorkUpload.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
            this.tsbWorkUpload.Name = "tsbWorkUpload";
            this.tsbWorkUpload.Size = new System.Drawing.Size(157, 42);
            this.tsbWorkUpload.Text = "Upload to Repository";
            this.tsbWorkUpload.ToolTipText = "Upload to Repository";
            this.tsbWorkUpload.Click += new System.EventHandler(this.tsbWorkUpload_Click);
            // 
            // tpUpload
            // 
            this.tpUpload.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tpUpload.Location = new System.Drawing.Point(4, 35);
            this.tpUpload.Name = "tpUpload";
            this.tpUpload.Padding = new System.Windows.Forms.Padding(3);
            this.tpUpload.Size = new System.Drawing.Size(1257, 562);
            this.tpUpload.TabIndex = 3;
            this.tpUpload.Text = "Upload";
            this.tpUpload.UseVisualStyleBackColor = true;
            // 
            // tpSchedule
            // 
            this.tpSchedule.Location = new System.Drawing.Point(4, 35);
            this.tpSchedule.Name = "tpSchedule";
            this.tpSchedule.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.tpSchedule.Size = new System.Drawing.Size(1257, 562);
            this.tpSchedule.TabIndex = 5;
            this.tpSchedule.Text = "Schedule";
            this.tpSchedule.UseVisualStyleBackColor = true;
            // 
            // tpDocReview
            // 
            this.tpDocReview.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tpDocReview.Location = new System.Drawing.Point(4, 35);
            this.tpDocReview.Name = "tpDocReview";
            this.tpDocReview.Padding = new System.Windows.Forms.Padding(3);
            this.tpDocReview.Size = new System.Drawing.Size(1257, 562);
            this.tpDocReview.TabIndex = 4;
            this.tpDocReview.Text = "Document Review";
            this.tpDocReview.UseVisualStyleBackColor = true;
            // 
            // timerRepoFilter
            // 
            this.timerRepoFilter.Interval = 1000;
            this.timerRepoFilter.Tick += new System.EventHandler(this.timerRepoFilter_tick);
            // 
            // cmsRemove
            // 
            this.cmsRemove.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator9,
            this.refreshToolStripMenuItem,
            this.toolStripMenuItem1,
            this.tsmWIPRemove});
            this.cmsRemove.Name = "cmsRemove";
            this.cmsRemove.Size = new System.Drawing.Size(172, 60);
            this.cmsRemove.Text = "Remove";
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(168, 6);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.refreshToolStripMenuItem.Text = "Refresh from CKM";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(168, 6);
            // 
            // tsmWIPRemove
            // 
            this.tsmWIPRemove.Name = "tsmWIPRemove";
            this.tsmWIPRemove.Size = new System.Drawing.Size(171, 22);
            this.tsmWIPRemove.Text = "Remove from WIP";
            this.tsmWIPRemove.Click += new System.EventHandler(this.tsmWIPRemove_Click);
            // 
            // statusForm
            // 
            this.statusForm.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslMainStatus,
            this.toolStripDropDownButton1,
            this.toolStripStatusLabel1,
            this.tsslFolder});
            this.statusForm.Location = new System.Drawing.Point(0, 579);
            this.statusForm.Name = "statusForm";
            this.statusForm.Size = new System.Drawing.Size(1265, 22);
            this.statusForm.TabIndex = 22;
            this.statusForm.Text = "statusStrip3";
            // 
            // tslMainStatus
            // 
            this.tslMainStatus.Name = "tslMainStatus";
            this.tslMainStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logToolStripMenuItem});
            this.toolStripDropDownButton1.Image = global::DAMBuddy2.Properties.Resources.outline_api_black_24dp;
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(29, 20);
            this.toolStripDropDownButton1.Text = "toolStripDropDownButton1";
            // 
            // logToolStripMenuItem
            // 
            this.logToolStripMenuItem.Name = "logToolStripMenuItem";
            this.logToolStripMenuItem.Size = new System.Drawing.Size(94, 22);
            this.logToolStripMenuItem.Text = "Log";
            this.logToolStripMenuItem.Click += new System.EventHandler(this.logToolStripMenuItem_Click);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(610, 17);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tsslFolder
            // 
            this.tsslFolder.Name = "tsslFolder";
            this.tsslFolder.Size = new System.Drawing.Size(610, 17);
            this.tsslFolder.Spring = true;
            this.tsslFolder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tslScheduleStatus
            // 
            this.tslScheduleStatus.AutoSize = false;
            this.tslScheduleStatus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tslScheduleStatus.Font = new System.Drawing.Font("Arial Unicode MS", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.tslScheduleStatus.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tslScheduleStatus.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.tslScheduleStatus.Name = "tslScheduleStatus";
            this.tslScheduleStatus.Size = new System.Drawing.Size(180, 52);
            this.tslScheduleStatus.Text = "<unknown>";
            this.tslScheduleStatus.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1265, 601);
            this.Controls.Add(this.statusForm);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.textBox3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "DAMBuddy2 vDraft";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tpRepo.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tcRepoResults.ResumeLayout(false);
            this.tpRepoAll.ResumeLayout(false);
            this.tpRepoAll.PerformLayout();
            this.toolStrip5.ResumeLayout(false);
            this.toolStrip5.PerformLayout();
            this.tpRepoSearch.ResumeLayout(false);
            this.tpRepoSearch.PerformLayout();
            this.toolStrip3.ResumeLayout(false);
            this.toolStrip3.PerformLayout();
            this.tcRepository.ResumeLayout(false);
            this.tpRepoPreview.ResumeLayout(false);
            this.tpRepoPreview.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tpWUR.ResumeLayout(false);
            this.tpWIP.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.toolStrip4.ResumeLayout(false);
            this.toolStrip4.PerformLayout();
            this.tbWIPViews.ResumeLayout(false);
            this.tpPreviewWIP.ResumeLayout(false);
            this.tpPreviewWIP.PerformLayout();
            this.statusStrip2.ResumeLayout(false);
            this.statusStrip2.PerformLayout();
            this.tpOverlapsWIP.ResumeLayout(false);
            this.tpOverlaps2.ResumeLayout(false);
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.cmsRemove.ResumeLayout(false);
            this.statusForm.ResumeLayout(false);
            this.statusForm.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpRepo;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox cbTransforms;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TabPage tpWIP;
        private System.Windows.Forms.TabPage tpUpload;
        private System.Windows.Forms.TabPage tpDocReview;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbRepositoryReload;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox tstbRepositoryFilter;
        private System.Windows.Forms.ToolStripButton tsbRepositoryFilterClear;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel tspTime;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton tsWorkReload;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListView lvWork;
        private System.Windows.Forms.ColumnHeader chFilename;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.ToolStripLabel toolStripLabel5;
        private System.Windows.Forms.ToolStripTextBox tstbRepositorySearch;
        private System.Windows.Forms.ToolStripButton tsbWorkUpload;
        private System.Windows.Forms.TabPage tpSchedule;
        private System.Windows.Forms.ToolStripLabel tslScheduleStatus;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Timer timerRepoFilter;
        private System.Windows.Forms.ToolStripButton tsbRepoSearch;
        private System.Windows.Forms.TabControl tcRepoResults;
        private System.Windows.Forms.TabPage tpRepoAll;
        private System.Windows.Forms.ListView lvRepository;
        private System.Windows.Forms.ColumnHeader Asset;
        private System.Windows.Forms.TabPage tpRepoSearch;
        private System.Windows.Forms.ListView lvRepoSearchResults;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ColumnHeader chStale;
        private System.Windows.Forms.ContextMenuStrip cmsRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem tsmWIPRemove;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ColumnHeader chModified;
        private System.Windows.Forms.TabControl tbWIPViews;
        private System.Windows.Forms.TabPage tpPreviewWIP;
        private System.Windows.Forms.WebBrowser wbWIP;
        private System.Windows.Forms.TabPage tpOverlapsWIP;
        private System.Windows.Forms.WebBrowser wbWIPWUR;
        private System.Windows.Forms.StatusStrip statusStrip2;
        private System.Windows.Forms.ToolStripStatusLabel tsStatusLabel;
        private System.Windows.Forms.ToolStripProgressBar tsPBWIPTransform;
        private System.Windows.Forms.ColumnHeader chRootNode;
        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.TabControl tcRepository;
        private System.Windows.Forms.TabPage tpRepoPreview;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tspStatusLabel;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar2;
        private System.Windows.Forms.WebBrowser wbRepositoryView;
        private System.Windows.Forms.TabPage tpWUR;
        private System.Windows.Forms.WebBrowser wbRepoWUR;
        private System.Windows.Forms.ToolStripButton tsbStart;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripLabel tslReadyState;
        private System.Windows.Forms.ToolStripButton tsbPause;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.StatusStrip statusForm;
        private System.Windows.Forms.ToolStripStatusLabel tslMainStatus;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem logToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton tsbLaunchTD;
        private System.Windows.Forms.ToolStripButton tsbLaunch2;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
        private System.Windows.Forms.ToolStripMenuItem tsmiUserAccount;
        private System.Windows.Forms.ToolStripMenuItem setupNewTicketToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton tsddbRepository;
        private System.Windows.Forms.ToolStripMenuItem tsmiAvailableRepo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem tsmSetupTicket;
        private System.Windows.Forms.ToolStripButton tsbHelp;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TabPage tpOverlaps2;
        private System.Windows.Forms.WebBrowser wbOverlaps;
        private System.Windows.Forms.ToolStripDropDownButton tsddbRepositoryWIP;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripButton tsbDocReview;
        private System.Windows.Forms.ToolStripStatusLabel tsslFolder;
        private System.Windows.Forms.ToolStrip toolStrip3;
        private System.Windows.Forms.ToolStripButton tsbAddFromSearch;
        private System.Windows.Forms.ToolStrip toolStrip4;
        private System.Windows.Forms.ToolStripButton tsbRemoveWIP;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripButton tsbRootNodeEdit;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStrip toolStrip5;
        private System.Windows.Forms.ToolStripButton tsbAddWIP;
        private System.Windows.Forms.ToolStripButton tsbNext;
        private System.Windows.Forms.ToolStripLabel tslPageCount;
        private System.Windows.Forms.ToolStripButton tsbPrev;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripButton tsbWordWIP;
        private System.Windows.Forms.ToolStripButton tsWorkViewDocument;
        private System.Windows.Forms.ToolStripButton tsbRepositoryViewDocument;
        private System.Windows.Forms.ToolStripButton tsbWord;
        private System.Windows.Forms.ToolStripLabel tslScheduleState;
        private System.Windows.Forms.ToolStripButton tsbRefreshStale;
    }
}

