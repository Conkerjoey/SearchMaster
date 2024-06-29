using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using DocLib;
using System.Diagnostics;

namespace MasterIndexer
{
    public class Indexer
    {
        private string version;
        private bool isMultithreaded;
        private string outputDirectory;
        private bool crawlUrl;

        public Indexer(string version, bool isMultithreaded, string outputDirectory, bool crawlUrl)
        {
            this.version = version;
            this.isMultithreaded = isMultithreaded;
            this.outputDirectory = outputDirectory;
            this.crawlUrl = crawlUrl;
        }

        public void ProcessCorpus(Corpus corpus, IProgress<double> progressRead, IProgress<double> progressCrawl, IProgress<double> progressCompute)
        {
            corpus.ListDocumentsAtLocation();
            List<Document> documents = new List<Document>();

            foreach (DocumentSource documentSource in corpus.DocumentsPath)
            {
                if (documentSource.PathType == DocumentSource.Type.Local)
                {
                    if (File.Exists(documentSource.Path))
                    {
                        Document doc = new Document(Path.GetFileName(documentSource.Path), documentSource.Path, documentSource);
                        documents.Add(doc);
                    }
                    else
                    {
                        Console.WriteLine("[WARNING] File '" + documentSource + "' in corpus '" + corpus.Name + "' has an invalid path or does not exists.");
                    }
                }
            }

            if (documents.Count <= 0)
            {
                Console.WriteLine("[WARNING] Empty corpus '" + corpus.Name + "'.");
                return;
            }

            int logicalProcessorCount = 1;
            if (isMultithreaded)
            {
                logicalProcessorCount = Environment.ProcessorCount;
            }
            int documentsPerProcessor = (int)Math.Ceiling((documents.Count + 0.0F) / logicalProcessorCount);
            List<List<Document>> documentListProcessors = new List<List<Document>>();

            for (int i = 0; i < documents.Count; i += documentsPerProcessor)
            {
                documentListProcessors.Add(documents.GetRange(i, Math.Min(documentsPerProcessor, documents.Count - i)));
            }

            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < logicalProcessorCount; i++)
            {
                int listIndex = i;
                if (listIndex >= documentListProcessors.Count)
                    break;
                Thread thread = new Thread(() => {
                    foreach (Document document in documentListProcessors[listIndex])
                    {
                        document.ReadContent();
                        progressRead?.Report(1);
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

            progressRead?.Report(-1);



            // Single Threaded by Design
            List<string> tempFiles = new List<string>();
            HttpService httpService = new HttpService();
            List<Document> documentsFromWeb = new List<Document>();
            foreach (Document document in documents)
            {
                foreach (string url in document.URLs)
                {
                    string tempFile = Utils.CreateTempFile();
                    DocumentType docType = httpService.Crawl(url, tempFile);
                    tempFiles.Add(tempFile);
                    DocumentSource documentSource = new DocumentSource(DocumentSource.Type.Web, url);
                    Document docFromWeb = new Document(Path.GetFileName(tempFile), tempFile, documentSource) { DocumentType = docType };
                    corpus.DocumentsPath.Add(documentSource);
                    docFromWeb.ReadContent();
                    documentsFromWeb.Add(docFromWeb);
                    documentListProcessors[0].Add(docFromWeb);
                    progressCrawl?.Report(1);
                }
            }
            documents.AddRange(documentsFromWeb);
            // Delete temporary files
            foreach (string tmpFile in tempFiles)
            {
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
            }
            progressCrawl?.Report(-1);
            // ----------------------------



            DirectoryInfo corpusDirectoryInfo = new DirectoryInfo(Path.Combine(new string[] { outputDirectory, corpus.Name }));
            corpusDirectoryInfo.Create();
            foreach (FileInfo file in corpusDirectoryInfo.EnumerateFiles())
            {
                file.Delete();
            }

            for (int i = 0; i < logicalProcessorCount; i++)
            {
                int listIndex = i;
                if (listIndex >= documentListProcessors.Count)
                    break;
                Thread thread = new Thread(() => corpus.ComputeCorpusDocumentsLabelWeights(documentListProcessors[listIndex], documents, corpusDirectoryInfo.FullName, progressCompute));
                threads.Add(thread);
                thread.Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            threads.Clear();
            progressCompute?.Report(-1);
        }
    }
}
