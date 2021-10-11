using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Engine
{
    [Serializable]
    public class Query
    {

        private string text;
        private SearchEngine.ResolverType type;

        public Query(string text, SearchEngine.ResolverType type)
        {
            this.text = text;
            this.type = type;
        }

        public string Text
        {
            get { return text; }
        }

        public SearchEngine.ResolverType Type
        {
            get { return type; }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Query))
                return false;
            return ((Query)obj).text == this.text && ((Query)obj).type == type;
        }

        public override string ToString()
        {
            return text;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
