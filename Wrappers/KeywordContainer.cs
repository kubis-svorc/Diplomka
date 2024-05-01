namespace Diplomka.Wrappers
{
    using Sanford.Multimedia.Midi;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

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
                "nástroj", "nastroj", "hraj", "opakuj", "stále", "ak", "inak", "koniec", "urob", "výpis", "vlákno", "akord",
                //"c1", "d1", "e1", "f1", "g1", "a1", "h1", "c2", "d2", "e2", "f2", "g2", "a2","h2", "c3",
                //"ck1", "dk1", "ek1", "fk1", "gk1", "ak1", "hk1", "ck2", "dk2", "ek2", "fk2", "gk2", "ak2", "hk2",
                //"cb1", "db1", "eb1", "fb1", "gb1", "ab1", "hb1", "cb2", "db2", "eb2", "fb2", "gb2", "ab2", "hb2",
                "husle", "bicie", "gitara", "organ", "spev", "trúbka", "trubka", "harfa", "akordeón", "akordeon", "flauta", "klavír",
                "klavir", "piano", "náhodný", "nahodny"
            };

            Headers = new HashSet<string> 
            { 
                "opakuj", "ak", "inak", "urob", "vlakno", "vlákno", "koniec"
            };

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
