using HarmonyLib;

namespace Unfoundry
{
    public class CommonEvents
    {
        public delegate void GameInitializationDoneDelegate();
        public static event GameInitializationDoneDelegate OnGameInitializationDone;

        public delegate void ApplicationStartDelegate();
        public static event ApplicationStartDelegate OnApplicationStart;

        public delegate void UpdateDelegate();
        public static event UpdateDelegate OnUpdate;

        public delegate void LateUpdateDelegate();
        public static event LateUpdateDelegate OnLateUpdate;

        public delegate void RotateYDelegate(CancellableEventArgs eventArgs);
        public static event RotateYDelegate OnRotateY;

        public delegate void DeselectToolDelegate();
        public static event DeselectToolDelegate OnDeselectTool;


        [HarmonyPatch]
        public static class Patch
        {
            [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.OnGameInitializationDone))]
            [HarmonyPostfix]
            private static void GameCamera_OnGameInitializationDone()
            {
                ActionManager.OnGameInitializationDone();
                OnGameInitializationDone?.Invoke();
            }

            [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.Update))]
            [HarmonyPrefix]
            private static void Update()
            {
                OnUpdate?.Invoke();
                ActionManager.Update();
            }

            [HarmonyPatch(typeof(GameRoot), "LateUpdate")]
            [HarmonyPrefix]
            private static void LateUpdate()
            {
                OnLateUpdate?.Invoke();
                ActionManager.Update();
            }

            [HarmonyPatch(typeof(ResourceDB), nameof(ResourceDB.InitOnApplicationStart))]
            [HarmonyPostfix]
            private static void ResourceDB_InitOnApplicationStart()
            {
                OnApplicationStart?.Invoke();
            }

            [HarmonyPatch(typeof(Character.ClientData), nameof(Character.ClientData.deselect))]
            [HarmonyPrefix]
            private static void ClientData_deselect()
            {
                OnDeselectTool?.Invoke();
            }

            [HarmonyPatch(typeof(GameRoot), "keyHandler_rotateY")]
            [HarmonyPrefix]
            private static bool GameRoot_keyHandler_rotateY()
            {
                if (CustomHandheldModeManager.IsCustomHandheldModeActive)
                {
                    var result = CustomHandheldModeManager.OnRotateY();
                    if (!result) return false;
                }

                var eventArgs = new CancellableEventArgs();
                OnRotateY?.Invoke(eventArgs);
                if (eventArgs.Cancel) return false;

                return true;
            }
        }
    }
}
