using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Tools
{
    public class ResourcesManager
    {
        public static string GetDocumentIconPathFromType(DocLib.DocumentType type)
        {
            switch (type)
            {
                case DocLib.DocumentType.PDF:
                    return "Resources/icon_pdf.png";
                case DocLib.DocumentType.CSharp:
                    return "Resources/icon_csharp.png";
                case DocLib.DocumentType.Cpp:
                    return "Resources/icon_cpp.png";
                case DocLib.DocumentType.Flow:
                    return "Resources/icon_flow.png";
                case DocLib.DocumentType.Word:
                    return "Resources/icon_word.png";
                case DocLib.DocumentType.Excel:
                    return "Resources/icon_excel.png";
                case DocLib.DocumentType.Onenote:
                    return "Resources/icon_onenote.png";
                case DocLib.DocumentType.Text:
                case DocLib.DocumentType.Undefined:
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
