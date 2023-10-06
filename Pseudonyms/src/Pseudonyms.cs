using Game.Interface;
using HarmonyLib;
using Server.Shared.Info;
using Services;
using SML;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Pseudonyms
{
    [Mod.SalemMod]
    public class Main
    {
        private static string[] _configFileNames = new string[]
        {
            "FirstNames.txt",
            "LastNames.txt",
            "MonoNames.txt"
        };

        public static void Start()
        {
            Utils.Logger.Log("ain't no way");

            GenerateDirectories();
            GenerateFiles();
        }

        public static void GenerateDirectories()
        {
            string path = Path.GetDirectoryName(Application.dataPath) + "/SalemModLoader/ModFolders/Pseudonyms";

            if (Directory.Exists(path)) return;

            Directory.CreateDirectory(path);
        }

        public static void GenerateFiles()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (string fileName in _configFileNames)
            {
                if (File.Exists(Path.GetDirectoryName(Application.dataPath) + "/SalemModLoader/ModFolders/Pseudonyms/" + fileName)) continue;

                string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
                string result;
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }

                File.WriteAllText(Path.GetDirectoryName(Application.dataPath) + "/SalemModLoader/ModFolders/Pseudonyms/" + fileName, result);
            }
        }
    }


    public static class ModInfo
    {
        public const string PLUGIN_GUID = "Pseudonyms";

        public const string PLUGIN_NAME = "Pseudonyms";

        public const string PLUGIN_VERSION = "1.0.0";
    }

    
    [HarmonyPatch(typeof(EndgameWrapupOverlayController))]
    public class ListenerHandler
    {
        
        [HarmonyPostfix]
        [HarmonyPatch("InitializeListeners")]
        public static void InitializeListeners()
        {
            Utils.Logger.Log("Initializing listeners...");
            GameInfoObservation gameInfo = Service.Game.Sim.info.gameInfo;
            gameInfo.OnDataChanged = (Action<GameInfo>)Delegate.Combine(gameInfo.OnDataChanged, new Action<GameInfo>(HandleGamePhaseChanged));
        }

        
        [HarmonyPostfix]
        [HarmonyPatch("OnDestroy")]
        public static void OnDestroy()
        {
            Utils.Logger.Log("Disabling listeners...");
            GameInfoObservation gameInfo = Service.Game.Sim.info.gameInfo;
            gameInfo.OnDataChanged = (Action<GameInfo>)Delegate.Remove(gameInfo.OnDataChanged, new Action<GameInfo>(HandleGamePhaseChanged));
        }

        public static void HandleGamePhaseChanged(GameInfo gameInfo)
        {
            if (gameInfo.gamePhase != Server.Shared.State.GamePhase.PICK_NAMES) return;

            

            if (ModSettings.GetBool("Random Names", "voidbehemoth.pseudonyms")) Utils.NameHelper.SetRandomName();
        }
    }
}