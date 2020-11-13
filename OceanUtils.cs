using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace DAMBuddy2
{

    class OceanUtils
    {
        public const string OceanDir = "/Ocean_Informatics";
        static System.Collections.Specialized.StringCollection log = new System.Collections.Specialized.StringCollection();
        static string filename;
        static DateTime lastwrite;


        public static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }


        public static bool ReapplyDefaults(string filepath, ref System.Collections.Generic.SortedDictionary<string, string> annotationDefaults)
        {
            System.Xml.XmlDocument AnnotationsDoc = new XmlDocument();
            AnnotationsDoc.Load(filepath);


            // get list of DAM element sections, iterate through their child nodes, adding each to a new node that will be added to TDConfigDoc
            XmlNodeList annotationSets = AnnotationsDoc.DocumentElement.SelectNodes("/ArrayOfAnnotationSet/AnnotationSet");


            foreach (XmlElement confignode in annotationSets)
            {
                var Names = confignode.GetElementsByTagName("Name");
                var name = Names.Item(0);
                var Defaults = confignode.GetElementsByTagName("IsDefault");
                var Default = Defaults.Item(0);
                var theOriginalDefault = "";
                if (annotationDefaults.TryGetValue(name.InnerText, out theOriginalDefault))
                {
                    Default.InnerText = theOriginalDefault;
                }
            }

            AnnotationsDoc.Save(filepath);

            return true;
        }

        public static bool GetAnnotationDefaults(string filepath, ref System.Collections.Generic.SortedDictionary<string, string> annotationDefaults)
        {
            System.Xml.XmlDocument AnnotationsDoc = new XmlDocument();
            AnnotationsDoc.Load(filepath);


            // get list of DAM element sections, iterate through their child nodes, adding each to a new node that will be added to TDConfigDoc
            XmlNodeList annotationSets = AnnotationsDoc.DocumentElement.SelectNodes("/ArrayOfAnnotationSet/AnnotationSet");


            foreach (XmlElement confignode in annotationSets)
            {
                var Names = confignode.GetElementsByTagName("Name");
                var name = Names.Item(0);
                var Defaults = confignode.GetElementsByTagName("IsDefault");
                var Default = Defaults.Item(0);

                annotationDefaults.Add(name.InnerText, Default.InnerText);
            }

            return false;
        }



        public static bool SyncAnnotationSets(string masterlocation, ref string messages, ref DateTime dateAnnotations)
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Ocean Informatics\\Template Designer";

            try
            {
                dateAnnotations = System.IO.File.GetLastWriteTime(masterlocation);

                string destpath = Path.Combine(appdata, "AnnotationSets.xml");

                if (File.Exists(destpath))
                {
                    FileAttributes attributes = File.GetAttributes(destpath);
                    attributes = attributes & ~FileAttributes.ReadOnly;
                    File.SetAttributes(destpath, attributes);
                    var defs = new SortedDictionary<string, string>();
                    GetAnnotationDefaults(destpath, ref defs);
                    File.Copy(masterlocation, destpath, true);
                    ReapplyDefaults(destpath, ref defs);
                }
            }
            catch (SystemException e)
            {
                messages += (e.Message) + "\n";
                messages += ("Something untoward happened in SyncAnnotationSets() :-(" + "\n");
                MessageBox.Show("Error in SyncAnnotationSets() + " + e.Message);
                return false;

            }

            return true;
        }

        static public void RemoveConfig(string TDConfigFile, string TicketDir)
        {

            System.Xml.XmlDocument TDConfigDoc = new XmlDocument();
            TDConfigDoc.Load(TDConfigFile);
            XmlNode TDRepositoryData = TDConfigDoc.DocumentElement.SelectSingleNode("/configuration/userSettings/TemplateTool.Properties.Settings/setting[@name='RepositoryList']/value/ArrayOfRepositoryData");
            List<XmlNode> toRemove = new List<XmlNode>();

            // strip out any existing data for this incoming repository
            foreach (XmlElement confignode in TDRepositoryData)
            {
                if (confignode.FirstChild.InnerText == TicketDir)
                {
                    toRemove.Add(confignode);
                }
            }
            foreach (XmlElement confignode in toRemove)
            {
                XmlNode node = confignode.ParentNode;
                node.RemoveChild(confignode);
            }

            TDConfigDoc.Save(TDConfigFile);
            return;

        }

        static public void AddConfig2(string TDConfigFile, string DAMConfig, string TicketDir)
        {
            System.Xml.XmlDocument TDConfigDoc = new XmlDocument();
            TDConfigDoc.Load(TDConfigFile);

            System.Xml.XmlDocument DAMConfigDoc = new XmlDocument();
            DAMConfigDoc.LoadXml(DAMConfig);


            XmlNode name = DAMConfigDoc.DocumentElement.SelectSingleNode("/DAM/RepositoryData");
            XmlNode TDRepositoryData = TDConfigDoc.DocumentElement.SelectSingleNode("/configuration/userSettings/TemplateTool.Properties.Settings/setting[@name='RepositoryList']/value/ArrayOfRepositoryData");

            //            string thename = name.FirstChild.InnerText;
            List<XmlNode> toRemove = new List<XmlNode>();

            // strip out any existing data for this incoming repository
            foreach (XmlElement confignode in TDRepositoryData)
            {
                if (confignode.FirstChild.InnerText == TicketDir)
                {
                    toRemove.Add(confignode);
                }
            }
            foreach (XmlElement confignode in toRemove)
            {
                XmlNode node = confignode.ParentNode;
                node.RemoveChild(confignode);
            }

            // get list of DAM element sections, iterate through their child nodes, adding each to a new node that will be added to TDConfigDoc
            XmlNodeList DAMconfigNodes = DAMConfigDoc.DocumentElement.SelectNodes("/DAM/RepositoryData");

            foreach (XmlElement confignode in DAMconfigNodes)
            {
                XmlNode newRepositoryData = TDConfigDoc.ImportNode(confignode, true);
                TDRepositoryData.AppendChild(newRepositoryData);
            }



            XmlNode integ = TDConfigDoc.DocumentElement.SelectSingleNode("/configuration/userSettings/TemplateTool.Properties.Settings/setting[@name='UseIntegrityChecks']/value");
            if (integ is null)
            {

            }
            else
            {
                integ.FirstChild.Value = "True";
            }


            XmlNode selectedrepo = TDConfigDoc.DocumentElement.SelectSingleNode("/configuration/userSettings/TemplateTool.Properties.Settings/setting[@name='SelectedRepository']/value");
            if (selectedrepo is null)
            {

            }
            else
            {
                if (!(selectedrepo.FirstChild is null))
                {
                    selectedrepo.FirstChild.Value = TicketDir;
                }

            }
            TDConfigDoc.Save(TDConfigFile);
            return;
        }

        public static bool WriteConfig2(string[] args, ref string messages, ref string DAMEnvironment, ref string RepoName, ref string CacheDir, string uniqueDir, ref string ServerName, ref string FolderName)
        {

            string text = "";

            if (args.Length < 1)
            {

                messages += ("Configuration updated. Nothing more to do.\n");

                return true;
            }

            messages += (args[0]) + "\n";

            if (File.Exists(args[0]))
            {
                text = System.IO.File.ReadAllText(@args[0]);
            }
            else
            {
                messages += ("Can't find " + args[0] + " can't do anything :(" + "\n");

                return false;
            }

            // support for the new samba share.
            text = text.Replace("Q:\\DAM\\TST", "\\\\UXCKCM01\\DAM\\TST");

            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string Ocean = appdata + OceanDir;

            messages += (appdata) + "\n" + "\n";
            DirectoryInfo diOcean = new DirectoryInfo(Ocean);
            WalkDirectoryTree(diOcean, ref messages);

            messages += (text) + "\n";
            messages += ("Storing config in " + filename + "\n");

            // cache support

            XmlDocument tempDoc = new XmlDocument();
            tempDoc.LoadXml(text);
            CacheDir = "c:\\temp\\" + uniqueDir;
            string ticketDir = tempDoc.SelectSingleNode("/DAM/RepositoryData/RepositoryName").InnerText;
            string templatepath = tempDoc.SelectSingleNode("/DAM/RepositoryData/TemplatesPath").InnerText;
            string[] seps = { "\\DAM\\" };
            string[] pieces = templatepath.Split(seps, StringSplitOptions.None);
            string Env = pieces[1];
            string Server = pieces[0];
            ServerName = Server.Replace("\\", "");
            DAMEnvironment = Env.Remove(3);
            string[] seps2 = { "\\" };
            pieces = Env.Split(seps2, StringSplitOptions.RemoveEmptyEntries);
            FolderName = pieces[1];

            tempDoc.SelectSingleNode("/DAM/RepositoryData/TemplatesPath").InnerText = CacheDir + "\\" + FolderName + "\\Templates";
            tempDoc.SelectSingleNode("/DAM/RepositoryData/ArchetypesPath").InnerText = CacheDir + "\\" + FolderName + "\\Archetypes";
            text = tempDoc.InnerXml;

            try
            {
                RepoName = AddConfig(filename, text);
                messages += ("Added OK! :-)");

            }
            catch (SystemException e)
            {
                messages += (e.Message) + "\n";
                messages += ("Something unsettling happened :-(" + "\n");
                return false;
            }


            return true;
        }


