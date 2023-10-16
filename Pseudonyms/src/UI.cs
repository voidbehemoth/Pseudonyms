using Game.Interface;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Utils;

namespace Pseudonyms.UI
{
    [HarmonyPatch(typeof(PickNamesPanel), "Start")]
    public class PseudonymsPickNamesPanelPatch
    {
        public static PickNamesPanel INSTANCE;

        [HarmonyPostfix]
        public static void Postfix(PickNamesPanel __instance)
        {
            Utils.Logger.Log("Adding PickNamesPlus");
            INSTANCE = __instance;
            __instance.gameObject.AddComponent<PickNamesPlus>();
        }
    }

    public class PickNamesPlus : MonoBehaviour
    {

        public Button rerollNameButton;

        public void Awake()
        {
            Setup();
            rerollNameButton.onClick.AddListener(RerollNameButtonClicked);
        }

        private void Setup()
        {
            Button SubmitButton = PseudonymsPickNamesPanelPatch.INSTANCE.submitButton;
            rerollNameButton = Instantiate(Main.RerollNameButton, SubmitButton.gameObject.transform.parent).GetComponent<Button>();
            rerollNameButton.transform.SetAsLastSibling();
            rerollNameButton.gameObject.transform.localPosition = new Vector3(500f, 0f, 0f);
            rerollNameButton.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            Utils.Logger.Log("Added button");
        }

        public void RerollNameButtonClicked()
        {
            PickNamesPanel pickNamesPanel = gameObject.GetComponentInParent<PickNamesPanel>();

            Utils.NameHelper.SetRandomName();
            Utils.Logger.Log("Button clicked!");

            string @string = Storage.GetString(Storage.Key.GameName, "");
            pickNamesPanel.nameInput.text = @string;
        }
    }
}
