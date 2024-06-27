using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using DocLib;

namespace MasterIndexer
{
    public class Program
    {
        public static Indexer indexer = null;

        static void Main(string[] args)
        {
            string configFilePath = null;
            if (args.Length > 0)
            {
                configFilePath = Path.GetFullPath(args[0]);
            }
            else
            {
                throw new ArgumentException("Configuration file was not provided in argument 0.");
            }

            XmlDocument xmlConfigFile = new XmlDocument();
            xmlConfigFile.Load(configFilePath);

            XmlNode indexerXmlNode = xmlConfigFile.DocumentElement;
            string indexerVersion = indexerXmlNode.Attributes["version"].InnerText;
            bool indexerMultithreadFlag = true;
            bool indexerMultithreadFlagParsed = bool.TryParse(indexerXmlNode.Attributes["multithreaded"].InnerText, out indexerMultithreadFlag);
            if (!indexerMultithreadFlagParsed)
            {
                Console.WriteLine("[WARNING] The 'multithreaded' flag in the indexer configuration file could not be parsed. Value 'true' will be used.");
            }

            string indexerOutputDirectory = indexerXmlNode.Attributes["output"].InnerText;

            FileAttributes indexerOutputDirectoryAttributes = File.GetAttributes(indexerOutputDirectory);
            if (!indexerOutputDirectoryAttributes.HasFlag(FileAttributes.Directory))
            {
                indexerOutputDirectory = Path.GetDirectoryName(indexerOutputDirectory);
                Console.WriteLine("[WARNING] The 'output' value is not a directory. Value '" + indexerOutputDirectory + "' will be used.");
            }

            int corpusCount = indexerXmlNode.ChildNodes.Count;
            List<Corpus> corpora = new List<Corpus>();

            foreach (XmlNode corpusNode in indexerXmlNode.ChildNodes)
            {
                string corpusName = corpusNode.Attributes["name"].InnerText;
                if (null == corpusName)
                {
                    Console.WriteLine("[WARNING] One corpus does not have a 'name' attribute. Thus, it will not be processed.");
                    continue;
                }
                Corpus currentCorpus = new Corpus() { Name = corpusName };
                foreach (XmlNode locationNode in corpusNode.ChildNodes)
                {
                    string currentLocation = locationNode.InnerText;
                    bool locationExistFlag = Directory.Exists(currentLocation);
                    if (locationExistFlag)
                    {
                        currentCorpus.Location = currentLocation;
                    }
                    else
                    {
                        Console.WriteLine("[WARNING] Location '" + currentLocation + "' is invalid for corpus '" + corpusName + "'.");
                    }
                }
                corpora.Add(currentCorpus);
            }

            indexer = new Indexer(indexerVersion, indexerMultithreadFlag, indexerOutputDirectory);

            foreach (Corpus corpus in corpora)
            {
                indexer.ProcessCorpus(corpus, null, null);
            }

            Console.ReadKey();
        }
    }
}
