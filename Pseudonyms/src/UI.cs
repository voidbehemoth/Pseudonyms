using Game.Interface;
using HarmonyLib;
using Home.HomeScene;
using SalemModLoaderUI;
using SML;
using UnityEngine;
using UnityEngine.UI;

namespace Pseudonyms.UI
{
    public class PseudonymsUI
    {

        private void CacheObjects(PickNamesPanel __instance)
        {
            // __instance.rerollButton = 
        }
    }

    /*[HarmonyPatch(typeof(PickNamesPanel), "Start")]
    public class PseudonymsPickNamesPanelPatch
    {
        [HarmonyPostfix]
        public static void Postfix(PickNamesPanel __instance)
        {
            GameObject gameObject = Object.Instantiate(FromAssetBundle.LoadGameObject("Pseudonyms.resources.assetbundles.pseudonyms", "PseudonymsPickNamesUIPanel"));
        }

        [HarmonyPatch(typeof(HomeSceneController), "Start")]
        public class SalemModLoaderHomeScenePatch
        {
            [HarmonyPostfix]
            public static void Postfix(HomeSceneController __instance)
            {
                GameObject gameObject = Object.Instantiate(FromAssetBundle.LoadGameObject("SalemModLoader.resources.assetbundles.salemmodloader", "SalemModLoaderMainUIPanel"));
                gameObject.name = "SalemModLoaderUI";
                gameObject.transform.SetParent(__instance.SafeArea.transform);
                gameObject.transform.SetAsLastSibling();
                gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
                gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                gameObject.AddComponent<SalemModLoaderMainMenuController>();
            }
        }
    }*/
}
