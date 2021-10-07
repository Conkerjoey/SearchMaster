using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DocLib;
using System.Xml;

namespace SearchMaster.Engine
{
    public class SearchEngine
    {
        public enum ResolverType
        {
            CosineSimilarity,
            LabelDensity,
        }

        private string version = null;
        private string corporaDirectory = null;
        private Dictionary<string, string> acronyms = null;

        public SearchEngine()
        {

        }

        public string CorporaDirectory
        {
            get { return corporaDirectory;  }
        }

        public static SearchEngine LoadConfiguration(string configurationFilePath)
        {
            SearchEngine searchEngine = new SearchEngine();
            string configurationFullFilePath = Path.GetFullPath(configurationFilePath);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(configurationFullFilePath);

            XmlNode indexerXmlNode = xmlDocument.DocumentElement;
            searchEngine.version = indexerXmlNode.Attributes["version"].InnerText;
            searchEngine.corporaDirectory = xmlDocument.GetElementsByTagName("corpora_directory")[0].InnerText;
            if (!Directory.Exists(searchEngine.corporaDirectory))
            {
                throw new ArgumentException("Corpora path in EngineConfiguration.xml file is invalid. Program cannot continue.");
            }

            searchEngine.acronyms = new Dictionary<string, string>();
            XmlNode acronymsNode = xmlDocument.GetElementsByTagName("acronyms")[0];
            foreach (XmlNode acronym in acronymsNode.ChildNodes)
            {
                string acronymName = acronym.Attributes["name"].InnerText;
                string acronymValue = acronym.InnerText;
                if (!searchEngine.acronyms.ContainsKey(acronymName))
                {
                    searchEngine.acronyms.Add(acronymName, acronymValue);
                }
            }
            return searchEngine;
        }

        public string GetVersion()
        {
            return this.version;
        }
    }
}
