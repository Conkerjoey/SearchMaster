﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using SearchMaster.Indexing;

namespace SearchMaster.Engine
{
    public class LabelDensityResolver : IResolver
    {
        private Finder finder;

        public LabelDensityResolver(Finder finder)
        {
            this.finder = finder;
        }

        public QueryResult SearchQuery(Query query)
        {
            string[] vecQuery = query.Text.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<SearchResult> results = MatchQueryLabels(vecQuery);
            stopwatch.Stop();
            return new QueryResult(results, query, finder.IndexedDocumentsPaths.Count, stopwatch.Elapsed);
        }

        private List<SearchResult> MatchQueryLabels(string[] vectorizedLabels)
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
                Thread thread = new Thread(() => BuildDocumentSublistWeightsVector(indexedDocumentsPathsProcessors[listIndex], vectorizedLabels, ref results));
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

        private void BuildDocumentSublistWeightsVector(List<string> documentsPathsSublist, string[] vectorizedLabels, ref List<SearchResult> results)
        {
            for (int i = 0; i < documentsPathsSublist.Count; i++)
            {
                Document document = Document.Load(documentsPathsSublist[i]);
                if (!finder.EvaluateQueryFilters(document.DocumentSource.Path))
                    continue;
                double relevance = 0;

                for (int l = 0; l < vectorizedLabels.Length; l++)
                {
                    List<WeightedLabel> weightedLabels = finder.Match(document.NGram.WeightedLabels, vectorizedLabels[l]);
                    foreach (WeightedLabel weightedLabel in weightedLabels)
                    {
                        relevance += weightedLabel.GetTotalOccurence();
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
