using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocLib
{
    public enum DocumentType
    {
        Undefined,
        Text, // .txt
        PDF, // .pdf
        Word, // .docx
        CSharp, // .cs
        Cpp, // .cpp, .c
        Flow, // .flow
    }
}
