using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using SearchMaster.Indexing;

namespace SearchMaster.Engine
{
    public class CosineSimilarityResolver : IResolver
    {
        private List<string> indexedDocumentsPaths;
        private Dictionary<Document, double[]> documentWeightsVectors;
        private bool multithreadingEnable;
        private Finder finder;

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

        public CosineSimilarityResolver(List<string> indexedDocumentsPaths, bool multithreadingEnable)
        {
            this.indexedDocumentsPaths = indexedDocumentsPaths;
            this.documentWeightsVectors = new Dictionary<Document, double[]>();
            this.multithreadingEnable = multithreadingEnable;
        }

        public QueryResult SearchQuery(Query query)
        {
            finder = new Finder(query);
            string[] vecQuery = query.Text.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<SearchResult> results = SearchByCosineSimilarity(vecQuery);
            stopwatch.Stop();
            return new QueryResult(results, query, indexedDocumentsPaths.Count, stopwatch.Elapsed);
        }

        private List<SearchResult> SearchByCosineSimilarity(string[] vectorizedLabels)
        {
            List<SearchResult> results = new List<SearchResult>();
            documentWeightsVectors.Clear();

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
                Thread thread = new Thread(() => BuildDocumentSublistWeightsVector(indexedDocumentsPathsProcessors[listIndex], vectorizedLabels));
                threads.Add(thread);
                thread.Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            threads.Clear();

            double[] queryWeights = Maths.Ones(vectorizedLabels.Length);

            foreach (KeyValuePair<Document, double[]> documentWeightsKeyPair in documentWeightsVectors)
            {
                double mag2 = Maths.Magnitude(queryWeights) * Maths.Magnitude(documentWeightsKeyPair.Value);
                double similarity = 0;
                if (mag2 != 0)
                {
                    similarity = Maths.Dot(queryWeights, documentWeightsKeyPair.Value) / mag2;
                }
                if (similarity != 0)
                {
                    results.Add(new SearchResult(documentWeightsKeyPair.Key, SearchResult.RelevanceType.Percentage, similarity));
                }
            }
            results = results.OrderByDescending(x => x.Relevance).ToList();
            return results;
        }

        private void BuildDocumentSublistWeightsVector(List<string> documentsPathsSublist, string[] vectorizedLabels)
        {
            for (int i = 0; i < documentsPathsSublist.Count; i++)
            {
                Document document = Document.Load(documentsPathsSublist[i]);
                for (int l = 0; l < vectorizedLabels.Length; l++)
                {
                    WeightedLabel weightedLabel = finder.Match(document.WeightedLabels, vectorizedLabels[l]);
                    if (weightedLabel != null)
                    {
                        if (documentWeightsVectors.ContainsKey(document))
                        {
                            documentWeightsVectors[document][l] = weightedLabel.GetWeight();
                        }
                        else
                        {
                            double[] tempArray = new double[vectorizedLabels.Length];
                            tempArray[l] = weightedLabel.GetWeight();
                            lock (documentWeightsVectors)
                            {
                                documentWeightsVectors.Add(document, tempArray);
                            }
                        }
                    }
                }
            }
        }
    }
}
