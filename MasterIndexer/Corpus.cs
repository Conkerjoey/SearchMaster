using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DocLib;

namespace MasterIndexer
{
    [Serializable]
    public class Corpus
    {
        private string name;
        private List<string> locations;
        private Filter filter;
        List<string> documentsPath = new List<string>();

        public Corpus(string name, Filter filter)
        {
            this.name = name;
            this.locations = new List<string>();
            this.filter = filter;
        }

        public void ScanDocumentLabels(List<Document> documentsSublist, IProgress<double> progress)
        {
            foreach (Document document in documentsSublist)
            {
                document.Scan();
                progress?.Report(1);
            }
        }

        public void ComputeCorpusDocumentsLabelWeights(List<Document> documentsSublist, List<Document> corpusDocuments, string corpusOutputDirectory, IProgress<double> progress)
        {
            for (int i = 0; i < documentsSublist.Count; i++)
            {
                for (int l = 0; l < documentsSublist[i].GetWeightedLabels().Count; l++)
                {
                    int labelDocOccurence = 1;
                    WeightedLabel weightedLabel = documentsSublist[i].GetWeightedLabels()[l];

                    for (int j = 0; j < corpusDocuments.Count; j++)
                    {
                        if (!documentsSublist[i].Equals(corpusDocuments[j]))
                        {
                            if (corpusDocuments[j].IsLabelPresent(weightedLabel))
                            {
                                labelDocOccurence++;
                            }
                        }
                    }
                    // Compute TF-IDF
                    double weight = (weightedLabel.GetTotalOccurence() + 0.0D) / documentsSublist[i].GetTotalWords() * Math.Log(corpusDocuments.Count / (labelDocOccurence + 0.0D));
                    // End of Compute
                    weightedLabel.SetWeight(weight);
                }
                documentsSublist[i].Save(corpusOutputDirectory);
                progress?.Report(1);
            }
        }

        public void AddLocation(string path)
        {
            locations.Add(path);
            documentsPath.AddRange(Files.GetAllFiles(path, true, filter?.IgnoreList.ToList()));
        }

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public List<string> Locations
        {
            get { return locations; }
        }
        
        public string ContentLocation
        {
            get
            {
                if (locations.Count <= 1)
                {
                    return locations[0];
                }
                else
                {
                    return locations.Count + " different locations.";
                }
            }
        }

        public int DocumentCount
        {
            get
            {
                return documentsPath.Count;
            }
        }

        public List<string> DocumentsPath
        {
            get
            {
                return documentsPath;
            }
        }

        public string FormattedDocumentCount
        {
            get
            {
                return documentsPath.Count + " documents";
            }
        }

        #endregion
    }
}
