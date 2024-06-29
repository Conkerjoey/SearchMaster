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

namespace SearchMaster.Indexing
{
    public class Reader
    {
        public static string[] ReadLines(DocFile docFile)
        {
            switch (docFile.FileType)
            {
                case FileType.Html:
                case FileType.Json:
                case FileType.Css:
                case FileType.Javascript:
                case FileType.Cpp:
                case FileType.CSharp:
                case FileType.Python:
                case FileType.Matlab:
                case FileType.Text:
                    return File.ReadAllLines(docFile.FilePath);
                case FileType.Word:
                    WordprocessingDocument wordDocument = WordprocessingDocument.Open(docFile.FilePath, false);
                    Body body = wordDocument.MainDocumentPart.Document.Body;
                    wordDocument.Close();
                    return body.InnerText.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                case FileType.Excel:
                    SpreadsheetDocument sheetDocument = SpreadsheetDocument.Open(docFile.FilePath, false);
                    WorkbookPart workbookPart = sheetDocument.WorkbookPart;
                    SharedStringTablePart sstpart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
                    SharedStringTable sst = sstpart.SharedStringTable;

                    WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                    Worksheet sheet = worksheetPart.Worksheet;

                    var cells = sheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Cell>();
                    var rows = sheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Row>();

                    List<string> strings = new List<string>();

                    // One way: go through each cell in the sheet
                    foreach (DocumentFormat.OpenXml.Spreadsheet.Cell cell in cells)
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
                case FileType.PDF:
                    StringBuilder text = new StringBuilder();
                    if (File.Exists(docFile.FilePath))
                    {
                        PdfReader pdfReader = new PdfReader(docFile.FilePath);
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
            return new string[] { };
        }
    }
}
