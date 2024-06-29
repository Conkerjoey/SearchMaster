﻿using System;
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
        Word, // .docx, .doc
        Excel, // .xlsx, .xls
        CSharp, // .cs
        Cpp, // .cpp, .c
        Flow, // .flow
        Onenote, // .one
        Python, // .py, .pyc, .pyi, .pyd, .pyo, .pyw, .pyz
        Matlab, // .m
        Json, // .json
        Html, // .html
        Css, // .css
        Javascript, // .js
    }
}
