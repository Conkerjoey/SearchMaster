using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace DocLib
{
    [Serializable]
    public class Document
    {
        private DocumentSource documentSource;
        private Guid guid;
        private List<WeightedLabel> weightLabels = new List<WeightedLabel>();
        private List<string> urls = new List<string>();
        private int totalWords;
        private string filepath;
        private DocumentType documentType;

        public Document(string name, string filepath, DocumentSource documentSource)
        {
            this.Name = name;
            this.documentSource = documentSource;
            this.documentType = DetermineDocType(filepath);
            this.filepath = filepath;
            this.guid = Guid.NewGuid();
        }

        public string Name
        {
            get;set;
        }

        public string Filepath
        {
            get { return filepath; }
            set { filepath = value; }
        }

        public DocumentSource DocumentSource
        {
            get
            {
                return documentSource;
            }
        }

        public void ReadContent()
        {
            string[] lines = Reader.ReadLines(this);
            Parser p = new Parser(lines);
            weightLabels.AddRange(p.GetLabels(ref totalWords));
            urls.AddRange(p.GetURLs());
        }

        public string[] GetLines()
        {
            return Reader.ReadLines(this);
        }

        public List<WeightedLabel> GetWeightedLabels()
        {
            return this.weightLabels;
        }

        public bool IsLabelPresent(WeightedLabel weightLabel)
        {
            return this.weightLabels.Find(x => x.GetText() == weightLabel.GetText()) != null;
        }

        public int GetTotalWords()
        {
            return this.totalWords;
        }

        public DocumentType DocumentType
        {
            get { return this.documentType; }
            set {  this.documentType = value; }
        }

        public List<string> URLs
        {
            get { return this.urls; }
            set { this.urls = value; }
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

        public static DocumentType DetermineDocType(string filepath)
        {
            if (filepath.EndsWith(".txt") ||
                filepath.EndsWith(".css") ||
                filepath.EndsWith(".html") ||
                filepath.EndsWith(".js"))
            {
                return DocumentType.Text;
            }
            else if (filepath.EndsWith(".pdf"))
            {
                return DocumentType.PDF;
            }
            else if (filepath.EndsWith(".flow"))
            {
                return DocumentType.Flow;
            }
            else if (filepath.EndsWith(".cs"))
            {
                return DocumentType.CSharp;
            }
            else if (filepath.EndsWith(".cpp") ||
                      filepath.EndsWith(".c"))
            {
                return DocumentType.Cpp;
            }
            else if (filepath.EndsWith(".doc") ||
                     filepath.EndsWith(".docx"))
            {
                return DocumentType.Word;
            }
            else if (filepath.EndsWith(".xls") ||
                     filepath.EndsWith(".xlsx"))
            {
                return DocumentType.Excel;
            }
            else if (filepath.EndsWith(".one"))
            {
                return DocumentType.Onenote;
            }
            else if (filepath.EndsWith(".py") || filepath.EndsWith(".pyc") || filepath.EndsWith(".pyi") || filepath.EndsWith(".pyd") || filepath.EndsWith(".pyo") || filepath.EndsWith(".pyw") || filepath.EndsWith(".pyz"))
            {
                return DocumentType.Python;
            }
            else if (filepath.EndsWith(".m"))
            {
                return DocumentType.Matlab;
            }
            else if (filepath.EndsWith(".html"))
            {
                return DocumentType.Html;
            }
            else if (filepath.EndsWith(".css"))
            {
                return DocumentType.Css;
            }
            else if (filepath.EndsWith(".js"))
            {
                return DocumentType.Javascript;
            }
            else if (filepath.EndsWith(".json"))
            {
                return DocumentType.Json;
            }
            return DocumentType.Undefined;
        }
    }
}
