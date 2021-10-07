using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using DocLib;

namespace SearchMaster.Engine
{
    public class LabelDensityResolver : IResolver
    {
        private List<string> indexedDocumentsPaths;
        private List<SearchResult> results;
        private Dictionary<Document, double[]> documentWeightsVectors;
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

        public LabelDensityResolver(List<string> indexedDocumentsPaths, bool multithreadingEnable)
        {
            this.indexedDocumentsPaths = indexedDocumentsPaths;
            this.results = new List<SearchResult>();
            this.documentWeightsVectors = new Dictionary<Document, double[]>();
            this.multithreadingEnable = multithreadingEnable;
        }

        public QueryResult SearchQuery(Query query)
        {
            if (query.Type == Query.QueryType.Text)
            {
                string[] vecQuery = query.Text.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                List<SearchResult> results = MatchQueryLabels(vecQuery);
                stopwatch.Stop();
                return new QueryResult(results, query, indexedDocumentsPaths.Count, stopwatch.Elapsed);
            }
            return null;
        }

        private List<SearchResult> MatchQueryLabels(string[] vectorizedLabels)
        {
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
                double relevance = 0;

                for (int l = 0; l < vectorizedLabels.Length; l++)
                {
                    List<WeightedLabel> weightedLabels = document.GetWeightedLabels().FindAll(x => x.GetText() == vectorizedLabels[l] || x.GetText().IndexOf(vectorizedLabels[l], StringComparison.InvariantCultureIgnoreCase) >= 0);
                    if (weightedLabels != null)
                    {
                        foreach (WeightedLabel wLabel in weightedLabels)
                            relevance += wLabel.GetTotalOccurence();
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
