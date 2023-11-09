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
                "nastroj", "hraj", "opakuj", "stale", "ak", "inak", "koniec", "urob", "vypis", "vlakno"
            };

            Headers = new HashSet<string> 
            { 
                "opakuj", "ak", "inak", "urob", "vlakno"
            };

            HeaderPattern = string.Join("|", Headers.Select(Regex.Escape));
            RegexObj = new Regex($@"\b({HeaderPattern})");

        }
    }
}
