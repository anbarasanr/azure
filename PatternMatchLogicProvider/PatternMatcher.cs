using PatternMatchLogicProvider.Contracts;
using System;
using System.Text.RegularExpressions;

namespace PatternMatchLogicProvider
{
    public class PatternMatcher : IPatternMatchLogicProvider
    {
        /// <summary>
        /// Incoming pattern
        /// </summary>
        public string Pattern { get; private set; }

        /// <summary>
        /// Converts the incoming pattern to a regular expression for simplified matching.
        /// </summary>
        public string RegexPattern => $"^{Pattern.Trim().Replace("*", ".*").Replace("?", ".")}$";

        /// <summary>
        /// Contains logic to check a string if it matches with a pattern.
        /// Input pattern is convereted to a regex and used for matching.
        /// </summary>
        /// <param name="pattern"></param>
        public PatternMatcher(string pattern)
        {
            Pattern = pattern;
        }

        public bool IsPatternValid()
        {
            if (string.IsNullOrEmpty(Pattern)) return false;

            foreach (var c in Pattern)
            {
                if (!(char.IsLetterOrDigit(c) || c == '?' || c == '*'))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if the data matches the Regex Pattern (converted from Pattern) completely. Otherwise false
        /// Also throws exception if the pattern is not valid. Make sure to call the IsPatternValid first to validate the pattern.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsMatch(string data)
        {
            if (!IsPatternValid()) throw new Exception("Invalid pattern");

            if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(Pattern) || Pattern.Trim() == "*")
                return true;

            if (Regex.IsMatch(data, RegexPattern))
            {
                return true;
            }

            return false;
        }

    }
}
