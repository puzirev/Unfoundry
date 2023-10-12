using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Unfoundry.Plugin;

namespace Unfoundry
{
    public static class CustomHandheldModeManager
    {
        public static bool IsCustomHandheldModeActive { get; private set; } = false;

        public const int FirstCustomIndex = 4;

        private static Dictionary<ulong, HandheldData> handheldData = new Dictionary<ulong, HandheldData>();
        private static List<CustomHandheldMode> customHandheldModes = new List<CustomHandheldMode>();

        public static int RegisterMode(CustomHandheldMode mode)
        {
            Debug.Log($"Registering custom handheld mode #{FirstCustomIndex + customHandheldModes.Count}");

            customHandheldModes.Add(mode);
            return FirstCustomIndex + customHandheldModes.Count - 1;
        }

        public static void ToggleMode(Character character, int modeIndex, int defaultMode = 0)
        {
            if (character is null) throw new ArgumentNullException(nameof(character));

            Character.ClientData clientData = character.clientData;
            HandheldData data = GetHandheldData(character);
            if (data.CurrentlySetMode != modeIndex)
            {
                clientData.setEquipmentMode(modeIndex);
            }
            else
            {
                clientData.setEquipmentMode(defaultMode);
            }
        }

        public static void ExitCurrentMode(ItemTemplate itemTemplate)
        {
            Debug.Log($"Deselecting custom handheld to select {itemTemplate.identifier}");

            ExitCurrentMode(GameRoot.getClientCharacter());
        }
        public static void ExitCurrentMode2()
        {
            Debug.Log($"Deselecting custom handheld");

            ExitCurrentMode(GameRoot.getClientCharacter());
        }
        public static void ExitCurrentMode() => ExitCurrentMode(GameRoot.getClientCharacter());
        public static void ExitCurrentMode(ulong usernameHash) => ExitCurrentMode(CharacterManager.getByUsernameHash(usernameHash));
        public static void ExitCurrentMode(Character character)
        {
            CustomRadialMenuManager.CloseMenu(false);

            if (!IsCustomHandheldModeActive) return;

            IsCustomHandheldModeActive = false;

            if (character is null) throw new ArgumentNullException(nameof(character));

            HandheldData data = GetHandheldData(character.usernameHash);
            if (data.CurrentlySetMode >= FirstCustomIndex)
            {
                customHandheldModes[data.CurrentlySetMode - FirstCustomIndex].Exit();
                data.CurrentlySetMode = 0;
                SetHandheldData(character, data);
            }

            character.clientData.setEquipmentMode(0);
        }

        public static HandheldData GetHandheldData() => GetHandheldData(GameRoot.getClientUsernameHash());
        public static HandheldData GetHandheldData(Character character) => GetHandheldData(character.usernameHash);
        public static HandheldData GetHandheldData(ulong usernameHash)
        {
            HandheldData data;
            if (!handheldData.TryGetValue(usernameHash, out data))
            {
                Debug.Log($"Creating userdata for {usernameHash}");
                handheldData[usernameHash] = data = new HandheldData(0);
            }
            return data;
        }

        public static void SetHandheldData(HandheldData data) => SetHandheldData(GameRoot.getClientUsernameHash(), data);
        public static void SetHandheldData(Character character, HandheldData data) => SetHandheldData(character.usernameHash, data);
        public static void SetHandheldData(ulong usernameHash, HandheldData data)
        {
            handheldData[usernameHash] = data;
        }

        public static bool OnRotateY() => OnRotateY(GameRoot.getClientUsernameHash());
        public static bool OnRotateY(Character character) => OnRotateY(character.usernameHash);
        public static bool OnRotateY(ulong usernameHash)
        {
            var data = handheldData[usernameHash];
            if (data.CurrentlySetMode >= FirstCustomIndex)
            {
                customHandheldModes[data.CurrentlySetMode - FirstCustomIndex].OnRotateY();
                return false;
            }

            return true;
        }

        [HarmonyPatch]
        public static class Patch
        {
            [HarmonyPatch(typeof(HandheldTabletHH), nameof(HandheldTabletHH._updateBehavoir))]
            [HarmonyPrefix]
            private static bool HandheldTabletHH_updateBehavoir(HandheldTabletHH __instance)
            {
                if (!__instance.relatedCharacter.sessionOnly_isClientCharacter) return true;

                int equipmentMode = __instance.relatedCharacter.saveSyncData.getEquipmentMode();
                if (equipmentMode != 0 && Traverse.Create(__instance).Field("currentlySetMode").GetValue<int>() != equipmentMode) return true;

                HandheldData data = GetHandheldData(__instance.relatedCharacter);
                if (data.CurrentlySetMode < FirstCustomIndex) return true;

                int customHandheldModeIndex = data.CurrentlySetMode - FirstCustomIndex;
                if (customHandheldModeIndex < customHandheldModes.Count)
                {
                    var customHandheldMode = customHandheldModes[customHandheldModeIndex];
                    if (Input.GetKeyDown(KeyCode.Mouse1) && InputHelpers.IsMouseInputAllowed)
                    {
                        customHandheldMode.ShowMenu();
                    }
                    if (CustomRadialMenuManager.IsRadialMenuOpen && !Input.GetKey(KeyCode.Mouse1))
                    {
                        CustomRadialMenuManager.CloseMenu();
                    }

                    customHandheldMode.UpdateBehavoir();
                }

                return false;
            }

