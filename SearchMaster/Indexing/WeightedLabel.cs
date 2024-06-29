using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Indexing
{
    [Serializable]
    public class WeightedLabel
    {
        private string text;
        private int totalOccurence = 1;
        private double weight;

        public WeightedLabel(string text)
        {
            this.text = text;
            this.weight = 0;
        }

        public WeightedLabel(string text, double weight)
        {
            this.text = text;
            this.weight = weight;
        }

        public double GetWeight()
        {
            return weight;
        }

        public void SetWeight(double weight)
        {
            this.weight = weight;
        }

        public string GetText()
        {
            return text;
        }

        public override string ToString()
        {
            return text + " " + weight;
        }

        public void IncrementTotalOccurence()
        {
            this.totalOccurence++;
        }

        public int GetTotalOccurence()
        {
            return this.totalOccurence;
        }
    }
}
