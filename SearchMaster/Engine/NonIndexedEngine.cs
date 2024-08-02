using SearchMaster.Indexing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Engine
{
    public class NonIndexedEngine
    {

        public NonIndexedEngine() { }

        public static QueryResult SearchInFolder(string folder, Query query)
        {
            if (Directory.Exists(folder))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                List<string> files = Utils.ListDirectory(folder, true, null, null);
                List<SearchResult> results = new List<SearchResult>();
                foreach (string file in files)
                {
                    FileType filetype = DocFile.DetermineFileType(file);
                    string[] lines = Reader.ReadLines(new DocFile() { FilePath = file, FileType = filetype });
                    Parser parser = new Parser(lines);
                    NGram ngram = parser.GetNGram();
                    string[] vecQuery = query.Text.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int hit = 0;
                    for (int i = 0; i < vecQuery.Length; i++)
                    {
                        List<WeightedLabel> labels = ngram.WeightedLabels.FindAll(x => x.GetText().ToLower().Contains(vecQuery[i].ToLower()));
                        hit += labels.Count;
                    }
                    if (hit > 0)
                    {
                        results.Add(new SearchResult(new Document() { Name = Path.GetFileName(file), DocumentSource = new DocumentSource(DocumentSource.Type.Local, file), FileType = filetype, NGram = ngram }, SearchResult.RelevanceType.Count, hit));
                    }
                }
                results = results.OrderByDescending(x => x.Relevance).ToList();
                stopwatch.Stop();
                return new QueryResult(results, query, files.Count, stopwatch.Elapsed);
            }
            return null;
        }
    }
}
