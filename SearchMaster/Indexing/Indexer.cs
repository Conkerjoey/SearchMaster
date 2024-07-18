using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using SearchMaster.Indexing;
using System.Diagnostics;
using DocumentFormat.OpenXml.Office2019.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Security.Policy;
using System.Runtime.InteropServices;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing.Diagrams;

namespace SearchMaster.Indexing
{
    public class Indexer
    {
        private string version;
        private bool isMultithreaded;
        private string outputDirectory;

        public Indexer(string version, bool isMultithreaded, string outputDirectory)
        {
            this.version = version;
            this.isMultithreaded = isMultithreaded;
            this.outputDirectory = outputDirectory;
        }

        public void ProcessCorpus(Corpus corpus)
        {
            List<string> filepaths = Utils.ListDirectory(corpus.Location, true, corpus.Whitelist, corpus.Blacklist);

            // Multithreading stuff
            int logicalProcessorCount = 1;
            if (isMultithreaded)
            {
                logicalProcessorCount = Environment.ProcessorCount;
            }
            int filesPerProcessor = (int)Math.Ceiling((filepaths.Count + 0.0F) / logicalProcessorCount);
            List<List<string>> filepathSublists = new List<List<string>>();

            for (int i = 0; i < filepaths.Count; i += filesPerProcessor)
            {
                filepathSublists.Add(filepaths.GetRange(i, Math.Min(filesPerProcessor, filepaths.Count - i)));
            }
            // ------------------------------------------

            List<Document> documents = new List<Document>();

            List<string> tempFiles = new List<string>();
            HttpService httpService = new HttpService();

            Dictionary<Document, List<string>> docURLs = new Dictionary<Document, List<string>>();

            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < Math.Min(logicalProcessorCount, filepathSublists.Count); i++)
            {
                int listIndex = i;
                Thread thread = new Thread(() => {
                    foreach (string filepath in filepathSublists[listIndex])
                    {
                        if (File.Exists(filepath))
                        {
                            DocFile file = new DocFile() { FilePath = filepath, FileType = DocFile.DetermineFileType(filepath) };
                            string[] lines = Reader.ReadLines(file);
                            Parser parser = new Parser(lines);
                            NGram ngram = parser.GetNGram();
                            List<string> urls = parser.GetURLs();

                            Document doc = new Document() { Name = Path.GetFileName(filepath), FileType = file.FileType, NGram = ngram, DocumentSource = new DocumentSource(DocumentSource.Type.Local, filepath) };
                            documents.Add(doc);

                            docURLs[doc] = urls;
                        }
                        else
                        {
                            Console.WriteLine("[WARNING] File '" + filepath + "' in corpus '" + corpus.Name + "' has an invalid path or does not exists.");
                        }
                    }
                });
                threads.Add(thread);
                thread.Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            threads.Clear();

            if (corpus.CrawlUrlEnabled)
            {
                List<string> processedURLs = new List<string>();
                foreach (var element in docURLs)
                {
                    Document parentDoc = element.Key;
                    foreach (string url in element.Value)
                    {
                        if (processedURLs.Contains(url))
                            continue;
                        string tempFile = Utils.CreateTempFile();
                        FileType docType = httpService.Crawl(url, tempFile);
                        tempFiles.Add(tempFile);

                        DocFile webfile = new DocFile() { FilePath = tempFile, FileType = docType };
                        string[] weblines = Reader.ReadLines(webfile);
                        Parser webparser = new Parser(weblines);
                        NGram ngram = webparser.GetNGram();
                        
                        processedURLs.Add(url);
                        DocumentSource documentSource = new DocumentSource(DocumentSource.Type.Web, url);
                        Document docFromWeb = new Document() { Name = url, FileType = webfile.FileType, NGram = ngram, DocumentSource = new DocumentSource(DocumentSource.Type.Web, url), Parent = parentDoc };
                        documents.Add(docFromWeb);
                    }
                }
                // Delete temporary files
                foreach (string tmpFile in tempFiles)
                {
                    if (File.Exists(tmpFile))
                    {
                        File.Delete(tmpFile);
                    }
                }
                // ----------------------------
            }

            if (documents.Count <= 0)
            {
                Console.WriteLine("[WARNING] Empty corpus '" + corpus.Name + "'.");
                return;
            }

            // Multithreading stuff
            int documentsPerProcessor = (int)Math.Ceiling((documents.Count + 0.0F) / logicalProcessorCount);
            List<List<Document>> documentSublists = new List<List<Document>>();

            for (int i = 0; i < documents.Count; i += filesPerProcessor)
            {
                documentSublists.Add(documents.GetRange(i, Math.Min(filesPerProcessor, documents.Count - i)));
            }
            // ------------------------------------------
            DirectoryInfo corpusDirectoryInfo = new DirectoryInfo(Path.Combine(new string[] { outputDirectory, corpus.Name }));
            corpusDirectoryInfo.Create();
            foreach (FileInfo file in corpusDirectoryInfo.EnumerateFiles())
            {
                file.Delete();
            }
            for (int i = 0; i < Math.Min(logicalProcessorCount, documentSublists.Count); i++)
            {
                int listIndex = i;
                Thread thread = new Thread(() => corpus.ComputeCorpusDocumentsLabelWeights(documentSublists[listIndex], documents, corpusDirectoryInfo.FullName));
                threads.Add(thread);
                thread.Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            threads.Clear();

            corpus.DocumentCount = documents.Count;
        }
    }
}
