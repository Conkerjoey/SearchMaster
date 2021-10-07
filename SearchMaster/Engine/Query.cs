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
        public enum QueryType
        {
            Text,
            Regex,
            FullMatch,
        }

        private string text;
        private QueryType type;

        public Query(string text, QueryType type)
        {
            this.text = text;
            this.type = type;
        }

        public string Text
        {
            get { return text; }
        }

        public QueryType Type
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
