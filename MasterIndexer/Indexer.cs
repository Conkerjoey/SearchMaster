using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using DocLib;

namespace MasterIndexer
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

        public void ProcessCorpus(Corpus corpus, IProgress<double> progressRead, IProgress<double> progressCompute)
        {
            List<Document> documents = new List<Document>();

            foreach (string documentPath in corpus.DocumentsPath)
            {
                if (File.Exists(documentPath))
                {
                    Document doc = new Document(Path.GetFileName(documentPath), new DocumentPath(DocumentPath.Type.Local, documentPath));
                    documents.Add(doc);
                }
                else
                {
                    Console.WriteLine("[WARNING] File '" + documentPath + "' in corpus '" + corpus.Name + "' has an invalid path or does not exists.");
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
                Thread thread = new Thread(() => corpus.ScanDocumentLabels(documentListProcessors[listIndex], progressRead));
                threads.Add(thread);
                thread.Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            threads.Clear();

            progressRead?.Report(-1);

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
        }
    }
}
