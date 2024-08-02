using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SearchMaster.Indexing;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.ComponentModel;
using SearchMaster.Engine;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.ObjectModel;
using SearchMaster.Tools;

namespace SearchMaster
{

    [Serializable]
    public class Settings : INotifyPropertyChanged
    {
        public enum EResolverType
        {
            CosineSimilarity,
            LabelDensity,
            Regex,
            FullMatch,
            OkapiBM25,
            TFIDF,
        }

        public static readonly int MAX_QUERY_HISTORY = 50;
        private ObservableCollection<Corpus> corpora = new ObservableCollection<Corpus>();
        private ObservableCollection<Query> queries = new ObservableCollection<Query>();
        private EResolverType resolverType;
        private bool multithreadedFlag = true;
        private bool useAcronymFlag = false;
        private bool nonIndexedSearch = false;
        private string corporaDirectory = null;
        private string acronymFilepath = null;

        [field: NonSerialized]
        private Dictionary<string, string> acronyms = new Dictionary<string, string>();

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public Settings()
        {
            if (corporaDirectory == null)
            {
                corporaDirectory = Path.Combine(Environment.CurrentDirectory, "corpora");
                Directory.CreateDirectory(corporaDirectory);
            }
        }

        public ObservableCollection<Corpus> Corpora
        {
            get { return corpora; }
            set { corpora = value; this.OnPropertyChanged("Corpora"); }
        }

        public ObservableCollection<Query> Queries
        {
            get { return queries; }
            set { queries = value; this.OnPropertyChanged("Queries"); }
        }

        public EResolverType ResolverType
        {
            get
            {
                return resolverType;
            }
            set
            {
                resolverType = value;
            }
        }

        public bool MultithreadingEnable
        {
            get { return multithreadedFlag; }
            set { multithreadedFlag = value; this.OnPropertyChanged("MultithreadingEnable"); }
        }

        public bool UseAcronymEnable
        {
            get { return useAcronymFlag; }
            set { useAcronymFlag = value; this.OnPropertyChanged("UseAcronymEnable"); }
        }

        public bool NonIndexedSearch
        {
            get { return nonIndexedSearch; }
            set { nonIndexedSearch = value; this.OnPropertyChanged("NonIndexedSearch"); }
        }

        public void Save()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            Stream fs = File.OpenWrite("settings.dat");
            formatter.Serialize(fs, this);
            fs.Flush();
            fs.Close();
            fs.Dispose();
        }

        public static Settings Load()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            try
            {
                FileStream fs = File.Open("settings.dat", FileMode.Open);
                Settings settings = (Settings)formatter.Deserialize(fs);
                settings.LoadAcronyms();
                fs.Flush();
                fs.Close();
                fs.Dispose();
                return settings;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                Settings settings = new Settings();
                settings.LoadAcronyms();
                settings.Save();
                return settings;
            }
        }

        public void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Settings Duplicate()
        {
            return new Settings()
            {
                MultithreadingEnable = this.multithreadedFlag,
                UseAcronymEnable = this.useAcronymFlag,
                CorporaDirectory = this.CorporaDirectory,
                AcronymFilepath = this.acronymFilepath,
                Acronyms = this.acronyms?.ToDictionary(entry => entry.Key, entry => entry.Value), // Deep copy
                Corpora = this.corpora, // soft copy
                Queries = this.queries // soft copy
            };
        }

        public void LoadAcronyms()
        {
            if (File.Exists(AcronymFilepath))
            {
                acronyms = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(AcronymFilepath));
                throw new Exception();
            }
        }

        public string CorporaDirectory
        {
            get { return corporaDirectory; }
            set { corporaDirectory = value; this.OnPropertyChanged("CorporaDirectory"); }
        }

        public string AcronymFilepath
        {
            get { return acronymFilepath; }
            set { acronymFilepath = value; this.OnPropertyChanged("AcronymFilepath"); }
        }

        public Dictionary<string, string> Acronyms
        {
            get { return acronyms; }
            set { acronyms = value; }
        }
    }
}
