using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using Path = System.IO.Path;
using BepInEx.Configuration;

namespace Unfoundry
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Plugin : BepInEx.IL2CPP.BasePlugin
    {
        public const string
            MODNAME = "Unfoundry",
            AUTHOR = "erkle64",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "0.1.0";

        public static BepInEx.Logging.ManualLogSource log;

        public static readonly string pluginsFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private static ConfigEntry<int> configMaxQueuedEventsPerFrame;

        public struct HandheldData
        {
            public int CurrentlySetMode { get; internal set; }

            public HandheldData(int currentlySetMode)
            {
                CurrentlySetMode = currentlySetMode;
            }
        }
        private static Dictionary<ulong, HandheldData> handheldData = new Dictionary<ulong, HandheldData>();

        private static ulong lastSpawnedBuildableWrapperEntityId = 0;

        public Plugin()
        {
            log = Log;
        }

        public override void Load()
        {
            log.LogMessage((string)$"Loading {MODNAME}");

            configMaxQueuedEventsPerFrame = Config.Bind("Events", "MaxQueuedEventsPerFrame", 20, "");
            ActionManager.MaxQueuedEventsPerFrame = Mathf.Max(1, configMaxQueuedEventsPerFrame.Value);

            try
            {
                var harmony = new Harmony(GUID);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
            }
        }


        [HarmonyPatch]
        public static class Patch
        {
            [HarmonyPatch(typeof(BuildEntityEvent), nameof(BuildEntityEvent.processEvent))]
            [HarmonyPrefix]
            private static void BuildEntityEvent_processEvent_prefix(BuildEntityEvent __instance)
            {
                if (__instance.characterHash != GameRoot.getClientCharacter().usernameHash) return;
                lastSpawnedBuildableWrapperEntityId = 0;
            }

            [HarmonyPatch(typeof(BuildEntityEvent), nameof(BuildEntityEvent.processEvent))]
            [HarmonyPostfix]
            private static void BuildEntityEvent_processEvent_postfix(BuildEntityEvent __instance)
            {
                if (__instance.characterHash != GameRoot.getClientCharacter().usernameHash) return;
                ActionManager.InvokeAndRemoveBuildEvent(__instance, lastSpawnedBuildableWrapperEntityId);
            }

            [HarmonyPatch(typeof(BuildingManager), nameof(BuildingManager.buildingManager_constructBuildableWrapper))]
            [HarmonyPostfix]
            private static void BuildingManager_buildingManager_constructBuildableWrapper(v3i pos, ulong buildableObjectTemplateId, ulong __result)
            {
                lastSpawnedBuildableWrapperEntityId = __result;
            }

            [HarmonyPatch(typeof(Character.DemolishBuildingEvent), nameof(Character.DemolishBuildingEvent.processEvent))]
            [HarmonyPrefix]
            private static bool DemolishBuildingEvent_processEvent(Character.DemolishBuildingEvent __instance)
            {
                if (__instance.clientPlaceholderId == -2)
                {
                    __instance.clientPlaceholderId = 0;
                    BuildingManager.buildingManager_demolishBuildingEntityForDynamite(__instance.entityId);
                    return false;
                }

                return true;
            }

            [HarmonyPatch(typeof(Character.RemoveTerrainEvent), nameof(Character.RemoveTerrainEvent.processEvent))]
            [HarmonyPrefix]
            private static bool RemoveTerrainEvent_processEvent(Character.RemoveTerrainEvent __instance)
            {
                if (__instance.terrainRemovalPlaceholderId == ulong.MaxValue)
                {
                    __instance.terrainRemovalPlaceholderId = 0ul;

                    ulong chunkIndex;
                    uint blockIndex;
                    ChunkManager.getChunkIdxAndTerrainArrayIdxFromWorldCoords(__instance.worldPos.x, __instance.worldPos.y, __instance.worldPos.z, out chunkIndex, out blockIndex);

                    byte terrainType = 0;
                    ChunkManager.chunks_removeTerrainBlock(chunkIndex, blockIndex, ref terrainType);
                    ChunkManager.flagChunkVisualsAsDirty(ChunkManager.getChunkByIdx(chunkIndex), true, true, false);
                    return false;
                }

                return true;
            }

            //[HarmonyPatch(typeof(GameRoot), nameof(GameRoot.addLockstepEvent))]
            //[HarmonyPostfix]
            //private static void GameRoot_addLockstepEvent(GameRoot.LockstepEvent e)
            //{
            //    log.LogMessage("====== GameRoot.addLockstepEvent ======");
            //    log.LogMessage(e.getDbgInfo());
            //}
        }
    }
}
