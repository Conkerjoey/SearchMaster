using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SearchMaster.Indexing
{
    [Serializable]
    public class Document
    {
        private Guid guid;

        public Document()
        {
            this.guid = Guid.NewGuid();
        }

        public string Name
        {
            get;set;
        }

        public Document Parent
        {
            get; set;
        }

        public DocumentSource DocumentSource
        {
            get;set;
        }

        public FileType FileType
        {
            get;set;
        }

        public List<WeightedLabel> WeightedLabels
        {
            get;set;
        }

        public bool IsLabelPresent(WeightedLabel weightLabel)
        {
            return this.WeightedLabels.Find(x => x.GetText() == weightLabel.GetText()) != null;
        }

        public int WordCount
        {
            get;set;
        }

        public void Save(string directory)
        {
            Directory.CreateDirectory(directory);
            string filePath = Path.Combine(new string[] { directory, GetGuid().ToString() });
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, this);
            stream.Close();
        }

        public static Document Load(string path)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            Document document = (Document)formatter.Deserialize(stream);
            stream.Close();
            return document;
        }

        public Guid GetGuid()
        {
            return this.guid;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is Document)
            {
                return this.guid.ToString() == ((Document)obj).GetGuid().ToString();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
