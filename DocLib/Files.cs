using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace DocLib
{
    public class Files
    {

        public static List<string> GetAllFiles(string directory, bool subdirectories, string whitelist = null, string blacklist = null)
        {
            List<string> files = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(directory))
                {
                    if (blacklist != null)
                    {
                        string ext = Path.GetExtension(f).Replace(".", "");
                        if (!blacklist.ToLower().Contains(ext.ToLower()))
                        {
                            files.Add(f);
                        }
                    }
                    else
                    {
                        files.Add(f);
                    }
                }
                if (subdirectories)
                {
                    foreach (string d in Directory.GetDirectories(directory))
                    {
                        files.AddRange(GetAllFiles(d, subdirectories, whitelist, blacklist));
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return files;
        }
    }
}
