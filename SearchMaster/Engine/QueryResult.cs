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
        }

        public string GetQueryResultStatus()
        {
            return searchResults.Count + " result(s) (" + elapsedTime.TotalSeconds.ToString("N2") + " seconds) from " + numberOfDocumentAnalyzed + " documents.";
        }
    }
}
