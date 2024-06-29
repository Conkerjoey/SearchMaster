using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Engine
{
    public class OkapiBM25Resolver : IResolver
    {
        private List<string> indexedDocumentsPaths;
        private bool multithreadingEnable;

        public List<string> IndexedDocumentsPath
        {
            get
            {
                return indexedDocumentsPaths;
            }
            set
            {
                indexedDocumentsPaths = value;
            }
        }

        public OkapiBM25Resolver(List<string> indexedDocumentsPaths, bool multithreadingEnable)
        {
            this.indexedDocumentsPaths = indexedDocumentsPaths;
            this.multithreadingEnable = multithreadingEnable;
        }

        public QueryResult SearchQuery(Query query)
        {
            throw new NotImplementedException();
        }
    }
}
