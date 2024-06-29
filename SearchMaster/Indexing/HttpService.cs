using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SearchMaster.Indexing;

namespace SearchMaster.Indexing
{
    public class HttpService
    {
        public HttpService()
        {
            
        }

        public FileType Crawl(string sURL, string tempLocation)
        {
            try
            {
                WebRequest wrGETURL = WebRequest.Create(sURL);
                WebResponse webResponse = wrGETURL.GetResponse();
                Stream objStream = webResponse.GetResponseStream();
                FileType docType = DocFile.DetermineFileType(sURL);
                if (docType == FileType.Undefined)
                    docType = DocTypeFromContentType(webResponse.ContentType);
                var fileStream = File.Create(tempLocation);
                objStream.CopyTo(fileStream);
                fileStream.Close();
                return docType;
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] " + e.ToString());
                return FileType.Undefined;
            }
        }

        public static FileType DocTypeFromContentType(string contentType)
        {
            if (contentType.Contains("json"))
            {
                return FileType.Json;
            }
            else if (contentType.Contains("html"))
            {
                return FileType.Html;
            }
            else if (contentType.Contains("css"))
            {
                return FileType.Css;
            }
            return FileType.Undefined;
        }
    }
}
