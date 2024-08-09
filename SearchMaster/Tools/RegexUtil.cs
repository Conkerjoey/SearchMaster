using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SearchMaster.Tools
{
    public class RegexUtil
    {
        // If you want to implement both "*" and "?"
        //     ? - any character  (one and only one)
        //     * - any characters(zero or more)
        // 
        public static string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }
    }
}
