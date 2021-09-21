using System;
using System.Linq;

namespace VkAutoResponder
{
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces chars in string input if they match the predicate
        /// </summary>
        /// <param name="input"></param>
        /// <param name="replacement"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static string ReplaceWithPredicate(this string input, char replacement, Func<char, bool> predicate)
        {
            return string.Create(input.Length, input, (span, s) =>
            {
                for (var i = 0; i < s.Length; i++)
                {
                    if (predicate(s[i]))
                    {
                        span[i] = replacement;
                    }
                    else
                    {
                        span[i] = s[i];
                    }
                }
            });
        }
    }
}