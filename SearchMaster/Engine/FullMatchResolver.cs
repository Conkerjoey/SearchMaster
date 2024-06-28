using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using DocLib;

namespace SearchMaster.Engine
{
    public class FullMatchResolver : IResolver
    {
        private List<string> indexedDocumentsPaths;
        private bool multithreadingEnable;

        public List<string> IndexedDocumentsPath
        {
            get
            {
                return indexedDocumentsPaths;
            }
            set
            {
                indexedDocumentsPaths = value;
            }
        }

        public FullMatchResolver(List<string> indexedDocumentsPaths, bool multithreadingEnable)
        {
            this.indexedDocumentsPaths = indexedDocumentsPaths;
            this.multithreadingEnable = multithreadingEnable;
        }

        public QueryResult SearchQuery(Query query)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<SearchResult> results = FindFullMatch(query.Text);
            stopwatch.Stop();
            return new QueryResult(results, query, indexedDocumentsPaths.Count, stopwatch.Elapsed);
        }

        private List<SearchResult> FindFullMatch(string text)
        {
            List<SearchResult> results = new List<SearchResult>();

            int logicalProcessorCount = 1;
            if (multithreadingEnable)
            {
                logicalProcessorCount = Environment.ProcessorCount;
            }
            int documentsPerProcessor = (int)Math.Ceiling((indexedDocumentsPaths.Count + 0.0F) / logicalProcessorCount);
            List<List<string>> indexedDocumentsPathsProcessors = new List<List<string>>();

            for (int i = 0; i < indexedDocumentsPaths.Count; i += documentsPerProcessor)
            {
                indexedDocumentsPathsProcessors.Add(indexedDocumentsPaths.GetRange(i, Math.Min(documentsPerProcessor, indexedDocumentsPaths.Count - i)));
            }
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < logicalProcessorCount; i++)
            {
                int listIndex = i;
                if (listIndex >= indexedDocumentsPathsProcessors.Count)
                    break;
                Thread thread = new Thread(() => FindInDocument(indexedDocumentsPathsProcessors[listIndex], text, ref results));
                threads.Add(thread);
                thread.Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            threads.Clear();

            results = results.OrderByDescending(x => x.Relevance).ToList();
            return results;
        }

        private void FindInDocument(List<string> documentsPathsSublist, string text, ref List<SearchResult> results)
        {
            for (int i = 0; i < documentsPathsSublist.Count; i++)
            {
                Document document = Document.Load(documentsPathsSublist[i]);
                double relevance = 0;
                string[] lines = document.GetLines();
                for (int l = 0; l < lines.Length; l++)
                {
                    if (lines[l].ToLower().Contains(text.ToLower()))
                    {
                        relevance++;
                    }
                }
                if (relevance > 0)
                {
                    results.Add(new SearchResult(document, SearchResult.RelevanceType.Count, relevance));
                }
            }
        }
    }
}
