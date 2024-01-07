using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Fluent.IO;
using NLog;
using Onion.SolutionParser.Parser;
using PowerArgs;

namespace SolutionFactory
{
    class Program
    {
        private static FactoryArgs parsedArgs;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static List<string> ExtensionsToEdit = AppSettings.FileExtensionsToEdit.Split('|').ToList();
        private static Dictionary<string, string> hashes = new();
        private static Dictionary<string, string> urls = new();

        static void Main(string[] args)
        {
            try
            {
                parsedArgs = Args.Parse<FactoryArgs>(args);
            }
            catch (ArgException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(ArgUsage.GetUsage<FactoryArgs>());
                return;
            }

            logger.Info("Current settings are : ");
            logger.Info("FriendlyName = " + parsedArgs.FriendlyName);
            logger.Info("CleanOnly = " + parsedArgs.CleanOnly);
            logger.Info("Namespace = " + parsedArgs.Namespace);
            logger.Info("PathToSolution = " + parsedArgs.PathToSolution);
            // string nsOfSolution = parsedArgs.Namespace;

            PerformPortMapping();

            Path target = Path.Get("output");

            if (!target.Exists)
            {
                logger.Info("Output directory does not exist, creating");
                target.CreateDirectory();
            }
            else
            {
                logger.Info("Output directory does exist, deleting and creating");
                ForceDeleteDirectory(target.FullPath);
                target.CreateDirectory();
            }
            
            var baseDir = Path.Get(parsedArgs.PathToSolution);

            PrepareSolutionPath(baseDir);

            if (parsedArgs.CleanOnly)
            {
                return;
            }

            baseDir.Copy(target, Overwrite.Always, true);

            if (System.IO.Directory.Exists(System.IO.Path.Combine(target.FullPath, ".git")))
            {
                logger.Info("git dir located, deleting");
                ForceDeleteDirectory(target.Directories(".git", false).FullPath);
            }

            string slnPath = target.Files("*.sln", false).FullPath;
            logger.Info("Remaing solution file " + slnPath);

            var destFileName = $@"{System.IO.Path.GetDirectoryName(slnPath)}\{System.IO.Path.GetFileName(slnPath).PerformReplacements(parsedArgs, ref hashes, urls)}";
            System.IO.File.Move(slnPath, destFileName);
            slnPath = destFileName;
            var parser = new SolutionParser(slnPath);
            var solution = parser.Parse();

            var replacementDictionary = new Dictionary<Guid, Guid>();
            string solutionContents = System.IO.File.ReadAllText(slnPath);

            foreach (var solutionProject in solution.Projects)
            {
                Guid guid = Guid.NewGuid();
                Guid oldGuid = solutionProject.Guid;
                replacementDictionary.Add(oldGuid, guid);

                solutionContents = solutionContents.Replace(oldGuid.ToString(), guid.ToString());
                logger.Debug($"Adding replacement for {oldGuid} to {guid}");
                solutionContents = solutionContents.PerformReplacements(parsedArgs, ref hashes, urls);
            }

            target.Directories().Move(delegate (Path path)
            {
                var pathToReplace = path.FullPath.Replace(target.FullPath, "").Substring(1).PerformReplacements(parsedArgs, ref hashes, urls);

                return Path.Get(target.FullPath, pathToReplace);
            });

            target.Files("*.*", true).Move(delegate (Path path)
            {
                try
                {

                    var pathToReplace = path.FullPath.Replace(target.FullPath, "").Substring(1).PerformReplacements(parsedArgs, ref hashes, urls);

                    if (path.HasExtension && ExtensionsToEdit.Contains(path.Extension))
                    {
                        string contents = path.Read().PerformReplacements(parsedArgs, ref hashes, urls);

                        var theRegex = @"<IISUrl>http://.*?:(?<Port>\d+)/</IISUrl>";

                        if (path.Extension == ".csproj" && contents.Contains("<IISUrl>"))
                        {
                            contents = Regex.Replace(contents, theRegex, match => match.Value.Replace(match.Groups["Port"].Value, CalculatePortFromInput(parsedArgs.Namespace).ToString()));
                        }

                        replacementDictionary.ForEach(x => contents.Replace(x.Key.ToString(), x.Value.ToString()));

                        path.Write(contents);
                    }

                    return Path.Get(target.FullPath, pathToReplace);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return path;
                }
            });


            System.IO.File.WriteAllText(slnPath, solutionContents);


        }

        private static void PerformPortMapping()
        {
            var rnd = new System.Random();
            foreach (var s in AppSettings.URLs.Split(';'))
            {
                var uri = new Uri(s);
                uri = uri.SetPort(rnd.Next(1025, 32768));
                urls.Add(s, uri.ToString());
            }
        }

        private static int CalculatePortFromInput(string input)
        {

            return (int)(1000 + GetInt64HashCode(input) % 64536);
        }

        private static Int64 GetInt64HashCode(string strText)
        {
            Int64 hashCode = 0;
            if (!string.IsNullOrEmpty(strText))
            {
                byte[] byteContents = Encoding.Unicode.GetBytes(strText);
                System.Security.Cryptography.SHA256 hash = new System.Security.Cryptography.SHA256CryptoServiceProvider();
                byte[] hashText = hash.ComputeHash(byteContents);
                Int64 hashCodeStart = BitConverter.ToInt64(hashText, 0);
                Int64 hashCodeMedium = BitConverter.ToInt64(hashText, 8);
                Int64 hashCodeEnd = BitConverter.ToInt64(hashText, 24);
                hashCode = hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
            }

            if (hashCode < 0)
            {
                hashCode = hashCode * -1;
            }
            return (hashCode);
        }
        public static void ForceDeleteDirectory(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                return;
            }
            var directory = new System.IO.DirectoryInfo(path) { Attributes = System.IO.FileAttributes.Normal };

            foreach (var info in directory.GetFileSystemInfos("*", System.IO.SearchOption.AllDirectories))
            {
                info.Attributes = System.IO.FileAttributes.Normal;
            }

            directory.Delete(true);
        }

        private static void PrepareSolutionPath(Path baseDir)
        {
            baseDir.Directories("bin", true).Delete(true);
            baseDir.Directories("obj", true).Delete(true);

            baseDir.Files("*.user", true).Delete();
            baseDir.Files("*.tss", true).Delete();
            baseDir.Files("*.suo", true).Delete();
        }
    }
    
    public class Random
    {
        public static string GetRandomString(int length, string characterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!?$%^&*()-_=+[]#';/")
        {
            if (length < 0)
                throw new ArgumentException("length must not be negative", "length");
            if (length > int.MaxValue / 8) // 250 million chars ought to be enough for anybody
                throw new ArgumentException("length is too big", "length");
            if (characterSet == null)
                throw new ArgumentNullException("characterSet");
            var characterArray = characterSet.Distinct().ToArray();
            if (characterArray.Length == 0)
                throw new ArgumentException("characterSet must not be empty", "characterSet");

            var bytes = new byte[length * 8];
            new RNGCryptoServiceProvider().GetBytes(bytes);
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                ulong value = BitConverter.ToUInt64(bytes, i * 8);
                result[i] = characterArray[value % (uint)characterArray.Length];
            }

            return new string(result);
        }
    }
    public static class UriExtensions {
        public static Uri SetPort(this Uri uri, int newPort) {
            var builder = new UriBuilder(uri);
            builder.Port = newPort;
            return builder.Uri;
        }
    }
}
