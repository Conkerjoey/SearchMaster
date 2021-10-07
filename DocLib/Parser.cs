using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocLib
{
    public class Parser
    {
        private string[] lines;

        public Parser(string[] lines)
        {
            this.lines = lines;
        }

        public List<WeightedLabel> GetLabels(ref int totalWords)
        {
            List<WeightedLabel> labels = new List<WeightedLabel>();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Replace(",", ""); // Remove comma
                line = lines[i].Replace("(", "").Replace(")", ""); // Remove parenthesis
                line = line.ToLower();

                string[] words = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < words.Length; j++)
                {
                    WeightedLabel l = labels.Find(x => x.GetText() == words[j]);
                    if (l == null)
                    {
                        labels.Add(new WeightedLabel(words[j])); // TODO: Change for an ordered list and speed up comparison process.
                    }
                    else
                    {
                        l.IncrementTotalOccurence();
                    }
                }
                totalWords += words.Length;
            }
            return labels;
        }
    }
}
