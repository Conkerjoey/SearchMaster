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
        public static readonly int MAX_QUERY_HISTORY = 50;
        private List<Corpus> corpora = new List<Corpus>();
        private ObservableCollection<Query> queries = new ObservableCollection<Query>();
        private SearchEngine.ResolverType resolverType;
        private bool multithreadedFlag = true;

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public Settings()
        {

        }

        public List<Corpus> Corpora
        {
            get { return corpora; }
            set { corpora = value; }
        }

        public ObservableCollection<Query> Queries
        {
            get { return queries; }
            set { queries = value; this.OnPropertyChanged("Queries"); }
        }

        public SearchEngine.ResolverType ResolverType
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
            set { multithreadedFlag = value; }
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
                fs.Flush();
                fs.Close();
                fs.Dispose();
                return settings;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                Settings settings = new Settings();
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
    }
}
