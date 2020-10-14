using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace DAMBuddy2
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private string m_Ticket = "";

        private GitManager manager = null;

        private static string m_Repository = "https://github.com/ahs-ckm/ckm-mirror";

        public string Ticket { get => m_Ticket; set => m_Ticket = value; }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = textBox1.Text;

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);

            //            fileSystemWatcher1.Path = path;
            //            fileSystemWatcher1.IncludeSubdirectories = true;
            //           fileSystemWatcher1.EnableRaisingEvents = true;


            DateTime currentDate = DateTime.Now;
            label1.Text = currentDate.ToString();
            Application.DoEvents();
            CloneOptions options = new CloneOptions();
            options.OnTransferProgress = Form2.TransferProgress;

            Repository.Clone(m_Repository, path, options);
            DateTime finishDate = DateTime.Now;

            label2.Text = finishDate.ToString();
        }

        public static bool TransferProgress(TransferProgress progress)
        {
            Console.WriteLine($"Objects: {progress.ReceivedObjects} of {progress.TotalObjects}");
            return true;
        }

        private void fileSystemWatcher1_Created(object sender, System.IO.FileSystemEventArgs e)
        {

            //checkedListBox1.Items.Add(e.Name);
            //Application.DoEvents();
            //e.Name
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            PullOptions options = new PullOptions();
            options.FetchOptions = new FetchOptions();
            //options.FetchOptions.OnProgress = TransferProgress;
            Repository repo = new Repository(textBox1.Text);

            //repo.Network.Pull(new Signature("truecraft", "git@truecraft.io", new DateTimeOffset(DateTime.Now)), options);
            Commands.Pull(repo, new Signature("truecraft", "git@truecraft.io", new DateTimeOffset(DateTime.Now)), options);
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            textBox1.Text = m_Ticket;
        }

        private void button3_Click(object sender, EventArgs e)
        {
//            manager = new GitManager( textBox1.Text );

        }

        private void button4_Click(object sender, EventArgs e)
        {
            manager.Init((1000 * 60), (1000 * 60));

        }

        private void button5_Click(object sender, EventArgs e)
        {
            manager.AddWIP( textBox2.Text);

            //C:\TD\git1\mgr\local\templates\mgr\local\templates\section

        }

        private void button6_Click(object sender, EventArgs e)
        {
            manager.DoClone();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //manager = new GitManager(textBox1.Text);
            //manager.Init(( 1000 * 30 ), (1000 * 60));
   
        }
    }
}

