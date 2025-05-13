using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lab1_compile
{
    public static class RegexSearcher
    {
        public static List<MatchResult> FindWordsNotEndingWithT(string text)
        {
            var results = new List<MatchResult>();
            string pattern = @"\b\w*[^tT\W]\b";
            var regex = new Regex(pattern);

            foreach (Match match in regex.Matches(text))
            {
                if (match.Length > 0)
                {
                    results.Add(new MatchResult(match.Value, match.Index, match.Length));
                }
            }

            return results;
        }

        public static List<MatchResult> FindPasswords(string text)
        {
            var results = new List<MatchResult>();
            string pattern = @"[a-zA-Zа-яА-Я0-9#?!@_$\/%\^&*\-|]{8,}";
            var regex = new Regex(pattern);

            foreach (Match match in regex.Matches(text))
            {
                results.Add(new MatchResult(match.Value, match.Index, match.Length));
            }

            return results;
        }

        public static List<MatchResult> FindStrongPasswords(string text)
        {
            var results = new List<MatchResult>();
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#?!@_$\/%\^&*\-|]).{10,}$";
            var regex = new Regex(pattern, RegexOptions.Multiline);

            foreach (Match match in regex.Matches(text))
            {
                results.Add(new MatchResult(match.Value, match.Index, match.Length));
            }

            return results;
        }
    }
}
