using System.Collections.Generic;

namespace Diplomka.Wrappers
{
    public static class KeywordContainer
    {
        public static readonly ICollection<string> Keywords;

        static KeywordContainer()
        {
            Keywords = new HashSet<string>
            {
                "nastroj", "hraj", "opakuj", "stale", "ak", "inak", "koniec", "urob", "vypis", "vlakno"
            };
        }
    }
}
