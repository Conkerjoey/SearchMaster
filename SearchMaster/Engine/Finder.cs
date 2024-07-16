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

        public WeightedLabel Match(List<WeightedLabel> labels, string queryFragment)
        {
            return labels.Find(x => x.GetText().ToLower().Contains(queryFragment.ToLower()));
        }

        public WeightedLabel MatchEntirely(List<WeightedLabel> labels, string queryFragment)
        {
            return labels.Find(x => x.GetText() == queryFragment);
        }

        public WeightedLabel MatchRegex(List<WeightedLabel> labels, Regex regex)
        {
            return labels.Find(x =>
            {
                return regex.Match(x.GetText()).Success;
            });
        }
    }
}
