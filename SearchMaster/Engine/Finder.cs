using SearchMaster.Indexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SearchMaster.Engine
{
    public class Finder
    {
        private Query query;

        public Finder(Query query)
        {
            this.query = query;
            // TODO: Wildcard*
            // TODO: Search option: file: “filename“ etc
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
