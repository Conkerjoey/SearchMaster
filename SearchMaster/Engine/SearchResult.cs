﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchMaster.Indexing;

namespace SearchMaster.Engine
{
    public class SearchResult
    {
        public enum RelevanceType
        {
            Percentage,
            Count
        }

        private Document document;
        private double relevance;
        private RelevanceType relevanceType;

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
            get; set;
        }

        public string FormattedRelevance
        {
            get
            {
                switch (relevanceType)
                {
                    case RelevanceType.Count:
                        return (int)relevance + " hit(s)";
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
                    return "From link in " + Document.Parent.DocumentSource.Path;
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
