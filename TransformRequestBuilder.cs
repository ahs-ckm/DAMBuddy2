using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace DAMBuddy2
{
    class TransformRequestBuilder
    {
        private RepoManager m_repoManager;
        private string m_sURLCacheService;

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


        public TransformRequestBuilder(RepoManager repoManager, string sURLCacheService)
        {
            m_repoManager = repoManager;
            m_sURLCacheService = sURLCacheService;
        }


        void ExtractArchetypesAndTemplates( ref List<string> listIdArchetypes, ref List<string> listEmbeddedTemplates, string sTemplateXML)
        {
            System.Xml.Linq.XDocument xtemplate = null;
            try
            {
                xtemplate = XDocument.Parse(sTemplateXML);
            }
            catch (Exception e)
            {
                throw e;
            }

            var defs = xtemplate.Descendants().Where(p => p.Name.LocalName == "integrity_checks");
            if (defs != null)
            {
                foreach (XElement def in defs)
                {
                    var archid = def.Attribute("archetype_id");
                    listIdArchetypes.Add(archid.Value);
                }
            }

            var names = xtemplate.Descendants().Where(p => p.Name.LocalName == "name").FirstOrDefault();
            if (names != null)
            {
                Console.WriteLine("Template Name = " + names.Value);
            }

            List<string> sValue = new List<string>();
            xtemplate.Descendants().Where(p => p.Name.LocalName == "Item")
                         .ToList()
                         .ForEach(e =>
                         {
                             XAttribute tid = e.Attribute("template_id");
                             if (tid != null)
                             {
                                 Console.WriteLine(tid.Value);
                                 string Value = tid.Value;

                                 sValue.Add(Value);
                                 

                             }
                             //Console.WriteLine(e);
                         });

           foreach( string template in sValue)
            {
                listEmbeddedTemplates.Add(template);
            }
        }

        public bool BuildRequest(string filepathTemplate, ref string sSOAPRequest)
        {
            string work = "";
            var sTemplateXML = GetTemplateXML(filepathTemplate);

            string sAllEmbeddedTemplateXML = "";
            string sAllArchetypesXML = "";
            string request = "";

            List<string> listAllEmbedded = new List<string>();

            if (GetAllXML(ref sAllEmbeddedTemplateXML, ref sAllArchetypesXML, sTemplateXML, ref listAllEmbedded))
            {
                request += startBlock;
                request += sTemplateXML;
                request += "<archetypes>";
                request += sAllArchetypesXML;
                request += "</archetypes>";
                request += "<embeddedTemplates>";
                request += sAllEmbeddedTemplateXML;
                request += "</embeddedTemplates>";
                request += endBlock;
            }

            sSOAPRequest = request;
            return true;
        }

        private bool GetAllXML(ref string sAllEmbeddedTemplateXML, ref string sAllArchetypesXML, string sTemplateXML, ref List<string> listAllEmbeddedTemplateIDs)
        {
            string sTemplateFile = "";
            List<string> listArch = new List<string>() ;
            List<string> listThisLevelEmbedded = new List<string>();

            try
            {
                ExtractArchetypesAndTemplates(ref listArch, ref listThisLevelEmbedded, sTemplateXML);
            }
            catch (Exception e)
            {
                throw e;
            }

            string sTempXML = "";

            foreach (string sArchID in listArch)
            {
                if (!sAllArchetypesXML.Contains(sArchID))
                {
                    sTempXML = File.ReadAllText(m_repoManager.LocalPath + @"\ArchetypeXML\" + sArchID + ".xml");

                    sTempXML = sTempXML.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "");
                    sTempXML = sTempXML.Replace("<archetype xmlns=\"http://schemas.openehr.org/v1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">", "");
                    sTempXML = "<opt:ARCHETYPE xmlns=\"http://schemas.openehr.org/v1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:opt=\"http://www.oceaninformatics.org/OPTWS\">" + sTempXML;
                    sTempXML = sTempXML.Replace("</archetype>", "</opt:ARCHETYPE>");
                    sAllArchetypesXML += sTempXML;

                }
            }

            if (listThisLevelEmbedded != null)
            {
                foreach (string sEmbeddedId in listThisLevelEmbedded)
                {
                    if (listAllEmbeddedTemplateIDs.Contains(sEmbeddedId.ToLower())) continue;

                    listAllEmbeddedTemplateIDs.Add(sEmbeddedId.ToLower());
                    string sRelPath = "";
                    string sTemplateFilePath = "";

                    if (m_repoManager.isAssetinWIPByID( sEmbeddedId, ref sTemplateFilePath ))
                    {
                        
                    } else
                    {
                        sRelPath = GetTemplatePathFromID(sEmbeddedId);
                        sTemplateFilePath = m_repoManager.LocalPath + sRelPath;

                    }


                    string sEmbeddedTemplateXML = GetTemplateXML(sTemplateFilePath);
                    

                    GetAllXML(ref sAllEmbeddedTemplateXML, ref sAllArchetypesXML, sEmbeddedTemplateXML, ref listAllEmbeddedTemplateIDs);
                    
                    sanitizeEmbeddedXML2(ref sEmbeddedTemplateXML);
                    sAllEmbeddedTemplateXML += sEmbeddedTemplateXML;

                }

            }

            return true;

        }

        private void sanitizeEmbeddedXML2(ref string sXML)
        {
            int pos = sXML.IndexOf("<id>");
            if (pos < 1) return;
            sXML = sXML.Remove(0, pos);
            sXML = @"<opt:TEMPLATE xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='openEHR/v1/Template' xmlns:opt='http://www.oceaninformatics.org/OPTWS'>" + sXML;
            sXML = sXML.Replace(@"</template>", @"</opt:TEMPLATE>");

        }

        private void sanitizeEmbeddedXML(ref string sXML)
        {
            sXML = sXML.Replace("<?xml version=\"1.0\"?>", "");
            sXML = sXML.Replace("<template xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"openEHR/v1/Template\">", @"<opt:TEMPLATE xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='openEHR/v1/Template' xmlns:opt='http://www.oceaninformatics.org/OPTWS'>");
            sXML = sXML.Replace(@"</template>", @"</opt:TEMPLATE>");
        }

        private string GetTemplateXML(string sTemplateFilepath)
        {
            var sXML = "";
            sXML = File.ReadAllText(sTemplateFilepath);

            int pos = sXML.IndexOf("<template");
            if (pos < 1) return sXML;
            sXML = sXML.Remove(0, pos);

            //sXML = sXML.Replace("<?xml version=\"1.0\"?>", "");
            return sXML;
        }


        private string GetTemplatePathFromID(string sEmbeddedId)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_sURLCacheService + "," + sEmbeddedId);
            //request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
            

        }

    }
}
