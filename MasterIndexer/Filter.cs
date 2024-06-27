using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MasterIndexer
{
    [Serializable]
    public class Filter : INotifyPropertyChanged
    {

        private ObservableCollection<String> _filters = new ObservableCollection<String>();

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Filter()
        {
            
        }

        public ObservableCollection<String> IgnoreList
        {
            get { return this._filters; } set { _filters = value; this.OnPropertyChanged("IgnoreList"); }
        }

        public Filter Duplicate()
        {
            return new Filter() { IgnoreList = new ObservableCollection<String>(this.IgnoreList) };
        }
    }
}
