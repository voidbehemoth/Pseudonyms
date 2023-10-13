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

        private static string[] _cachedFirstNames;
        private static string[] _cachedLastNames;
        private static string[] _cachedMonoNames;
        private static string[] _cachedSuffixes;
        private static string[] _cachedTitles;

        public static void SetRandomName()
        {
            if (Pepper.IsMyInGameNameSet()) return;

            Logger.Log("Setting random name");

            bool useFirstAndLastNames = ModSettings.GetBool("Use First & Last Names", "voidbehemoth.pseudonyms");
            double chanceSuffix = ModSettings.GetInt("Chance of Suffix (%)", "voidbehemoth.pseudonyms") / 100f;
            double chanceTitle = ModSettings.GetInt("Chance of Title (%)", "voidbehemoth.pseudonyms") / 100f;

            string tempName, name;
            do
            {
                tempName = GetRandomName(useFirstAndLastNames, chanceTitle, chanceSuffix).Trim();
            } while (String.IsNullOrEmpty(tempName) || !ValidateName(tempName));
            name = tempName;

            // PlayerNameMessage message = new PlayerNameMessage((byte)Pepper.GetMyPosition(), name);
            // Service.Game.Network.Send(message);
            Storage.SetString(Storage.Key.GameName, name);
            Storage.Save();

            // Logger.Log("Set name to " + name);

            _cachedFirstNames = null; 
            _cachedLastNames = null; 
            _cachedMonoNames = null;
            _cachedSuffixes = null;
            _cachedTitles = null;
        }

        
        public static string GetRandomName(bool useFirstAndLastNames, double chanceTitle, double chanceSuffix)
        {
            System.Random random = new System.Random();

            bool hasSuffix = useFirstAndLastNames && chanceSuffix != 0 && random.NextDouble() < chanceSuffix;
            bool hasTitle = useFirstAndLastNames && chanceTitle != 0 && random.NextDouble() < chanceTitle;

            if (useFirstAndLastNames && _cachedFirstNames == null) _cachedFirstNames = File.ReadAllLines(PathHelper.firstNamePath);
            if (useFirstAndLastNames && _cachedLastNames == null) _cachedLastNames = File.ReadAllLines(PathHelper.lastNamePath);
            if (!useFirstAndLastNames && _cachedMonoNames == null) _cachedMonoNames = File.ReadAllLines(PathHelper.monoNamePath);
            if (hasSuffix && _cachedSuffixes == null) _cachedSuffixes = File.ReadAllLines(PathHelper.nameSuffixPath);
            if (hasTitle && _cachedTitles == null) _cachedTitles = File.ReadAllLines(PathHelper.nameTitlePath);

            if ((hasTitle && _cachedTitles.Length < 1) || (hasSuffix && _cachedSuffixes.Length < 1) || (useFirstAndLastNames && (_cachedFirstNames.Length < 1 || _cachedLastNames.Length < 1)) || (!useFirstAndLastNames && _cachedMonoNames.Length < 1)) return "John Doe";

            return (useFirstAndLastNames ? (hasTitle ? _cachedTitles[random.Next(_cachedTitles.Length)] : _cachedFirstNames[random.Next(_cachedFirstNames.Length)]) + " " + _cachedLastNames[random.Next(_cachedLastNames.Length)] : _cachedMonoNames[random.Next(_cachedMonoNames.Length)]) + (hasSuffix ? " " + _cachedSuffixes[random.Next(_cachedSuffixes.Length)] : "");
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
