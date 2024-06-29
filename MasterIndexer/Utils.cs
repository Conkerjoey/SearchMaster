using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterIndexer
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

    }
}
