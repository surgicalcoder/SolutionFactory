using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SolutionFactory
{
    public static class Extensions
    {
        public static string PerformReplacements(this string input, FactoryArgs parsedArgs, ref Dictionary<string, string> dictionary)
        {
            var result = Regex.Replace(input, AppSettings.SolutionFactoryFriendlyKey, parsedArgs.FriendlyName, RegexOptions.IgnoreCase);
            result = Regex.Replace(result, AppSettings.SolutionFactoryKey, parsedArgs.Namespace, RegexOptions.IgnoreCase);

            const string SecureRegex = @"^.*?GENERATE_SECURE_KEY\((?<KeyStrength>\d+),.*?(?<Id>\d+).*?$";

            var matches = Regex.Matches(input,SecureRegex,RegexOptions.Multiline);
            foreach (Match match in matches)
            {
                int keyStrength = Convert.ToInt32(match.Groups["KeyStrength"].Value);
                var id = match.Groups["Id"].Value;
                
                string rand;
                if (!dictionary.ContainsKey(id))
                {
                    rand = Random.GetRandomString(keyStrength);
                    dictionary.Add(id, rand);
                }
                else
                {
                    rand = dictionary[id];
                }

                result = result.Replace($"GENERATE_SECURE_KEY({keyStrength},{id})", rand);
            }
            
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