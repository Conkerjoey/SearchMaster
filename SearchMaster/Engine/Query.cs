using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Engine
{
    [Serializable]
    public class Query : IEquatable<Query>
    {

        private string text;
        private Settings.EResolverType type;

        public Query(string text, Settings.EResolverType type)
        {
            this.text = text;
            this.type = type;
        }

        public string Text
        {
            get { return text; }
        }

        public Settings.EResolverType Type
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

        public bool Equals(Query other)
        {
            return this.text == other.text && this.type == other.type;
        }
    }
}
