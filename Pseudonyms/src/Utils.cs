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
          FIRST_LAST,
          FIRST_LAST_SUFFIX_TITLE
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

            NameFormat nameFormat = ConvertToNameFormat(ModSettings.GetString("Name Format", "voidbehemoth.pseudonyms"));
            double chanceSuffix = ModSettings.GetInt("Chance of Suffix (%)", "voidbehemoth.pseudonyms") / 100f;
            double chanceTitle = ModSettings.GetInt("Chance of Title (%)", "voidbehemoth.pseudonyms") / 100f;

            string name = GetRandomName(nameFormat, chanceTitle, chanceSuffix);

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
                case "First & Last Names":
                    return NameFormat.FIRST_LAST;
                case "First & Last Names (+ suffixes & titles)":
                    return NameFormat.FIRST_LAST_SUFFIX_TITLE;
                default:
                    return NameFormat.FIRST_LAST;
            }
        }

        private static string GetRandomName(NameFormat nameFormat, double chanceTitle, double chanceSuffix)
        {
            CacheNames(nameFormat);

            System.Random random = new System.Random();

            bool hasSuffix = nameFormat == NameFormat.FIRST_LAST_SUFFIX_TITLE && chanceSuffix != 0 && random.NextDouble() < chanceSuffix;
            bool hasTitle = nameFormat == NameFormat.FIRST_LAST_SUFFIX_TITLE && chanceTitle != 0 && random.NextDouble() < chanceTitle;

            switch (nameFormat)
            {
                case NameFormat.SIMPLE:
                    if (_cachedMonoNames.Length < 1) return _defaultName;
                    break;
                case NameFormat.FIRST_LAST:
                    if (_cachedFirstNames.Length < 1 
                        || _cachedLastNames.Length < 1) return _defaultName;
                    break;
                case NameFormat.FIRST_LAST_SUFFIX_TITLE:
                    if (_cachedFirstNames.Length < 1 
                        || _cachedLastNames.Length < 1 
                        || (hasSuffix && _cachedSuffixes.Length < 1)
                        || (hasTitle && _cachedTitles.Length < 1)) return _defaultName;
                    break;
                default:
                    break;
            }
            
            return (nameFormat == NameFormat.SIMPLE) ? _cachedMonoNames[random.Next(_cachedMonoNames.Length)] 
                : string.Format("{0} {1}{2}", 
                hasTitle ? _cachedTitles[random.Next(_cachedTitles.Length)] : _cachedFirstNames[random.Next(_cachedFirstNames.Length)], 
                _cachedLastNames[_cachedLastNames.Length], 
                hasSuffix ? " " + _cachedSuffixes[random.Next(_cachedSuffixes.Length)]  : "");
        }

        public static void ClearCache()
        {

        }

        private static void CacheNames(NameFormat nameFormat)
        {
            switch(nameFormat)
            {
                case NameFormat.SIMPLE:
                    if (_cachedMonoNames != null) return;
                    _cachedMonoNames = ReadAllNames(PathHelper.monoNamePath);
                    return;
                case NameFormat.FIRST_LAST:
                    if (_cachedFirstNames == null) _cachedFirstNames = ReadAllNames(PathHelper.firstNamePath);
                    if (_cachedLastNames == null) _cachedLastNames = ReadAllNames(PathHelper.lastNamePath);
                    return;
                case NameFormat.FIRST_LAST_SUFFIX_TITLE:
                    if (_cachedFirstNames == null) _cachedFirstNames = ReadAllNames(PathHelper.firstNamePath);
                    if (_cachedLastNames == null) _cachedLastNames = ReadAllNames(PathHelper.lastNamePath);
                    if (_cachedSuffixes == null) _cachedSuffixes = ReadAllNames(PathHelper.nameSuffixPath);
                    if (_cachedTitles == null) _cachedTitles = ReadAllNames(PathHelper.nameTitlePath);
                    return;
                default:
                    return;
            }
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
                        Logger.Log("Skipping invalid name '" + name + "'");
                        continue;
                    }

                    list.Add(name.Trim());
                }
            }

            return list.ToArray();
        }

        public static bool ValidateName(string name)
        {
            bool valid;
            valid = (Regex.IsMatch(name, @"^[a-zA-Z ]+$") && name.Length <= 16);

            string chatToCheck = name;
            Service.Game.ProfanityFilter.FilterText(Service.Home.LocalizationService.GetUILanguageId(), ref chatToCheck, out var hasProfanity);
            valid = valid && !hasProfanity;

            return valid;
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
