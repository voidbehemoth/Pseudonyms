using Game.Interface;
using HarmonyLib;
using Server.Shared.Extensions;
using Server.Shared.Info;
using Services;
using SML;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Pseudonyms
{
    [Mod.SalemMod]
    public class Main
    {
        private static string[] _configFileNames = new string[]
        {
            "FirstNames.txt",
            "LastNames.txt",
            "MonoNames.txt",
            "NameSuffixes.txt",
            "NameTitles.txt"
        };

        public static GameObject RerollNameButton;

        public static void Start()
        {
            Utils.Logger.Log("ain't no way");

            GenerateDirectories();
            GenerateFiles();
            LoadButton();
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

        public static void LoadButton()
        {
            try
            {
                AssetBundle assetBundleFromResources = FromAssetBundle.GetAssetBundleFromResources("Pseudonyms.resources.assetbundles.pseudonyms", Assembly.GetExecutingAssembly());
                RerollNameButton = assetBundleFromResources.LoadAsset<GameObject>("RerollNameButton");
                if (assetBundleFromResources != null)
                {
                    assetBundleFromResources.Unload(false);
                }
            } catch (Exception ex)
            {
                Utils.Logger.Log("button error: " + ex);
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
            if (gameInfo.gamePhase != Server.Shared.State.GamePhase.PICK_NAMES || Pepper.IsMyInGameNameSet()) return;

            Utils.NameHelper.SetRandomName();
        }
    }

    [HarmonyPatch(typeof(Home.Shared.ApplicationController), nameof(Home.Shared.ApplicationController.QuitGame))]
    public class CacheClearer
    {
        [HarmonyPrefix]
        public static void postfix()
        {
            Utils.NameHelper.ClearCache();
        }
    }
}