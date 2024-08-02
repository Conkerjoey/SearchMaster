using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchMaster.Indexing;

namespace SearchMaster.Tools
{
    public class ResourcesManager
    {
        public static string GetFileIconFromType(FileType type)
        {
            switch (type)
            {
                case FileType.Cpp:
                    return "Resources/icon_cpp.png";
                case FileType.CSharp:
                    return "Resources/icon_csharp.png";
                case FileType.Excel:
                    return "Resources/icon_excel.png";
                case FileType.Flow:
                    return "Resources/icon_flow.png";
                case FileType.Matlab:
                    return "Resources/icon_matlab.png";
                case FileType.Onenote:
                    return "Resources/icon_onenote.png";
                case FileType.PDF:
                    return "Resources/icon_pdf.png";
                case FileType.Python:
                    return "Resources/icon_python.png";
                case FileType.Html:
                    return "Resources/icon_web.png";
                case FileType.Word:
                    return "Resources/icon_word.png";
                case FileType.Javascript:
                    return "Resources/icon_js.png";
                case FileType.Json:
                case FileType.Css:
                case FileType.Text:
                case FileType.Undefined:
                case FileType.Java:
                    return "Resources/icon_default.png";
                default:
                    {
                        Console.WriteLine("[RESOURCE] Warning no icon exist for the document type " + type.ToString());
                        return "Resources/icon_default.png";
                    }
            }
        }
    }
}
