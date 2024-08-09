using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using SearchMaster.Indexing;

namespace SearchMaster.Engine
{
    public class FullMatchResolver : IResolver
    {
        private Finder finder;

        public FullMatchResolver(Finder finder)
        {
            this.finder = finder;
        }

        public QueryResult SearchQuery(Query query)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<SearchResult> results = FindFullMatch(query.Text);
            stopwatch.Stop();
            return new QueryResult(results, query, finder.IndexedDocumentsPaths.Count, stopwatch.Elapsed);
        }

        private List<SearchResult> FindFullMatch(string text)
        {
            List<SearchResult> results = new List<SearchResult>();

            int logicalProcessorCount = 1;
            if (finder.MultithreadingEnabled)
            {
                logicalProcessorCount = Environment.ProcessorCount - 1; // Let 1 core free to avoid to slow the OS
            }
            int documentsPerProcessor = (int)Math.Ceiling((finder.IndexedDocumentsPaths.Count + 0.0F) / logicalProcessorCount);

            List<List<string>> indexedDocumentsPathsProcessors = Tools.ResourcesManager.SplitToCores(finder.IndexedDocumentsPaths, logicalProcessorCount);
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
            string[] vecQuery = text.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < documentsPathsSublist.Count; i++)
            {
                Document document = Document.Load(documentsPathsSublist[i]);
                if (!finder.EvaluateQueryFilters(document.DocumentSource.Path))
                    continue;
                double relevance = 0;
                for (int l = 0; l < vecQuery.Length; l++)
                {
                    List<WeightedLabel> weightedLabels = finder.MatchEntirely(document.NGram.WeightedLabels, vecQuery[l]);
                    foreach (WeightedLabel weightedLabel in weightedLabels)
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
