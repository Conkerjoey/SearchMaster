﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DocLib;

namespace MasterIndexer
{
    public class HttpService
    {
        public HttpService()
        {
            
        }

        public DocumentType Crawl(string sURL, string tempLocation)
        {
            try
            {
                WebRequest wrGETURL = WebRequest.Create(sURL);
                WebResponse webResponse = wrGETURL.GetResponse();
                Stream objStream = webResponse.GetResponseStream();
                DocumentType docType = Document.DetermineDocType(sURL);
                if (docType == DocumentType.Undefined)
                    docType = DocTypeFromContentType(webResponse.ContentType);
                var fileStream = File.Create(tempLocation);
                objStream.CopyTo(fileStream);
                fileStream.Close();
                return docType;
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] " + e.ToString());
                return DocumentType.Undefined;
            }
        }

        public static DocumentType DocTypeFromContentType(string contentType)
        {
            if (contentType.Contains("json"))
            {
                return DocumentType.Json;
            }
            else if (contentType.Contains("html"))
            {
                return DocumentType.Html;
            }
            else if (contentType.Contains("css"))
            {
                return DocumentType.Css;
            }
            return DocumentType.Undefined;
        }
    }
}
