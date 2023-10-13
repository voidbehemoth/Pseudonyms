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
            "MonoNames.txt",
            "NameSuffixes.txt",
            "NameTitles.txt"
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

    // Shamelessly stolen from the SalemModLoader code base
    public class FromAssetBundle
    {
        public static AssetBundle GetAssetBundleFromResources(string filename, Assembly execAssembly)
        {
            if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix)
            {
                filename += "_mac";
                Console.WriteLine("MAC ASSETBUNDLE IN USE!");
            }

            string name = execAssembly.GetManifestResourceNames().Single((string str) => str.EndsWith(filename));
            using Stream stream = execAssembly.GetManifestResourceStream(name);
            return AssetBundle.LoadFromStream(stream);
        }

        public static Material LoadMaterial(string bundleName, string material)
        {
            AssetBundle assetBundleFromResources = GetAssetBundleFromResources(bundleName, Assembly.GetCallingAssembly());
            Material result = assetBundleFromResources.LoadAsset<Material>(material);
            if (assetBundleFromResources != null)
            {
                assetBundleFromResources.Unload(false);
            }

            return result;
        }

        public static Sprite LoadSprite(string bundleName, string sprite)
        {
            AssetBundle assetBundleFromResources = GetAssetBundleFromResources(bundleName, Assembly.GetCallingAssembly());
            Sprite result = assetBundleFromResources.LoadAsset<Sprite>(sprite);
            if (assetBundleFromResources != null)
            {
                assetBundleFromResources.Unload(false);
            }

            return result;
        }

        public static GameObject LoadGameObject(string bundleName, string obj)
        {
            AssetBundle assetBundleFromResources = GetAssetBundleFromResources(bundleName, Assembly.GetCallingAssembly());
            GameObject result = assetBundleFromResources.LoadAsset<GameObject>(obj);
            if (assetBundleFromResources != null)
            {
                assetBundleFromResources.Unload(false);
            }

            return result;
        }

        public static T LoadAsset<T>(string bundleName, string asset) where T : UnityEngine.Object
        {
            AssetBundle assetBundleFromResources = GetAssetBundleFromResources(bundleName, Assembly.GetCallingAssembly());
            T result = assetBundleFromResources.LoadAsset<T>(asset);
            if (assetBundleFromResources != null)
            {
                assetBundleFromResources.Unload(false);
            }

            return result;
        }
    }
}