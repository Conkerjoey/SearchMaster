using SearchMaster.Indexing;
using SearchMaster.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SearchMaster.Engine
{
    public class Finder
    {
        private string queryStr;
        private List<string> indexedDocumentsPaths;
        private bool multithreadingEnabled;
        private string fileWildcard = null;

        public Finder(string queryStr, List<string> indexedDocumentPaths, bool multithreadingEnabled)
        {
            int idxStart = queryStr.IndexOf("file:");
            if (idxStart != -1)
            {
                int idxEnd = queryStr.IndexOf(" ", idxStart);
                if (idxEnd != -1)
                {
                    fileWildcard = RegexUtil.WildCardToRegular(queryStr.Substring(idxStart, idxEnd - idxStart).Replace("file:", ""));
                }
                else
                {
                    idxEnd = idxStart + 6;
                    if (queryStr.Length > idxEnd)
                    {
                        fileWildcard = RegexUtil.WildCardToRegular(queryStr.Substring(idxStart, queryStr.Length - idxStart).Replace("file:", ""));
                    }
                }
            }
            this.multithreadingEnabled = multithreadingEnabled;
            this.indexedDocumentsPaths = indexedDocumentPaths;
        }

        public String QueryStr
        {
            get { return queryStr; }
        }

        public List<string> IndexedDocumentsPaths
        {
            get { return indexedDocumentsPaths; }
        }

        public bool MultithreadingEnabled
        {
            get { return multithreadingEnabled; }
        }

        public bool EvaluateQueryFilters(string filepath)
        {
            if (fileWildcard != null)
            {
                return Regex.IsMatch(filepath, fileWildcard);
            }
            return true;
        }

        // string[] myarr = new string[] { "s", "f", "s" };

        // int[] v = myarr.Select((b, i) => b == "s" ? i : -1).Where(i => i != -1).ToArray();

        public List<WeightedLabel> Match(List<WeightedLabel> labels, string queryFragment)
        {
            return labels.FindAll(x => x.GetText().ToLower().Contains(queryFragment.ToLower()));
        }

        public List<WeightedLabel> MatchEntirely(List<WeightedLabel> labels, string queryFragment)
        {
            return labels.FindAll(x => x.GetText() == queryFragment);
        }

        public List<WeightedLabel> MatchRegex(List<WeightedLabel> labels, Regex regex)
        {
            return labels.FindAll(x =>
            {
                return regex.Match(x.GetText()).Success;
            });
        }
    }
}
