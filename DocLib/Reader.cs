using System.IO;
using System.Collections.Generic;
using System;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Linq;

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
                        switch (document.GetDocumentType())
                        {
                            case DocumentType.Text:
                                return File.ReadAllLines(document.DocumentPath.Path);
                            case DocumentType.Word:
                                WordprocessingDocument wordDocument = WordprocessingDocument.Open(document.DocumentPath.Path, false);
                                Body body = wordDocument.MainDocumentPart.Document.Body;
                                wordDocument.Close();
                                return body.InnerText.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                            case DocumentType.Excel:
                                SpreadsheetDocument sheetDocument = SpreadsheetDocument.Open(document.DocumentPath.Path, false);
                                WorkbookPart workbookPart = sheetDocument.WorkbookPart;
                                SharedStringTablePart sstpart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
                                SharedStringTable sst = sstpart.SharedStringTable;

                                WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                                Worksheet sheet = worksheetPart.Worksheet;

                                var cells = sheet.Descendants<Cell>();
                                var rows = sheet.Descendants<Row>();

                                // Console.WriteLine("Row count = {0}", rows.LongCount());
                                // Console.WriteLine("Cell count = {0}", cells.LongCount());

                                List<string> strings = new List<string>();

                                // One way: go through each cell in the sheet
                                foreach (Cell cell in cells)
                                {
                                    if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
                                    {
                                        int ssid = int.Parse(cell.CellValue.Text);
                                        string str = sst.ChildElements[ssid].InnerText;
                                        //Console.WriteLine("Shared string {0}: {1}", ssid, str);
                                        strings.Add(str);
                                    }
                                    else if (cell.CellValue != null)
                                    {
                                        // Console.WriteLine("Cell contents: {0}", cell.CellValue.Text);
                                        strings.Add(cell.CellValue.Text);
                                    }
                                }

                                // // Or... via each row
                                // foreach (Row row in rows)
                                // {
                                //     foreach (Cell c in row.Elements<Cell>())
                                //     {
                                //         if ((c.DataType != null) && (c.DataType == CellValues.SharedString))
                                //         {
                                //             int ssid = int.Parse(c.CellValue.Text);
                                //             string str = sst.ChildElements[ssid].InnerText;
                                //             Console.WriteLine("Shared string {0}: {1}", ssid, str);
                                //         }
                                //         else if (c.CellValue != null)
                                //         {
                                //             Console.WriteLine("Cell contents: {0}", c.CellValue.Text);
                                //         }
                                //     }
                                // }
                                return strings.ToArray();
                            case DocumentType.PDF:
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
            // return File.ReadAllLines(document.DocumentPath.Path);
            return new string[] { };
        }
    }
}
