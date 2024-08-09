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
    public class RegexResolver : IResolver
    {
        private Finder finder;

        public RegexResolver(Finder finder)
        {
            this.finder = finder;
        }

        public QueryResult SearchQuery(Query query)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Regex regex = new Regex(query.Text, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            List<SearchResult> results = FindRegexMatch(regex);
            stopwatch.Stop();
            return new QueryResult(results, query, finder.IndexedDocumentsPaths.Count, stopwatch.Elapsed);
        }

        private List<SearchResult> FindRegexMatch(Regex regex)
        {
            List<SearchResult> results = new List<SearchResult>();

            int logicalProcessorCount = 1;
            if (finder.MultithreadingEnabled)
            {
                logicalProcessorCount = Environment.ProcessorCount - 1; // Let 1 core free to avoid to slow the OS
            }
            List<List<string>> indexedDocumentsPathsProcessors = Tools.ResourcesManager.SplitToCores(finder.IndexedDocumentsPaths, logicalProcessorCount);

            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < logicalProcessorCount; i++)
            {
                int listIndex = i;
                if (listIndex >= indexedDocumentsPathsProcessors.Count)
                    break;
                Thread thread = new Thread(() => FindRegexInDocument(indexedDocumentsPathsProcessors[listIndex], regex, ref results));
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

        private void FindRegexInDocument(List<string> documentsPathsSublist, Regex regex, ref List<SearchResult> results)
        {
            for (int i = 0; i < documentsPathsSublist.Count; i++)
            {
                Document document = Document.Load(documentsPathsSublist[i]);
                if (!finder.EvaluateQueryFilters(document.DocumentSource.Path))
                    continue;
                double relevance = 0;

                List<WeightedLabel> weightedLabels = finder.MatchRegex(document.NGram.WeightedLabels, regex);
                foreach (WeightedLabel weightedLabel in weightedLabels)
                {
                    relevance += weightedLabel.GetTotalOccurence();
                }
                if (relevance > 0)
                {
                    results.Add(new SearchResult(document, SearchResult.RelevanceType.Count, relevance));
                }
            }
        }
    }
}
