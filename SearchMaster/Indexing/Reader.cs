using System.IO;
using System.Collections.Generic;
using System;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Presentation;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Linq;
using DocumentFormat.OpenXml.ExtendedProperties;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using Microsoft.Office.Interop.OneNote;
using System.Xml;
using System.Runtime.InteropServices;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml;
using NUglify;

namespace SearchMaster.Indexing
{
    public class Reader
    {
        public static string[] ReadLines(DocFile docFile)
        {
            switch (docFile.FileType)
            {
                case FileType.Html:
                    {
                        string content = File.ReadAllText(docFile.FilePath);
                        UglifyResult result = Uglify.HtmlToText(content);
                        return result.Code.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    }
                case FileType.Flow:
                case FileType.Json:
                case FileType.Css:
                case FileType.Javascript:
                case FileType.Java:
                case FileType.Cpp:
                case FileType.CSharp:
                case FileType.Python:
                case FileType.Matlab:
                case FileType.Text:
                    return File.ReadAllLines(docFile.FilePath);
                case FileType.Word:
                    {
                        try
                        {
                            WordprocessingDocument wordDocument = WordprocessingDocument.Open(docFile.FilePath, false);
                            Body body = wordDocument.MainDocumentPart.Document.Body;
                            return body.InnerText.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        }
                        catch (FileFormatException e)
                        {
                            Console.WriteLine("[ERROR] File is corrupted: " + docFile.FilePath);
                            return new string[] { };
                        }
                    }
                case FileType.Excel:
                    {
                        List<string> strings = new List<string>();
                        try
                        {
                            SpreadsheetDocument sheetDocument = SpreadsheetDocument.Open(docFile.FilePath, false);
                            WorkbookPart workbookPart = sheetDocument.WorkbookPart;
                            SharedStringTablePart sstpart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
                            SharedStringTable sst = sstpart.SharedStringTable;

                            WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                            Worksheet sheet = worksheetPart.Worksheet;

                            var cells = sheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Cell>();
                            var rows = sheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Row>();


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
                        }
                        catch (FileFormatException e)
                        {
                            Console.WriteLine("[ERROR] " + docFile.FilePath + " Message: " + e.Message);
                        }
                        catch (InvalidOperationException e)
                        {
                            Console.WriteLine("[ERROR] " + docFile.FilePath + " Message: " + e.Message);
                        }
                        return strings.ToArray();
                    }
                case FileType.Onenote:
                    {
                        List<string> content = new List<string>();
                        Microsoft.Office.Interop.OneNote.Application oneNoteApp = null;
                        try
                        {
                            oneNoteApp = new Microsoft.Office.Interop.OneNote.Application();
                            string notebookId;
                            oneNoteApp.OpenHierarchy(docFile.FilePath, null, out notebookId, CreateFileType.cftNone);
                            string notebookXml;
                            // Get the XML content of the OneNote file
                            oneNoteApp.GetHierarchy(null, HierarchyScope.hsPages, out notebookXml);
                            
                            // Parse the XML content to get the text content
                            XmlDocument notebookDoc = new XmlDocument();
                            notebookDoc.LoadXml(notebookXml);
                            XmlNodeList test = notebookDoc.ChildNodes;
                            // Navigate through the XML to find the desired content.
                            XmlNodeList notebookNodes = notebookDoc.SelectNodes("//one:Notebook", GetNamespaceManager(notebookDoc));
                            foreach (XmlNode notebookNode in notebookNodes)
                            {
                                // Console.WriteLine("Notebook: " + notebookNode.Attributes["name"].Value);
                                XmlNodeList sectionNodes = notebookNode.SelectNodes("one:Section", GetNamespaceManager(notebookDoc));
                                foreach (XmlNode sectionNode in sectionNodes)
                                {
                                    // Console.WriteLine("  Section: " + sectionNode.Attributes["name"].Value);
                                    if (string.Equals(System.IO.Path.GetFullPath(docFile.FilePath).TrimEnd('\\'), System.IO.Path.GetFullPath(sectionNode.Attributes["path"].Value).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                                    {
                                        XmlNodeList pageNodes = sectionNode.SelectNodes("one:Page", GetNamespaceManager(notebookDoc));
                                        foreach (XmlNode pageNode in pageNodes)
                                        {
                                            // Console.WriteLine("    Page: " + pageNode.Attributes["name"].Value);

                                            // Get the content of the page.
                                            string pageId = pageNode.Attributes["ID"].Value;
                                            string pageContent;
                                            oneNoteApp.GetPageContent(pageId, out pageContent, PageInfo.piAll);

                                            XmlDocument pageDoc = new XmlDocument();
                                            pageDoc.LoadXml(pageContent);

                                            // Console.WriteLine("      Content: " + pageDoc.DocumentElement.InnerText);
                                            content.Add(pageDoc.DocumentElement.InnerText);
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
                            // Release COM objects and force garbage collection.
                            if (oneNoteApp != null)
                            {
                                Marshal.ReleaseComObject(oneNoteApp);
                            }
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                        return content.ToArray();
                    }
                case FileType.PowerPoint:
                    {
                        try
                        {
                            List<string> content = new List<string>();
                            using (PresentationDocument ppt = PresentationDocument.Open(docFile.FilePath, false))
                            {

                                PresentationPart presentationPart = ppt.PresentationPart;
                                // Get the slide count from the SlideParts.
                                if (presentationPart != null)
                                {
                                    int slidesCount = presentationPart.SlideParts.Count();

                                    for (int i = 0; i < slidesCount; i++)
                                    {
                                        OpenXmlElementList slideIds = presentationPart?.Presentation?.SlideIdList?.ChildElements ?? default;

                                        if (presentationPart == null || slideIds.Count == 0)
                                        {
                                            return content.ToArray();
                                        }

                                        string relId = ((SlideId)slideIds[i]).RelationshipId;

                                        if (relId != null)
                                        {
                                            // Get the slide part from the relationship ID.
                                            SlidePart slide = (SlidePart)presentationPart.GetPartById(relId);

                                            // Get the inner text of the slide:
                                            IEnumerable<DocumentFormat.OpenXml.Drawing.Text> texts = slide.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>();
                                            foreach (DocumentFormat.OpenXml.Drawing.Text txt in texts)
                                            {
                                                content.Add(txt.Text);
                                            }
                                        }
                                    }
                                }
                            }
                            return content.ToArray();
                        }
                        catch (FileFormatException e)
                        {
                            Console.WriteLine("[ERROR] " + docFile.FilePath + " Message: " + e.Message);
                            return new string[] { };
                        }
                    }
                case FileType.PDF:
                    StringBuilder text = new StringBuilder();
                    if (File.Exists(docFile.FilePath))
                    {
                        try
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
                        catch (iTextSharp.text.exceptions.InvalidPdfException e)
                        {
                            Console.WriteLine("[ERROR] " + docFile.FilePath + " Message: " + e.Message);
                        }
                    }
                    return text.ToString().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            return new string[] { };
        }

        // Helper method to create an XmlNamespaceManager for the given XmlDocument.
        private static XmlNamespaceManager GetNamespaceManager(XmlDocument xmlDoc)
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("one", "http://schemas.microsoft.com/office/onenote/2013/onenote");
            return namespaceManager;
        }
    }
}
