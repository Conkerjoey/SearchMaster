using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocLib
{
    [Serializable]
    public class DocumentSource
    {
        public enum Type
        {
            Undefined,
            Subversion,
            Local,
            Network,
            Web,
        }

        private Type type;
        private string path;

        public DocumentSource(Type type, string path)
        {
            this.type = type;
            this.path = path;
        }

        public Type PathType
        {
            get
            {
                return type;
            }
        }

        public string Path
        {
            get
            {
                return path;
            }
        }

        public override string ToString()
        {
            return System.IO.Path.GetFullPath(path);
        }
    }
}
