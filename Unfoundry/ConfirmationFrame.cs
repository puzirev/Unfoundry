using HarmonyLib;
using UnityEngine;

namespace Unfoundry
{
    public static class ConfirmationFrame
    {
        public delegate void ConfirmDestroyDelegate();
        private static DestroyItemConfirmationFrame confirmDestroyFrame;
        private static ConfirmDestroyDelegate onConfirm = null;
        private static ConfirmDestroyDelegate onCancel = null;

        public static void Show(string text, ConfirmDestroyDelegate onConfirm, ConfirmDestroyDelegate onCancel = null)
        {
            if (confirmDestroyFrame != null) Object.Destroy(confirmDestroyFrame);

            confirmDestroyFrame = Object.Instantiate(ResourceDB.ui_destroyItemConfirmation, GlobalStateManager.getDefaultUICanvasTransform(true), false).GetComponent<DestroyItemConfirmationFrame>();
            confirmDestroyFrame.uiText_message.setText(text);
            Traverse.Create(confirmDestroyFrame).Field("itemTemplateToDestroyId").SetValue((ulong)0);
            ConfirmationFrame.onConfirm = onConfirm;
            ConfirmationFrame.onCancel = onCancel;

            AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIOpen);
        }

        public static void Show(string text, string confirmButtonText, ConfirmDestroyDelegate onConfirm, ConfirmDestroyDelegate onCancel = null)
        {
            if (confirmDestroyFrame != null) Object.Destroy(confirmDestroyFrame);

            confirmDestroyFrame = Object.Instantiate(ResourceDB.ui_destroyItemConfirmation, GlobalStateManager.getDefaultUICanvasTransform(true), false).GetComponent<DestroyItemConfirmationFrame>();
            confirmDestroyFrame.uiText_message.setText(text);
            Traverse.Create(confirmDestroyFrame).Field("itemTemplateToDestroyId").SetValue((ulong)0);
            ConfirmationFrame.onConfirm = onConfirm;
            ConfirmationFrame.onCancel = onCancel;

            foreach (var button in confirmDestroyFrame.GetComponentsInChildren<UIButton>())
            {
                if (button.tmp_text.text == "Destroy") button.tmp_text.text = confirmButtonText;
            }

            AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIOpen);
        }


        [HarmonyPatch]
        public static class Patch
        {
            [HarmonyPatch(typeof(DestroyItemConfirmationFrame), nameof(DestroyItemConfirmationFrame.createFrame))]
            [HarmonyPrefix]
            private static void DestroyItemConfirmationFrame_createFrame()
            {
                onConfirm = onCancel = null;
            }

            [HarmonyPatch(typeof(DestroyItemConfirmationFrame), nameof(DestroyItemConfirmationFrame.destroyOnClick))]
            [HarmonyPrefix]
            private static bool DestroyItemConfirmationFrame_destroyOnClick(DestroyItemConfirmationFrame __instance)
            {
                if (Traverse.Create(__instance).Field("itemTemplateToDestroyId").GetValue<ulong>() != 0) return true;

                onConfirm?.Invoke();
                onConfirm = onCancel = null;

                Object.Destroy(__instance.gameObject);

                AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIClose);

                return false;
            }

            [HarmonyPatch(typeof(DestroyItemConfirmationFrame), nameof(DestroyItemConfirmationFrame.cancelOnClick))]
            [HarmonyPrefix]
            private static bool DestroyItemConfirmationFrame_cancelOnClick(DestroyItemConfirmationFrame __instance)
            {
                if (Traverse.Create(__instance).Field("itemTemplateToDestroyId").GetValue<ulong>() != 0) return true;

                onCancel?.Invoke();
                onConfirm = onCancel = null;

                Object.Destroy(__instance.gameObject);

                AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIClose);

                return false;
            }
        }
    }
}
