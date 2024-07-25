using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchMaster.Indexing;

namespace SearchMaster.Engine
{
    public class SearchResult : INotifyPropertyChanged
    {
        public enum RelevanceType
        {
            Percentage,
            Count
        }

        private Document document;
        private double relevance;
        private RelevanceType relevanceType;
        private bool opened = false;

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

        public SearchResult(Document doc, RelevanceType relevanceType, double relevance)
        {
            this.document = doc;
            this.relevanceType = relevanceType;
            this.relevance = relevance;
        }

        #region BindableProperties

        public string Text
        {
            get
            {
                return document.Name;
            }
        }
        public double Relevance
        {
            get
            {
                return this.relevance;
            }
        }

        public string IconPath
        {
            get
            {
                return "../" + Tools.ResourcesManager.GetFileIconFromType(document.FileType);
            }
        }

        public bool Opened
        {
            get { return this.opened; }
            set { this.opened = value; this.OnPropertyChanged("Opened"); }
        }

        public string FormattedRelevance
        {
            get
            {
                switch (relevanceType)
                {
                    case RelevanceType.Count:
                        return (int)relevance + " " + Properties.lang.Hits;
                    case RelevanceType.Percentage:
                        return (relevance * 100.0D).ToString("N1") + " %";
                    default:
                        return (int)relevance + "";
                }
            }
        }

        public string FullPath
        {
            get
            {
                if (Document.Parent != null)
                {
                    return Properties.lang.FromLinkIn + " " + Document.Parent.DocumentSource.Path;
                }
                else
                {
                    return Document.DocumentSource.Path;
                }
            }
        }

        #endregion

        public Document Document
        {
            get
            {
                return this.document;
            }
        }

        public override string ToString()
        {
            return "Document: " + document.ToString() + " Relevance: " + relevance.ToString("N3");
        }
    }
}
