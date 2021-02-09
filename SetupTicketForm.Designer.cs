namespace DAMBuddy2
{
    partial class SetupTicketForm
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
            this.tbTicket = new System.Windows.Forms.TextBox();
            this.cbPrefix = new System.Windows.Forms.ComboBox();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.timerSearch = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // tbTicket
            // 
            this.tbTicket.Location = new System.Drawing.Point(187, 12);
            this.tbTicket.Margin = new System.Windows.Forms.Padding(4);
            this.tbTicket.Name = "tbTicket";
            this.tbTicket.Size = new System.Drawing.Size(203, 25);
            this.tbTicket.TabIndex = 0;
            this.tbTicket.TextChanged += new System.EventHandler(this.tbTicket_TextChanged);
            this.tbTicket.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbTicket_KeyDown);
            // 
            // cbPrefix
            // 
            this.cbPrefix.FormattingEnabled = true;
            this.cbPrefix.Items.AddRange(new object[] {
            "CSDFK-",
            "CSDCD-",
            "CSDCKT-"});
            this.cbPrefix.Location = new System.Drawing.Point(17, 12);
            this.cbPrefix.Margin = new System.Windows.Forms.Padding(4);
            this.cbPrefix.Name = "cbPrefix";
            this.cbPrefix.Size = new System.Drawing.Size(160, 26);
            this.cbPrefix.TabIndex = 1;
            this.cbPrefix.SelectedIndexChanged += new System.EventHandler(this.cbPrefix_SelectedIndexChanged);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(17, 53);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(27, 28);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(812, 255);
            this.webBrowser1.TabIndex = 2;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(399, 8);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(4);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(100, 32);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Visible = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(621, 327);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 32);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(729, 327);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 32);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // timerSearch
            // 
            this.timerSearch.Interval = 1000;
            this.timerSearch.Tick += new System.EventHandler(this.timerSearch_Tick);
            // 
            // SetupTicketForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(846, 375);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.cbPrefix);
            this.Controls.Add(this.tbTicket);
            this.Font = new System.Drawing.Font("Arial Unicode MS", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "SetupTicketForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Setup Work Ticket";
            this.Load += new System.EventHandler(this.SetupTicketForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbTicket;
        private System.Windows.Forms.ComboBox cbPrefix;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Timer timerSearch;
    }
}