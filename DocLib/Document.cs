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
        private string name;
        private DocumentPath documentPath;
        private Guid guid;
        private List<WeightedLabel> weightLabels = new List<WeightedLabel>();
        private List<string> urls = new List<string>();
        private int totalWords;

        [NonSerialized]
        private DocumentType documentType;

        public Document(string name, DocumentPath documentPath)
        {
            this.name = name;
            this.documentPath = documentPath;
            this.documentType = DetermineDocType(this);
            this.guid = Guid.NewGuid();
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public DocumentPath DocumentPath
        {
            get
            {
                return documentPath;
            }
        }

        public void Scan(bool crawlUrlEnabled)
        {
            string[] lines = Reader.ReadLines(this);
            Parser p = new Parser(lines);
            weightLabels.AddRange(p.GetLabels(ref totalWords));
            if (crawlUrlEnabled)
                urls.AddRange(p.GetURLs());
        }

        public string[] GetLines()
        {
            return Reader.ReadLines(this);
        }

        public static DocumentType DetermineDocType(Document doc)
        {
            if (doc.DocumentPath != null)
            {
                string docPathStr = doc.DocumentPath.Path;
                if (docPathStr.EndsWith(".txt") ||
                    docPathStr.EndsWith(".css") ||
                    docPathStr.EndsWith(".html") ||
                    docPathStr.EndsWith(".js"))
                {
                    return DocumentType.Text;
                }
                else if (docPathStr.EndsWith(".pdf"))
                {
                    return DocumentType.PDF;
                }
                else if (docPathStr.EndsWith(".flow"))
                {
                    return DocumentType.Flow;
                }
                else if (docPathStr.EndsWith(".cs"))
                {
                    return DocumentType.CSharp;
                }
                else if (docPathStr.EndsWith(".cpp") ||
                          docPathStr.EndsWith(".c"))
                {
                    return DocumentType.Cpp;
                }
                else if (docPathStr.EndsWith(".doc") ||
                         docPathStr.EndsWith(".docx"))
                {
                    return DocumentType.Word;
                }
                else if (docPathStr.EndsWith(".xls") ||
                         docPathStr.EndsWith(".xlsx"))
                {
                    return DocumentType.Excel;
                }
                else if (docPathStr.EndsWith(".one"))
                {
                    return DocumentType.Onenote;
                }
            }
            return DocumentType.Undefined;
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

        public DocumentType GetDocumentType()
        {
            return documentType;
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
            document.documentType = DetermineDocType(document);
            stream.Close();
            return document;
        }

        public Guid GetGuid()
        {
            return this.guid;
        }

        public override string ToString()
        {
            return name;
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
