using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unfoundry
{
    public static class TextEntryFrame
    {
        public delegate void ConfirmDelegate(string text);
        public delegate void CancelDelegate();
        private static DroneTransportSetNameFrame textEntryFrame;
        private static ConfirmDelegate onConfirm = null;
        private static CancelDelegate onCancel = null;

        private static FieldInfo relatedEntityId = typeof(DroneTransportSetNameFrame).GetField("relatedEntityId", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo firstFrame = typeof(DroneTransportSetNameFrame).GetField("firstFrame", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Show(string title, string text, string confirmButtonText, ConfirmDelegate onConfirm, CancelDelegate onCancel = null)
        {
            if (textEntryFrame != null) Object.Destroy(textEntryFrame);

            textEntryFrame = Object.Instantiate(ResourceDB.ui_droneStationSetNameFrame, GlobalStateManager.getDefaultUICanvasTransform(true), false).GetComponent<DroneTransportSetNameFrame>();
            textEntryFrame.inputField_stationName.tmp_inputField.text = text;
            relatedEntityId.SetValue(textEntryFrame, (ulong)0);
            TextEntryFrame.onConfirm = onConfirm;
            TextEntryFrame.onCancel = onCancel;

            var headerBarTransform = textEntryFrame.transform.Find("HeaderBar");
            if (headerBarTransform != null)
            {
                var headerBar = headerBarTransform.GetComponent<UIHeaderBar>();
                if (headerBar != null)
                {
                    headerBar.setText(title);
                }
                else
                {
                    Debug.LogWarning($"Failed to find UIHeaderBar on {textEntryFrame.name}/{headerBarTransform.name}");
                }
            }
            else
            {
                Debug.LogWarning($"Failed to find HeaderBar on {textEntryFrame.name}");
            }

            textEntryFrame.uiText_buttonText.text = confirmButtonText;

            AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIOpen);
        }


        [HarmonyPatch]
        public static class Patch
        {
            [HarmonyPatch(typeof(DroneTransportSetNameFrame), nameof(DroneTransportSetNameFrame.showFrame))]
            [HarmonyPrefix]
            private static void DroneTransportSetNameFrame_showFrame()
            {
                onConfirm = null;
                onCancel = null;
            }

            [HarmonyPatch(typeof(DroneTransportSetNameFrame), "Update")]
            [HarmonyPrefix]
            private static bool DroneTransportSetNameFrame_Update(DroneTransportSetNameFrame __instance)
            {
                if ((ulong)relatedEntityId.GetValue(__instance) != 0) return true;

                if ((bool)firstFrame.GetValue(__instance))
                {
                    __instance.inputField_stationName.gameObject.GetComponent<Selectable>().select_advanced(EventSystem.current);
                    firstFrame.SetValue(__instance, false);
                }

                return false;
            }

            [HarmonyPatch(typeof(DroneTransportSetNameFrame), nameof(DroneTransportSetNameFrame.onClick_save))]
            [HarmonyPrefix]
            private static bool DroneTransportSetNameFrame_onClick_save(DroneTransportSetNameFrame __instance)
            {
                if ((ulong)relatedEntityId.GetValue(__instance) != 0) return true;

                var text = textEntryFrame.inputField_stationName.tmp_inputField.text;

                Object.Destroy(__instance.gameObject);

                onConfirm?.Invoke(text);
                onConfirm = null;
                onCancel = null;

                AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIClose);

                return false;
            }

            [HarmonyPatch(typeof(DroneTransportSetNameFrame), nameof(DroneTransportSetNameFrame.onClick_close))]
            [HarmonyPrefix]
            private static bool DroneTransportSetNameFrame_onClick_close(DroneTransportSetNameFrame __instance)
            {
                if ((ulong)relatedEntityId.GetValue(__instance) != 0) return true;

                Object.Destroy(__instance.gameObject);

                onCancel?.Invoke();
                onConfirm = null;
                onCancel = null;

                AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIClose);

                return false;
            }
        }
    }
}