/*        public static bool WriteConfig(string[] args, ref string messages, ref string RepoName)
        {

            string text = "";

            if (args.Length < 1)
            {

                messages += ("Configuration updated. Nothing more to do.\n");

                return true;
            }

            messages += (args[0]) + "\n";

            if (File.Exists(args[0]))
            {
                text = System.IO.File.ReadAllText(@args[0]);
            }
            else
            {
                messages += ("Can't find " + args[0] + " can't do anything :(" + "\n");

                return false;
            }

            // support for the new samba share.
            text = text.Replace("Q:\\DAM\\TST", "\\\\UXCKCM01\\DAM\\TST");

            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string Ocean = appdata + OceanDir;

            messages += (appdata) + "\n" + "\n";
            DirectoryInfo diOcean = new DirectoryInfo(Ocean);
            WalkDirectoryTree(diOcean, ref messages);

            messages += (text) + "\n";
            messages += ("Storing config in " + filename + "\n");

            try
            {
                RepoName = AddConfig(filename, text);
                messages += ("Added OK! :-)");

            }
            catch (SystemException e)
            {
                messages += (e.Message) + "\n";
                messages += ("Something unsettling happened :-(" + "\n");
                return false;
            }


            return true;
        }*/

        static string AddConfig(string TDConfigFile, string DAMConfig)
        {
            System.Xml.XmlDocument TDConfigDoc = new XmlDocument();
            TDConfigDoc.Load(TDConfigFile);

            System.Xml.XmlDocument DAMConfigDoc = new XmlDocument();
            DAMConfigDoc.LoadXml(DAMConfig);


            XmlNode name = DAMConfigDoc.DocumentElement.SelectSingleNode("/DAM/RepositoryData");
            XmlNode TDRepositoryData = TDConfigDoc.DocumentElement.SelectSingleNode("/configuration/userSettings/TemplateTool.Properties.Settings/setting[@name='RepositoryList']/value/ArrayOfRepositoryData");

            string thename = name.FirstChild.InnerText;
            List<XmlNode> toRemove = new List<XmlNode>();

            // strip out any existing data for this incoming repository
            foreach (XmlElement confignode in TDRepositoryData)
            {
                if (confignode.FirstChild.InnerText == thename)
                {
                    toRemove.Add(confignode);
                }
            }
            foreach (XmlElement confignode in toRemove)
            {
                XmlNode node = confignode.ParentNode;
                node.RemoveChild(confignode);
            }

            // get list of DAM element sections, iterate through their child nodes, adding each to a new node that will be added to TDConfigDoc
            XmlNodeList DAMconfigNodes = DAMConfigDoc.DocumentElement.SelectNodes("/DAM/RepositoryData");

            foreach (XmlElement confignode in DAMconfigNodes)
            {
                XmlNode newRepositoryData = TDConfigDoc.ImportNode(confignode, true);
                TDRepositoryData.AppendChild(newRepositoryData);
            }



            XmlNode integ = TDConfigDoc.DocumentElement.SelectSingleNode("/configuration/userSettings/TemplateTool.Properties.Settings/setting[@name='UseIntegrityChecks']/value");
            if (integ is null)
            {

            }
            else
            {
                integ.FirstChild.Value = "True";
            }


            XmlNode selectedrepo = TDConfigDoc.DocumentElement.SelectSingleNode("/configuration/userSettings/TemplateTool.Properties.Settings/setting[@name='SelectedRepository']/value");
            if (selectedrepo is null)
            {

            }
            else
            {
                selectedrepo.FirstChild.Value = thename;
            }




            TDConfigDoc.Save(TDConfigFile);
            return thename;

        }

        static public string WalkDirectoryTree(System.IO.DirectoryInfo root, ref string messages)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

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
                log.Add(e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                messages += (e.Message) + "\n";
            }

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    messages += (fi.FullName);
                    messages += (fi.LastWriteTime);

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
                    WalkDirectoryTree(dirInfo, ref messages);
                }
            }
            return filename;
        }

    }

    public class FileAssociation
    {
        public string Extension { get; set; }
        public string ProgId { get; set; }
        public string FileTypeDescription { get; set; }
        public string ExecutableFilePath { get; set; }
    }

    public class FileAssociations
    {
        // needed so that Explorer windows get refreshed after the registry is updated
        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        private const int SHCNE_ASSOCCHANGED = 0x8000000;
        private const int SHCNF_FLUSH = 0x1000;

        public static void EnsureAssociationsSet()
        {
            var filePath = Process.GetCurrentProcess().MainModule.FileName;
            EnsureAssociationsSet(
                new FileAssociation
                {
                    Extension = ".damconf",
                    ProgId = "DAMBuddy_Config_File",
                    FileTypeDescription = "DAM File",
                    ExecutableFilePath = filePath
                });
        }

        public static void EnsureAssociationsSet(params FileAssociation[] associations)
        {
            bool madeChanges = false;
            foreach (var association in associations)
            {
                madeChanges |= SetAssociation(
                    association.Extension,
                    association.ProgId,
                    association.FileTypeDescription,
                    association.ExecutableFilePath);
            }

            if (madeChanges)
            {
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public static bool SetAssociation(string extension, string progId, string fileTypeDescription, string applicationFilePath)
        {
            bool madeChanges = false;
            madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + extension, progId);
            madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + progId, fileTypeDescription);
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\shell\open\command", "\"" + applicationFilePath + "\" \"%1\"");
            return madeChanges;
        }

        private static bool SetKeyDefaultValue(string keyPath, string value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                if (key.GetValue(null) as string != value)
                {
                    key.SetValue(null, value);
                    return true;
                }
            }

            return false;
        }
    }
}

