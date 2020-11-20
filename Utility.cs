using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAMBuddy2
{
    class Utility
    {
        public static string GetTemplateID( string filepath)
        {
                string template = File.ReadAllText(filepath);
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
