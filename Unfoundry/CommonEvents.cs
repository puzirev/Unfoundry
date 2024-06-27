using C3;
using HarmonyLib;

namespace Unfoundry
{
    [AddSystemToGameSimulation]
    public class CommonEventsSystem : SystemManager.System
    {
        [EventHandler]
        public void HandleOnUpdate(OnUpdate _)
        {
            CommonEvents.Update();
        }

        [EventHandler]
        public void HandleOnLateUpdate(OnLateUpdate _)
        {
            CommonEvents.LateUpdate();
        }

        [EventHandler]
        public void HandleOnFixedUpdate(OnFixedUpdate _)
        {
            CommonEvents.FixedUpdate();
        }
    }

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

        public delegate void FixedUpdateDelegate();
        public static event FixedUpdateDelegate OnFixedUpdate;

        public delegate void RotateYDelegate(CancellableEventArgs eventArgs);
        public static event RotateYDelegate OnRotateY;

        public delegate void DeselectToolDelegate();
        public static event DeselectToolDelegate OnDeselectTool;

        internal static void Update()
        {
            OnUpdate?.Invoke();
        }

        internal static void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }

        internal static void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }


        [HarmonyPatch]
        public static class Patch
        {
            [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.OnGameInitializationDone))]
            [HarmonyPostfix]
            private static void GameCamera_OnGameInitializationDone()
            {
                if (CustomRadialMenuManager.radialMenu != null)
                {
                    OnLateUpdate -= CustomRadialMenuManager.radialMenu.Update;
                    CustomRadialMenuManager.radialMenu = null;
                    CustomRadialMenuManager.spriteCenterBackground = null;
                    CustomRadialMenuManager.spriteSectorBackground = null;
                }
                ActionManager.OnGameInitializationDone();
                OnGameInitializationDone?.Invoke();
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
