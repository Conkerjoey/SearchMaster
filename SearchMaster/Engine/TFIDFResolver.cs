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
    public class TFIDFResolver : IResolver
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

        public TFIDFResolver(List<string> indexedDocumentsPaths, bool multithreadingEnable)
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
            List<SearchResult> results = SearchByTFIDF(vecQuery);
            stopwatch.Stop();
            return new QueryResult(results, query, indexedDocumentsPaths.Count, stopwatch.Elapsed);
        }

        private List<SearchResult> SearchByTFIDF(string[] vectorizedLabels)
        {
            List<SearchResult> results = new List<SearchResult>();
            documentWeightsVectors.Clear();

            int logicalProcessorCount = 1;
            if (multithreadingEnable)
            {
                logicalProcessorCount = Environment.ProcessorCount - 1; // Let 1 core free to avoid to slow the OS
            }
            int documentsPerProcessor = (int)Math.Ceiling((indexedDocumentsPaths.Count + 0.0F) / logicalProcessorCount);

            List<List<string>> indexedDocumentsPathsProcessors = Tools.ResourcesManager.SplitToCores(indexedDocumentsPaths, logicalProcessorCount);
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < logicalProcessorCount; i++)
            {
                int listIndex = i;
                if (listIndex >= indexedDocumentsPathsProcessors.Count)
                    break;
                Thread thread = new Thread(() => BuildDocumentSublistTFVector(indexedDocumentsPathsProcessors[listIndex], vectorizedLabels));
                threads.Add(thread);
                thread.Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            threads.Clear();
            int[] doc_count = new int[vectorizedLabels.Length];
            double[,] mat = new double[documentWeightsVectors.Count, vectorizedLabels.Length];

            for (int i = 0; i < documentWeightsVectors.Count; i++)
            {
                for (int j = 0; j < documentWeightsVectors.ElementAt(i).Value.Length; j++)
                    mat[i, j] = documentWeightsVectors.ElementAt(i).Value[j];
                doc_count = Maths.Add(doc_count, Maths.NonZero(documentWeightsVectors.ElementAt(i).Value));
            }

            double[] docCounts = Maths.Times(Maths.Ones(vectorizedLabels.Length), indexedDocumentsPaths.Count);
            double[] idf = Maths.Apply(Math.Log, Maths.Divide(docCounts, doc_count));
            double[,] tfidf = Maths.Times(mat, idf, 1);
            double[] docMag = Maths.Apply(Maths.Magnitude, tfidf, 1);
            double maxVal = Maths.Max(docMag);
            docMag = Maths.Divide(docMag, maxVal);

            for (int i = 0; i < documentWeightsVectors.Count; i++)
            {
                results.Add(new SearchResult(documentWeightsVectors.ElementAt(i).Key, SearchResult.RelevanceType.Percentage, docMag[i]));
            }
            results = results.OrderByDescending(x => x.Relevance).ToList();
            return results;
        }

        private void BuildDocumentSublistTFVector(List<string> documentsPathsSublist, string[] vectorizedLabels)
        {
            for (int i = 0; i < documentsPathsSublist.Count; i++)
            {
                Document document = Document.Load(documentsPathsSublist[i]);

                for (int l = 0; l < vectorizedLabels.Length; l++)
                {
                    List<WeightedLabel> weightedLabels = finder.Match(document.NGram.WeightedLabels, vectorizedLabels[l]);
                    foreach (WeightedLabel weightedLabel in weightedLabels)
                    {
                        double tf = weightedLabel.GetWeight();
                        if (documentWeightsVectors.ContainsKey(document))
                        {
                            documentWeightsVectors[document][l] = tf;
                        }
                        else
                        {
                            double[] tempArray = new double[vectorizedLabels.Length];
                            tempArray[l] = tf;
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
