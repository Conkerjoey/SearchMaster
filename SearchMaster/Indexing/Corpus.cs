using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SearchMaster.Indexing;
using System.ComponentModel;

namespace SearchMaster.Indexing
{
    [Serializable]
    public class Corpus : INotifyPropertyChanged
    {
        private string originalName;
        private string name;
        private string location;
        private string whitelist;
        private string blacklist;

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Corpus()
        {

        }

        public void ComputeCorpusDocumentsLabelWeights(List<Document> documentsSublist, List<Document> corpusDocuments, string corpusOutputDirectory)
        {
            for (int i = 0; i < documentsSublist.Count; i++)
            {
                for (int l = 0; l < documentsSublist[i].NGram.WeightedLabels.Count; l++)
                {
                    int labelDocOccurence = 1;
                    WeightedLabel weightedLabel = documentsSublist[i].NGram.WeightedLabels[l];

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
                    double weight = (weightedLabel.GetTotalOccurence() + 0.0D) / documentsSublist[i].NGram.WordCount * Math.Log(corpusDocuments.Count / (labelDocOccurence + 0.0D));
                    // End of Compute
                    weightedLabel.SetWeight(weight);
                }
                documentsSublist[i].Save(corpusOutputDirectory);
            }
        }

        public Corpus Duplicate()
        {
            return new Corpus() { Name = this.Name, Whitelist = this.Whitelist, blacklist = this.Blacklist, Location = this.Location, };
        }

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; if (originalName == null) originalName = value; this.OnPropertyChanged("Name"); }
        }

        public string Location
        {
            get { return location; }
            set { location = value; this.OnPropertyChanged("Location"); }
        }

        public string Whitelist
        {
            get { return whitelist; }
            set { whitelist = value; this.OnPropertyChanged("Whitelist"); }
        }

        public string Blacklist
        {
            get { return blacklist; }
            set { blacklist = value; this.OnPropertyChanged("Blacklist"); }
        }

        public int DocumentCount
        {
            get;set;
        }

        public string FormattedDocumentCount
        {
            get
            {
                return DocumentCount + " files";
            }
        }

        public bool CrawlUrlEnabled
        {
            get; set;
        }
        #endregion
    }
}
