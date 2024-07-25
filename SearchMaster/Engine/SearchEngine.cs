using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SearchMaster.Indexing;
using System.Xml;
using System.ComponentModel;
using DocumentFormat.OpenXml.Bibliography;
using System.Text.Json;

namespace SearchMaster.Engine
{
    [Serializable]
    public class SearchEngine : INotifyPropertyChanged
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

        private string corporaDirectory = null;
        private string acronymFilepath = null;

        [field: NonSerialized]
        private Dictionary<string, string> acronyms = new Dictionary<string, string>();
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public SearchEngine()
        {
            if (corporaDirectory == null)
            {
                corporaDirectory = Path.Combine(Environment.CurrentDirectory, "corpora");
                Directory.CreateDirectory(corporaDirectory);
            }
        }

        public SearchEngine Duplicate()
        {
            return new SearchEngine()
            {
                CorporaDirectory = this.CorporaDirectory,
                AcronymFilepath = this.acronymFilepath,
                Acronyms = this.acronyms?.ToDictionary(entry => entry.Key, entry => entry.Value)
            };
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
                SearchEngine engine = (SearchEngine) formatter.Deserialize(fs);
                engine.LoadAcronyms();
                fs.Flush();
                fs.Close();
                fs.Dispose();
                return engine;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                SearchEngine engine = new SearchEngine();
                engine.LoadAcronyms();
                engine.Save();
                return engine;
            }
        }

        public void LoadAcronyms()
        {
            if (File.Exists(AcronymFilepath))
            {
                acronyms = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(AcronymFilepath));
            }
        }

        public string CorporaDirectory
        {
            get { return corporaDirectory;  }
            set { corporaDirectory = value; this.OnPropertyChanged("CorporaDirectory"); }
        }

        public string AcronymFilepath
        {
            get { return acronymFilepath; }
            set { acronymFilepath = value; this.OnPropertyChanged("AcronymFilepath"); }
        }

        public Dictionary<string, string> Acronyms {
            get { return acronyms; }
            set { acronyms = value; }
        }
    }
}
