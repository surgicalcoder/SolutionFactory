using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SolutionFactory
{
    public static class Extensions
    {
        public static string PerformReplacements(this string input, FactoryArgs parsedArgs, ref Dictionary<string, string> hashesDictionary, Dictionary<string, string> URLs)
        {
            var result = Regex.Replace(input, AppSettings.SolutionFactoryFriendlyKey, parsedArgs.FriendlyName, RegexOptions.IgnoreCase);
            result = Regex.Replace(result, AppSettings.SolutionFactoryKey, parsedArgs.Namespace, RegexOptions.IgnoreCase);
            result = URLs.Aggregate(result, (current, keyValuePair) => Regex.Replace(current, keyValuePair.Key, keyValuePair.Value, RegexOptions.IgnoreCase));
            result = GenerateSecurityKeyReplacements(input, hashesDictionary, result);

            return result;
        }

        private static string GenerateSecurityKeyReplacements(string input, Dictionary<string, string> hashesDictionary, string result)
        {
            /*language=regexp|jsregexp*/
            const string SecureRegex = @"^.*?GENERATE_SECURE_KEY\((?<KeyStrength>\d+),.*?(?<Id>[a-zA-Z0-9_]+).*?$";

            var matches = Regex.Matches(input, SecureRegex, RegexOptions.Multiline);

            foreach (Match match in matches)
            {
                int keyStrength = Convert.ToInt32(match.Groups["KeyStrength"].Value);
                var id = match.Groups["Id"].Value;
                bool base64 = false;

                if (id.StartsWith("base64_"))
                {
                    base64 = true;
                    id = id.Replace("base64_", "");
                }

                string rand;

                if (!hashesDictionary.ContainsKey(id))
                {
                    rand = Random.GetRandomString(keyStrength);
                    hashesDictionary.Add(id, rand);
                }
                else
                {
                    rand = hashesDictionary[id];
                }

                if (base64)
                {
                    result = result.Replace($"GENERATE_SECURE_KEY({keyStrength},{id})", Base64Encode(rand));
                }
                else
                {
                    result = result.Replace($"GENERATE_SECURE_KEY({keyStrength},{id})", rand);
                }
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
        
        

        public static string Base64Encode(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}