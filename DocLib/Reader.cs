using System.IO;
using System.Collections.Generic;
using System;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace DocLib
{
    public class Reader
    {

        public static string[] ReadLines(Document document)
        {
            switch (document.DocumentPath.PathType)
            {
                case DocumentPath.Type.Local:
                    {
                        if (document.GetDocumentType() == DocumentType.Text)
                        {
                            return File.ReadAllLines(document.DocumentPath.Path);
                        }
                        else if (document.GetDocumentType() == DocumentType.Word)
                        {
                            WordprocessingDocument wordDocument = WordprocessingDocument.Open(document.DocumentPath.Path, false);
                            Body body = wordDocument.MainDocumentPart.Document.Body;
                            return body.InnerText.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        }
                        else if (document.GetDocumentType() == DocumentType.PDF)
                        {
                            StringBuilder text = new StringBuilder();

                            if (File.Exists(document.DocumentPath.Path))
                            {
                                PdfReader pdfReader = new PdfReader(document.DocumentPath.Path);

                                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                                {
                                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                                    string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                                    currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                                    text.Append(currentText);
                                }
                                pdfReader.Close();
                            }
                            return text.ToString().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        }
                    }
                    break;
                case DocumentPath.Type.Network:
                    // TODO: Check if network drive is mapped. If so, open the file from the network location.
                    return new string[] { };
                case DocumentPath.Type.Subversion:
                    // TODO: Download file on local temp folder and open local temp copy.
                    return new string[] { };
            }
            return File.ReadAllLines(document.DocumentPath.Path);
        }
    }
}
