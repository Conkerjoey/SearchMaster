using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Indexing
{
    [Serializable]
    public class NGram
    {
        private int ngram_depth = 3;
        private List<int[]> ngrams = new List<int[]>();
        private List<WeightedLabel> weightedLabels = new List<WeightedLabel>();

        [NonSerialized]
        private int[] stack = null;
        private int[] voidArray;

        public NGram(int ngram_depth)
        {
            if ((ngram_depth - 1) % 2 != 0)
                throw new Exception("Invalid N-gram depth. Should be 1, 3, 5, 7, etc.");
            this.ngram_depth = ngram_depth;
            this.stack = new int[(ngram_depth - 1) / 2 + 1];
            this.voidArray = new int[this.stack.Length];
            for (int i = 0; i < this.voidArray.Length; i++)
            {
                voidArray[i] = -1;
            }
            this.voidArray.CopyTo(this.stack, 0);
        }

        public int NGramDepth
        {
            get { return ngram_depth; }
            set { ngram_depth = value; }
        }

        public List<int[]> Ngrams
        {
            get { return this.ngrams; }
            set { this.ngrams = value; }
        }

        public List<WeightedLabel> WeightedLabels
        {
            get { return this.weightedLabels; }
            set { this.weightedLabels = value; }
        }

        public void InsertLabel(string label)
        {
            int idx = WeightedLabels.FindIndex(x => x.GetText() == label);
            if (idx == -1)
            {
                idx = weightedLabels.Count;
                WeightedLabels.Add(new WeightedLabel(label));
            }
            else
            {
                WeightedLabels[idx].IncrementTotalOccurence();
            }
            WordCount++;

            Push(idx); // Update stack
            int[] ng = new int[this.ngram_depth];
            voidArray.CopyTo(ng, ng.Length - voidArray.Length);
            stack.CopyTo(ng, 0);
            for (int i = 0; i < Math.Min(this.ngram_depth - this.stack.Length, ngrams.Count); i++)
            {
                ngrams[ngrams.Count - 1 - i][this.stack.Length + i] = idx;
            }
            ngrams.Add(ng);
        }

        public int WordCount
        {
            get; set;
        }

        private void Push(int idx)
        {
            if (stack == null)
                return;
            for (int i = 0; i < stack.Length - 1; i++)
            {
                stack[i] = stack[i + 1];
            }
            stack[stack.Length - 1] = idx;
        }
    }
}
