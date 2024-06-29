using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Indexing
{
    public class Utils
    {
        public static string CreateTempFile()
        {
            string fileName = string.Empty;
            try
            {
                fileName = Path.GetTempFileName();
                FileInfo fileInfo = new FileInfo(fileName);
                fileInfo.Attributes = FileAttributes.Temporary;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] " + ex.Message);
            }
            return fileName;
        }

        public static List<string> ListDirectory(string directory, bool subdirectories, string whitelist = "", string blacklist = "")
        {
            if (whitelist == null)
                whitelist = "";
            if (blacklist == null)
                blacklist = "";
            whitelist = whitelist.Trim();
            whitelist = whitelist.Trim();
            List<string> files = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(directory))
                {
                    if (blacklist.Length > 0 && whitelist.Length <= 0)
                    {
                        string ext = Path.GetExtension(f).Replace(".", "");
                        if (!blacklist.ToLower().Contains(ext.ToLower()))
                        {
                            files.Add(f);
                        }
                    }
                    else if (whitelist.Length > 0 && blacklist.Length <= 0)
                    {
                        string ext = Path.GetExtension(f).Replace(".", "");
                        if (whitelist.ToLower().Contains(ext.ToLower()))
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
                        files.AddRange(ListDirectory(d, subdirectories, whitelist, blacklist));
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
