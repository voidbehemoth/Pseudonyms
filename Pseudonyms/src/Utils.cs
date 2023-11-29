using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SML;
using Services;
using Utils;
using UnityEngine;
using System.Threading;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

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
            new Thread(() =>
            {
                _cachedMonoNames = ReadAllNames(PathHelper.monoNamePath);
                _cachedFirstNames = ReadAllNames(PathHelper.firstNamePath);
                _cachedLastNames = ReadAllNames(PathHelper.lastNamePath);
                _cachedSuffixes = ReadAllNames(PathHelper.nameSuffixPath);
                _cachedTitles = ReadAllNames(PathHelper.nameTitlePath);
                Logger.Log("Finished caching names!");
            }).Start();
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
            bool regex = Regex.IsMatch(name, @"^[a-zA-Z ]+$");

            return regex && !hasProfanity && name.Length <= 20 && name.Length >= 1;
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

    public class LoadEmbeddedResources
    {
        public static Sprite LoadSprite(string filePath, float pixelsPerUnit = 100f, SpriteMeshType spriteType = SpriteMeshType.Tight)
        {
            Texture2D texture2D = LoadTexture(filePath);
            return Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0f, 0f), pixelsPerUnit, 0u, spriteType);
        }

        public static Texture2D LoadTexture(string fileName)
        {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string filePath = execAssembly.GetManifestResourceNames().Single((string str) => str.EndsWith(fileName));
            Stream manifestResourceStream = execAssembly.GetManifestResourceStream(filePath);
            if (manifestResourceStream == null) Logger.Log("Manifest resource stream is null!");
            Texture2D texture2D = new Texture2D(2, 2);
            if (ImageConversion.LoadImage(texture2D, ReadFully(manifestResourceStream)))
            {
                return texture2D;
            }

            Logger.Log("File does not exist!");
            return null;
        }

        private static byte[] ReadFully(Stream input)
        {
            using MemoryStream memoryStream = new MemoryStream();
            input.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
