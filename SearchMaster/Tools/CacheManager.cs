using SearchMaster.Indexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Tools
{
    public class CacheManager
    {
        public CacheManager()
        {

        }

        public string SrcPath
        {
            get; set;
        }

        public string DstPath
        {
            get; set;
        }

        public string CacheDirectory
        {
            get { return DstPath; }
        }

        public void Sync()
        {
            return;
            List<string> srcElements = Utils.ListDirectory(SrcPath, true, null, null);
            foreach (string srcElement in srcElements)
            {
                string dstElement = srcElement.Replace(SrcPath, DstPath);
                if (System.IO.File.Exists(dstElement))
                {
                    DateTime srcModification = System.IO.File.GetLastWriteTime(srcElement);
                    DateTime dstModification = System.IO.File.GetLastWriteTime(srcElement);
                    if (srcModification > dstModification)
                    {
                        System.IO.File.Delete(dstElement);
                        System.IO.File.Copy(srcElement, dstElement);
                    }
                }
                else
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dstElement));
                    System.IO.File.Copy(srcElement, dstElement);
                }
            }

            List<string> dstElements = Utils.ListDirectory(SrcPath, true, null, null);
            foreach (string dstElement in dstElements)
            {
                string srcElement = dstElement.Replace(DstPath, SrcPath);
                if (!System.IO.File.Exists(srcElement))
                {
                    System.IO.File.Delete(dstElement);
                }
            }
        }
    }
}
