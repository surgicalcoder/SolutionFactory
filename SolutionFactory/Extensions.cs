using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SolutionFactory
{
    public static class Extensions
    {
        public static string PerformReplacements(this string input, FactoryArgs parsedArgs)
        {
            var result = Regex.Replace(input, AppSettings.SolutionFactoryFriendlyKey, parsedArgs.FriendlyName, RegexOptions.IgnoreCase);
            result = Regex.Replace(result, AppSettings.SolutionFactoryKey, parsedArgs.Namespace, RegexOptions.IgnoreCase);
            return result;
        }

        public static void ForEach<T>(
            this IEnumerable<T> source,
            Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }
    }
}