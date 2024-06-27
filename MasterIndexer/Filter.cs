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

        private ObservableCollection<StringWrapper> _filters = new ObservableCollection<StringWrapper>();

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

        public List<string> ToList()
        {
            return this._filters.Cast<string>().ToList();
        }

        public ObservableCollection<StringWrapper> IgnoreList
        {
            get { return this._filters; } set { _filters = value; this.OnPropertyChanged("IgnoreList"); }
        }

        public Filter Duplicate()
        {
            var clonedList = this.IgnoreList.Select(objEntity => (StringWrapper) objEntity.Clone()).ToList();
            return new Filter() { IgnoreList = new ObservableCollection<StringWrapper>(clonedList) };
        }
    }
}
