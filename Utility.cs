using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Net;
using System.Net.Http;

namespace DAMBuddy2
{
    class Utility
    {

        public static void PutSettingString(string sName, string sValue)
        {

            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;

            if( settings[sName] == null )
            {
                settings.Add(sName, sValue);
            }
            else
            {
                settings[sName].Value = sValue;
             
            }

            configFile.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);

            return;
        }

        public static async Task<bool> AuthorizeUserAsync( string sUser, string sPassword )
        {
            bool result = false;
            string sSessionURL = Utility.GetSettingString("CKMSessionURL");

            using (var httpClient = new HttpClient())
            {
                var byteArray = Encoding.ASCII.GetBytes($"{sUser}:{sPassword}");
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                using (var request = new HttpRequestMessage(new HttpMethod("POST"), sSessionURL))
                {
                    request.Headers.TryAddWithoutValidation("accept", "application/xml");

                    var response = await httpClient.SendAsync(request);

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        MessageBox.Show("Failed to authenticate.\n\nPlease update your user account credentials.", "Problem", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        result = true;
                    }

                }
            }


            return result;
        }

        public static string GetSettingString( string sName )
        {
            return ConfigurationManager.AppSettings[sName];

        }

        public static int GetSettingInt(string sName)
        {
            return Int32.Parse( ConfigurationManager.AppSettings[sName] );

        }

        public static void MakeAllWritable( string folderpath )
        {
            var readOnlyFiles = new DirectoryInfo(folderpath)
                                    .EnumerateFiles("*", SearchOption.AllDirectories)
                                    .Where(file => file.Attributes.HasFlag(FileAttributes.ReadOnly));

            foreach (FileInfo fi in readOnlyFiles)
            {
                File.SetAttributes(fi.FullName, FileAttributes.Normal);
            }

            var readOnlyDirs = new DirectoryInfo(folderpath)
                .EnumerateDirectories("*", SearchOption.AllDirectories);

            foreach (DirectoryInfo di in readOnlyDirs)
            {
                File.SetAttributes(di.FullName, System.IO.FileAttributes.Normal);
            }

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

        public static string HighlightHtml(string SearchText, HtmlDocument doc2)
        {
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



        public static void MakeMd5(string filepath)
        {
            // strip all spaces and write md5 to asset.oet.md5
            string assetcontent = ReadAsset(filepath);
            //string hashvalue = "";
            byte[] hashBytes = { };

            using (var md5 = MD5.Create())
            {
                hashBytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(assetcontent));
            }

            if (File.Exists(filepath + ".md5")) File.Delete(filepath + ".md5");
            string hex = BitConverter.ToString(hashBytes);
            File.WriteAllText(filepath + ".md5", hex);
        }



        public static string GetTemplateID( string filepath)
        {
            //string template = File.ReadAllText(filepath);
            string template = "";

            if (!File.Exists(filepath)) return "";


            using (FileStream fs = new FileStream(filepath,
                                      FileMode.Open,
                                      FileAccess.Read,
                                      FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (sr.Peek() >= 0) // reading the old data
                    {
                        template += sr.ReadLine();
                        //index++;
                    }
                }
            }


            string tID = "";

            //char[] token = "<id>".ToCharArray();
            string[] tokens = { "<id>" };

            var parts = template.Split(tokens, StringSplitOptions.RemoveEmptyEntries);
            
            if ( parts.Length > 1 ) 
            {

                tID = parts[1].Substring(0, 36);
            }

            return tID;
            //var parts = template.Split(  ["<id>"],1 );

        }
    }
}
