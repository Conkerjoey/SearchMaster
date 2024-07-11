using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMaster.Tools
{
    public class Localization
    {
        private static readonly string LANGUAGES_FOLDER = "./Lang";

        public Localization()
        {

        }

        public static Localization Load()
        {
            return new Localization();
        }

        public void Save()
        {

        }
    }
}
