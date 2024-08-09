using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Engine
{
    public interface IResolver
    {
        QueryResult SearchQuery(Query query);
    }
}
