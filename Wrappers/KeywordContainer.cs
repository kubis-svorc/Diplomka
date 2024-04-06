using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Diplomka.Wrappers
{
    public static class KeywordContainer
    {
        public static readonly ICollection<string> Keywords;
        public static readonly ICollection<string> Headers;
        public static readonly string HeaderPattern;
        public static readonly Regex RegexObj;


        static KeywordContainer()
        {
            Keywords = new HashSet<string>
            {
                "nastroj", "hraj", "opakuj", "stale", "ak", "inak", "koniec", "urob", "vypis", "vlakno", "akord"
            };

            Headers = new HashSet<string> 
            { 
                "opakuj", "ak", "inak", "urob", "vlakno", "koniec"
            };

            //HeaderPattern = string.Join("|", Headers.Select(Regex.Escape));
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            int i = 0;
            foreach (string word in Headers)
            {
                stringBuilder.Append(@"\b");
                stringBuilder.Append(word);
                stringBuilder.Append(@"\b");
                if (++i < Headers.Count)
                {
                    stringBuilder.Append("|");
                }
            }
            RegexObj = new Regex(stringBuilder.ToString());

        }
    }
}