            [HarmonyPatch(typeof(HandheldTabletHH), nameof(HandheldTabletHH.setTabletMode))]
            [HarmonyPrefix]
            private static void HandheldTabletHH_setTabletMode(HandheldTabletHH __instance, ref int characterEquipmentMode)
            {
                if (!__instance.relatedCharacter.sessionOnly_isClientCharacter) return;

                int newArraySize = customHandheldModes.Count + FirstCustomIndex - 1;
                if (__instance.materialsByMode.Length < newArraySize)
                {
                    var materialsByMode = new Material[newArraySize];
                    var containersByMode = new GameObject[newArraySize];
                    for (int i = 0; i < FirstCustomIndex - 1; i++)
                    {
                        materialsByMode[i] = __instance.materialsByMode[i];
                        containersByMode[i] = __instance.containersByMode[i];
                    }
                    for (int i = FirstCustomIndex - 1; i < newArraySize; i++)
                    {
                        materialsByMode[i] = __instance.materialsByMode[0];
                        containersByMode[i] = __instance.containersByMode[0];
                    }
                    __instance.materialsByMode = materialsByMode;
                    __instance.containersByMode = containersByMode;
                }

                HandheldData data = GetHandheldData(__instance.relatedCharacter);
                if (data.CurrentlySetMode != characterEquipmentMode && data.CurrentlySetMode >= FirstCustomIndex)
                {
                    customHandheldModes[data.CurrentlySetMode - FirstCustomIndex].Exit();
                }

                data.CurrentlySetMode = characterEquipmentMode;
                SetHandheldData(__instance.relatedCharacter, data);

                characterEquipmentMode = (characterEquipmentMode < FirstCustomIndex) ? characterEquipmentMode : 1;
            }

            [HarmonyPatch(typeof(HandheldTabletHH), nameof(HandheldTabletHH.initMode))]
            [HarmonyPrefix]
            private static bool HandheldTabletHH_initMode(HandheldTabletHH __instance)
            {
                if (!__instance.relatedCharacter.sessionOnly_isClientCharacter) return true;

                HandheldData data = GetHandheldData(__instance.relatedCharacter);
                if (data.CurrentlySetMode >= FirstCustomIndex)
                {
                    Traverse.Create(__instance).Field("currentlySetMode").SetValue(1);
                    Traverse.Create(__instance.relatedCharacter.clientData).Field("equipmentMode").SetValue(1);
                    customHandheldModes[data.CurrentlySetMode - FirstCustomIndex].Enter();
                    return false;
                }
                else
                {
                    return true;
                }
            }

            [HarmonyPatch(typeof(Character.ClientData), nameof(Character.ClientData.setEquippedItemTemplate))]
            [HarmonyPrefix]
            private static void ClientData_setEquippedItemTemplate(Character.ClientData __instance, ItemTemplate itemTemplate)
            {
                if (itemTemplate != null) ExitCurrentMode(itemTemplate);
            }

            [HarmonyPatch(typeof(Character.ClientData), nameof(Character.ClientData.setEquipmentMode))]
            [HarmonyPrefix]
            private static bool ClientData_setEquipmentMode(Character.ClientData __instance, int modeToSet)
            {
                if (modeToSet <= 3)
                {
                    ExitCurrentMode();
                    return true;
                }

                IsCustomHandheldModeActive = true;

                __instance.setBuildModeIntoCopyWithSettingsMode(null);
                if (modeToSet != 0 && __instance.getEquippedItemTemplate() != null) __instance.setEquippedItemTemplate(null);
                Traverse.Create(__instance).Field("equipmentMode").SetValue(modeToSet);
                if (modeToSet != 0) GameRoot.characterEquipmentModeChangeUnlockCallback(modeToSet);
                IconShortcutHelper.Refresh();

                var character = GameRoot.getTabletHH().relatedCharacter;
                Debug.Assert(character != null);
                Character.SaveSync_EquipmentMode syncEquipmentMode = new Character.SaveSync_EquipmentMode(character.usernameHash, modeToSet);

                GameRoot.addLockstepEvent(syncEquipmentMode);

                Debug.Log($"Set equipment mode to {modeToSet}");

                return false;
            }
        }
    }
}
