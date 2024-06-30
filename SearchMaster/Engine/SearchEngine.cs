using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SearchMaster.Indexing;
using System.Xml;

namespace SearchMaster.Engine
{
    [Serializable]
    public class SearchEngine
    {
        public enum ResolverType
        {
            CosineSimilarity,
            LabelDensity,
            Regex,
            FullMatch,
            OkapiBM25,
            TFIDF,
        }

        private string corporaDirectory = "processed";
        private Dictionary<string, string> acronyms = new Dictionary<string, string>();

        public SearchEngine()
        {

        }


        public void Save()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            Stream fs = File.OpenWrite("configuration.dat");
            formatter.Serialize(fs, this);
            fs.Flush();
            fs.Close();
            fs.Dispose();
        }

        public static SearchEngine Load()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                FileStream fs = File.Open("configuration.dat", FileMode.Open);
                SearchEngine engine = (SearchEngine)formatter.Deserialize(fs);
                fs.Flush();
                fs.Close();
                fs.Dispose();
                return engine;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                SearchEngine engine = new SearchEngine();
                engine.Save();
                return engine;
            }
        }

        public string CorporaDirectory
        {
            get { return corporaDirectory;  }
            set { corporaDirectory = value; }
        }

        public Dictionary<string, string> Acronyms {
            get { return acronyms; }
            set { acronyms = value; }
        }
    }
}
