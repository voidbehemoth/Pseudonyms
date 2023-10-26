using SalemModLoaderUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SML;
using Services;
using Server.Shared.Messages;
using Utils;
using UnityEngine;
using System.Threading;
using System.Xml.Linq;

namespace Pseudonyms.Utils
{
    public class Logger
    {
        public static void Log(string message)
        {
            Console.WriteLine("[" + Pseudonyms.ModInfo.PLUGIN_GUID + "] " + message);
        }
    }

    public class NameHelper
    {

        enum NameFormat
        {
          SIMPLE,
          COMPLEX
        }

        private static string _defaultName = "John Doe";

        private static string[] _cachedFirstNames;
        private static string[] _cachedLastNames;
        private static string[] _cachedMonoNames;
        private static string[] _cachedSuffixes;
        private static string[] _cachedTitles;

        public static void SetRandomName()
        {

            Logger.Log("Setting random name");


            string name = GetRandomName();

            // PlayerNameMessage message = new PlayerNameMessage((byte)Pepper.GetMyPosition(), name);
            // Service.Game.Network.Send(message);
            Storage.SetString(Storage.Key.GameName, name);
            Storage.Save();
        }

        private static NameFormat ConvertToNameFormat(string s)
        {
            switch (s)
            {
                case "Simple Names":
                    return NameFormat.SIMPLE;
                case "Complex Names":
                    return NameFormat.COMPLEX;
                default:
                    return NameFormat.COMPLEX;
            }
        }

        private static string GetRandomName()
        {
            System.Random random = new System.Random();
            bool useSuffixesAndTitles = ModSettings.GetBool("Use Suffixes & Titles", "voidbehemoth.pseudonyms");
            NameFormat nameFormat = ConvertToNameFormat(ModSettings.GetString("Name Format", "voidbehemoth.pseudonyms"));
            double chanceSuffix = ModSettings.GetInt("Chance of Suffix (%)", "voidbehemoth.pseudonyms") / 100f;
            double chanceTitle = ModSettings.GetInt("Chance of Title (%)", "voidbehemoth.pseudonyms") / 100f;

            bool hasSuffix = useSuffixesAndTitles && chanceSuffix != 0 && random.NextDouble() < chanceSuffix;
            bool hasTitle = useSuffixesAndTitles && chanceTitle != 0 && random.NextDouble() < chanceTitle;

            switch (nameFormat)
            {
                case NameFormat.SIMPLE:
                    if (_cachedMonoNames.Length < 1) return _defaultName;
                    break;
                case NameFormat.COMPLEX:
                    if (_cachedFirstNames.Length < 1 
                        || _cachedLastNames.Length < 1) return _defaultName;
                    if (hasSuffix && _cachedSuffixes.Length < 1) return _defaultName;
                    if (hasTitle && _cachedTitles.Length < 1) return _defaultName;
                    break;
            }

            if (nameFormat == NameFormat.SIMPLE) return _cachedMonoNames[random.Next(_cachedMonoNames.Length)];

            string first = hasTitle ? _cachedTitles[random.Next(_cachedTitles.Length)] : _cachedFirstNames[random.Next(_cachedFirstNames.Length)];
            string last = _cachedLastNames[random.Next(_cachedLastNames.Length)];
            string suffix = hasSuffix ? " " + _cachedSuffixes[random.Next(_cachedSuffixes.Length)] : "";

            return string.Format("{0} {1}{2}", first, last, suffix);
        }

        public static void ClearCache()
        {
            _cachedFirstNames = null;
            _cachedLastNames = null;
            _cachedSuffixes = null;
            _cachedTitles = null;
        }

        public static void CacheNames()
        {
            _cachedMonoNames = ReadAllNames(PathHelper.monoNamePath);
            _cachedFirstNames = ReadAllNames(PathHelper.firstNamePath);
            _cachedLastNames = ReadAllNames(PathHelper.lastNamePath);
            _cachedSuffixes = ReadAllNames(PathHelper.nameSuffixPath);
            _cachedTitles = ReadAllNames(PathHelper.nameTitlePath);
            Logger.Log("Finished caching names!");
        }

        private static string[] ReadAllNames(string path)
        {
            if (path == null || path.Length == 0)
            {
                throw new ArgumentNullException("path");
            }

            List<string> list = new List<string>();
            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
            {
                string name;
                while ((name = streamReader.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(name))
                    {
                        Logger.Log("Skipping empty/null line");
                        continue;
                    }
                    if (!ValidateName(name))
                    {
                        Logger.Log("Skipping invalid/inappropriate name '" + name + "'");
                        continue;
                    }

                    list.Add(name.Trim());
                }
            }

            return list.ToArray();
        }

        public static bool ValidateName(string name)
        {
            Service.Game.ProfanityFilter.FilterText(Service.Home.LocalizationService.GetUILanguageId(), ref name, out var hasProfanity);
    
            return !hasProfanity && name.Length <= 20;
        }
    }

    public class PathHelper
    {
        public static string firstNamePath = Path.GetDirectoryName(Application.dataPath) + "/SalemModLoader/ModFolders/Pseudonyms/FirstNames.txt";
        public static string lastNamePath = Path.GetDirectoryName(Application.dataPath) + "/SalemModLoader/ModFolders/Pseudonyms/LastNames.txt";
        public static string monoNamePath = Path.GetDirectoryName(Application.dataPath) + "/SalemModLoader/ModFolders/Pseudonyms/MonoNames.txt";
        public static string nameSuffixPath = Path.GetDirectoryName(Application.dataPath) + "/SalemModLoader/ModFolders/Pseudonyms/NameSuffixes.txt";
        public static string nameTitlePath = Path.GetDirectoryName(Application.dataPath) + "/SalemModLoader/ModFolders/Pseudonyms/NameTitles.txt";
    }
}
