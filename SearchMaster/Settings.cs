using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterIndexer;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.ComponentModel;
using SearchMaster.Engine;

namespace SearchMaster
{
    [Serializable]
    public class Settings
    {
        private List<Corpus> corpora = new List<Corpus>();
        private List<Query> queries = new List<Query>();
        private SearchEngine.ResolverType resolverType;
        private bool multithreadedFlag = true;

        public Settings()
        {

        }

        public List<Corpus> Corpora
        {
            get { return corpora; }
        }

        public List<Query> Queries
        {
            get { return queries; }
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
    }
}
