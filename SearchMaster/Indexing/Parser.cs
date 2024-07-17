using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SearchMaster.Indexing
{
    public class Parser
    {
        private string[] lines;
        private Regex urlRegex = new Regex(@"(https?)://(-\.)?([^\s/?\.#-]+\.?)+(/[^\s]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Parser(string[] lines)
        {
            this.lines = lines;
        }

        public NGram GetNGram()
        {
            NGram ngram = new NGram(5);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Replace(",", " ").Replace("{", " ").Replace("}", " "); // Remove comma, curly-bracket
                line = lines[i].Replace("(", " ").Replace(")", " ").Replace("[", " ").Replace("]", " "); // Remove parenthesis
                line = line.ToLower();

                string[] words = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < words.Length; j++)
                {
                    ngram.InsertLabel(words[j]);
                }
            }
            return ngram;
        }

        public List<string> GetURLs()
        {
            List<string> urls = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                MatchCollection matches = urlRegex.Matches(lines[i]);
                foreach (Match match in matches)
                {
                    urls.Add(match.Value);
                }
            }
            return urls;
        }
    }
}
