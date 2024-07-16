using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Engine
{
    public class QueryResult
    {
        public List<SearchResult> searchResults;
        public Query query;
        public int numberOfDocumentAnalyzed;
        public TimeSpan elapsedTime;

        public QueryResult(List<SearchResult> searchResults, Query query, int numberOfDocumentAnalyzed, TimeSpan elapsedTime)
        {
            this.searchResults = searchResults;
            this.query = query;
            this.numberOfDocumentAnalyzed = numberOfDocumentAnalyzed;
            this.elapsedTime = elapsedTime;

            // Remove duplicate results on same document
            this.searchResults = this.searchResults.GroupBy(x => x.FullPath).Select(x => x.First()).ToList();
        }

        public string GetQueryResultStatus()
        {
            return searchResults.Count + " " + Properties.lang.Results.ToLower() + " (" + elapsedTime.TotalSeconds.ToString("N2") + " " + Properties.lang.Seconds.ToLower() + ") " + Properties.lang.In.ToLower() + " " + numberOfDocumentAnalyzed + " " + Properties.lang.Documents.ToLower() + ".";
        }
    }
}
