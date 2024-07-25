using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Indexing
{
    [Serializable]
    public class StringWrapper : INotifyPropertyChanged, ICloneable
    {
        private string _value;

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

        public StringWrapper(string value)
        {
            this._value = value;
        }

        public object Clone()
        {
            return new StringWrapper(this.Value);
        }

        public static explicit operator string(StringWrapper str)
        {
            return str.Value;
        }

        public string Value
        {
            get { return this._value; }
            set { this._value = value; this.OnPropertyChanged("Value"); }
        }
    }
}
