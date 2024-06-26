using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace MasterIndexer
{
    [Serializable]
    public class Filter
    {
        public Filter()
        {
            IgnoreList = new ObservableCollection<string>();
        }

        public ObservableCollection<string> IgnoreList
        {
            get; set;
        }

        public Filter Duplicate()
        {
            return new Filter() { IgnoreList = new ObservableCollection<string>(this.IgnoreList) };
        }
    }
}
