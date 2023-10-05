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

        public static void SetRandomName()
        {
            Logger.Log("Setting random name");

            bool useFirstAndLastNames = ModSettings.GetBool("Use First & Last Names");

            string tempName, name;
            do
            {
                tempName = GetRandomName(useFirstAndLastNames);
                tempName.Trim();
            } while (String.IsNullOrEmpty(tempName) || !ValidateName(tempName));
            name = tempName;

            PlayerNameMessage message = new PlayerNameMessage((byte)Pepper.GetMyPosition(), name);
            Service.Game.Network.Send(message);
            Storage.SetString(Storage.Key.GameName, name);
            Storage.Save();

            Logger.Log("Set name to " + name);

            _cachedFirstNames = null; 
            _cachedLastNames = null; 
            _cachedMonoNames = null;
        }

        
        public static string GetRandomName(bool useFirstAndLastNames)
        {
            string name = "John Doe";
            try
            {
                if (useFirstAndLastNames && _cachedFirstNames == null) _cachedFirstNames = File.ReadAllLines(PathHelper.firstNamePath);
                if (useFirstAndLastNames && _cachedLastNames == null) _cachedLastNames = File.ReadAllLines(PathHelper.lastNamePath);
                if (!useFirstAndLastNames && _cachedMonoNames == null) _cachedMonoNames = File.ReadAllLines(PathHelper.monoNamePath);

                System.Random random = new System.Random();

                if (useFirstAndLastNames && (_cachedFirstNames.Length < 1 || _cachedLastNames.Length < 1) || !useFirstAndLastNames && _cachedMonoNames.Length < 1) return "John Doe";

                name = useFirstAndLastNames ? _cachedFirstNames[random.Next(_cachedFirstNames.Length)] + " " + _cachedLastNames[random.Next(_cachedLastNames.Length)] : _cachedMonoNames[_cachedMonoNames.Length];
            } catch (Exception e)
            {
                Logger.Log("error while getting random name: " + e);
            }
            
            return name;
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
    }
}
