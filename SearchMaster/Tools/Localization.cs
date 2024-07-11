using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SearchMaster.Tools
{
    public class Localization
    {
        private static readonly string LANGUAGES_FOLDER = "./Lang";
        public enum SupportedLanguage
        {
            French,
            English
        }

        public Dictionary<string, string> Container { get; set; }

        public Localization(SupportedLanguage language)
        {
            string lang_content = File.ReadAllText(Path.Combine(LANGUAGES_FOLDER, language.ToString() + ".json"));
            Container = JsonSerializer.Deserialize<Dictionary<string, string>>(lang_content);
        }
    }
}
