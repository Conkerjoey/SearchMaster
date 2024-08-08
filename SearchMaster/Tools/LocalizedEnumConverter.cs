using SearchMaster.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Tools
{
    public class LocalizedEnumConverter : TypeConverter
    {
        private ResourceManager _resources = new ResourceManager("SearchMaster.Properties.lang", typeof(lang).Assembly);

        public LocalizedEnumConverter()
        {

        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return _resources.GetString(value.ToString(), Properties.lang.Culture);
            }
            throw new NotImplementedException();
        }
    }
}
