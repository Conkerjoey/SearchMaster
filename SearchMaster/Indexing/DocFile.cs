using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Indexing
{
    public class DocFile
    {
        public string FilePath { get; set; }
        public FileType FileType { get; set; }
        public List<string> urls = new List<string>();

        public DocFile()
        {

        }

        public static FileType DetermineFileType(string filepath)
        {
            if (filepath.EndsWith(".txt") ||
                filepath.EndsWith(".css") ||
                filepath.EndsWith(".html") ||
                filepath.EndsWith(".js"))
            {
                return FileType.Text;
            }
            else if (filepath.EndsWith(".pdf"))
            {
                return FileType.PDF;
            }
            else if (filepath.EndsWith(".flow"))
            {
                return FileType.Flow;
            }
            else if (filepath.EndsWith(".cs"))
            {
                return FileType.CSharp;
            }
            else if (filepath.EndsWith(".cpp") ||
                      filepath.EndsWith(".c"))
            {
                return FileType.Cpp;
            }
            else if (filepath.EndsWith(".doc") ||
                     filepath.EndsWith(".docx"))
            {
                return FileType.Word;
            }
            else if (filepath.EndsWith(".xls") ||
                     filepath.EndsWith(".xlsx"))
            {
                return FileType.Excel;
            }
            else if (filepath.EndsWith(".one"))
            {
                return FileType.Onenote;
            }
            else if (filepath.EndsWith(".py") || filepath.EndsWith(".pyc") || filepath.EndsWith(".pyi") || filepath.EndsWith(".pyd") || filepath.EndsWith(".pyo") || filepath.EndsWith(".pyw") || filepath.EndsWith(".pyz"))
            {
                return FileType.Python;
            }
            else if (filepath.EndsWith(".m"))
            {
                return FileType.Matlab;
            }
            else if (filepath.EndsWith(".html"))
            {
                return FileType.Html;
            }
            else if (filepath.EndsWith(".css"))
            {
                return FileType.Css;
            }
            else if (filepath.EndsWith(".js"))
            {
                return FileType.Javascript;
            }
            else if (filepath.EndsWith(".json"))
            {
                return FileType.Json;
            }
            return FileType.Undefined;
        }
    }
}
