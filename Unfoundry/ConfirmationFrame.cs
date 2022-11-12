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
            confirmDestroyFrame.itemTemplateToDestroyId = 0;
            ConfirmationFrame.onConfirm = onConfirm;
            ConfirmationFrame.onCancel = onCancel;

            Vector2 panelSize = confirmDestroyFrame.GetComponent<RectTransform>().sizeDelta;
            var targetPos = CursorManager.singleton.mousePosition;
            targetPos.x = Mathf.Clamp(targetPos.x, panelSize.x * 0.6f, Screen.width - panelSize.x * 0.6f);
            targetPos.y = Mathf.Clamp(targetPos.y, panelSize.y * 0.6f, Screen.height - panelSize.y * 0.6f);

            confirmDestroyFrame.transform.position = targetPos;
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
                if (__instance.itemTemplateToDestroyId != 0 || onConfirm == null) return true;

                onConfirm.Invoke();
                onConfirm = onCancel = null;

                Object.Destroy(__instance.gameObject);

                return false;
            }

            [HarmonyPatch(typeof(DestroyItemConfirmationFrame), nameof(DestroyItemConfirmationFrame.cancelOnClick))]
            [HarmonyPrefix]
            private static bool DestroyItemConfirmationFrame_cancelOnClick(DestroyItemConfirmationFrame __instance)
            {
                if (__instance.itemTemplateToDestroyId != 0 || onCancel == null) return true;

                onCancel.Invoke();
                onConfirm = onCancel = null;

                Object.Destroy(__instance.gameObject);

                return false;
            }
        }
    }
}
