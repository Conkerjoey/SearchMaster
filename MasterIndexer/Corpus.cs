﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DocLib;
using System.ComponentModel;

namespace MasterIndexer
{
    [Serializable]
    public class Corpus : INotifyPropertyChanged
    {
        private string name;
        private string location;
        private Filter filter;
        List<string> documentsPath = new List<string>();

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

        public Corpus(string name, Filter filter)
        {
            this.name = name;
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

        public void ListDocumentsAtLocation()
        {
            documentsPath.Clear();
            documentsPath.AddRange(Files.GetAllFiles(location, true, filter?.IgnoreList.ToList()));
        }

        public Corpus Duplicate()
        {
            return new Corpus(Name, Filter.Duplicate()) { Location = this.Location, DocumentsPath = this.DocumentsPath.ToList<string>() };
        }

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; this.OnPropertyChanged("Name"); }
        }

        public string Location
        {
            get { return location; }
            set { location = value; this.OnPropertyChanged("Location"); }
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
            set
            {
                documentsPath = value;
                this.OnPropertyChanged("DocumentsPath");
            }
        }

        public string FormattedDocumentCount
        {
            get
            {
                return documentsPath.Count + " documents";
            }
        }

        public Filter Filter
        {
            get { return filter; }
            set { filter = value; this.OnPropertyChanged("Filter"); }
        }

        #endregion
    }
}
