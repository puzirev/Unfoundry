using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using C3.ModKit;
using C3;

namespace Unfoundry
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class UnfoundryModAttribute : System.Attribute
    {
        public string modIdentifier;

        public UnfoundryModAttribute(string modIdentifier)
        {
            this.modIdentifier = modIdentifier;
        }
    }

    [NoStrip]
    public class Plugin : AssemblyProcessor
    {
        public const string
            MODNAME = "Unfoundry",
            AUTHOR = "erkle64",
            GUID = AUTHOR + "." + MODNAME,
            VERSION = "0.3.2";

        private static Dictionary<string, UnfoundryPlugin> _unfoundryPlugins = new Dictionary<string, UnfoundryPlugin>();
        private static Config _config = null;
        private static TypedConfigEntry<int> _configMaxQueuedEventsPerFrame = null;

        public override void ProcessAssembly(Assembly assembly, System.Type[] types)
        {
            LoadConfig();

            foreach (System.Type type in assembly.GetTypes())
            {
                var attributes = (UnfoundryModAttribute[])type.GetCustomAttributes(typeof(UnfoundryModAttribute), true);
                if (typeof(UnfoundryPlugin).IsAssignableFrom(type) && attributes.Length > 0)
                {
                    var plugin = (UnfoundryPlugin)System.Activator.CreateInstance(type);
                    if (plugin != null)
                    {
                        Debug.Log($"Unfoundry instantiating plugin for {type.FullName}");
                        _unfoundryPlugins.Add(attributes[0].modIdentifier, plugin);
                    }
                }
            }
        }

        private static void LoadConfig()
        {
            if (_config != null) return;

            _config = new Config(GUID)
                .Group("Action Manager")
                    .Entry(out _configMaxQueuedEventsPerFrame, "Max Queued Events Per Frame", 40)
                .EndGroup()
                .Load()
                .Save();
        }

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
        
        public static Vector3 SnappedToNearestAxis(Vector3 direction)
        {
            float num1 = Mathf.Abs(direction.x);
            float num2 = Mathf.Abs(direction.y);
            float num3 = Mathf.Abs(direction.z);
            if ((double)num1 > (double)num2 && (double)num1 > (double)num3)
                return new Vector3(Mathf.Sign(direction.x), 0.0f, 0.0f);
            return (double)num2 > (double)num1 && (double)num2 > (double)num3 ? new Vector3(0.0f, Mathf.Sign(direction.y), 0.0f) : new Vector3(0.0f, 0.0f, Mathf.Sign(direction.z));
        }

        [HarmonyPatch]
        public static class Patch
        {
            [HarmonyPatch(typeof(ObjectPoolManager), nameof(ObjectPoolManager.InitOnApplicationStart))]
            [HarmonyPrefix]
            private static void LoadPlugin()
            {
                ActionManager.MaxQueuedEventsPerFrame = _configMaxQueuedEventsPerFrame?.Get() ?? 40;

                var allMods = typeof(ModManager).GetField("mods", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as List<Mod>;
                foreach (var plugin in _unfoundryPlugins)
                {
                    var modFound = false;
                    foreach (var mod in allMods)
                    {
                        if (mod.modInfo.identifier == plugin.Key)
                        {
                            Debug.Log($"Unfoundry loading mod '{plugin.Key}'");
                            plugin.Value.Load(mod);
                            modFound = true;
                            break;
                        }
                    }
                    if (!modFound)
                    {
                        Debug.Log($"Unfoundry failed to find mod '{plugin.Key}'");
                    }
                }
            }

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
                    ChunkManager.flagChunkVisualsAsDirty(chunkIndex, true, true);
                    return false;
                }

                return true;
            }

            //[HarmonyPatch(typeof(GameRoot), nameof(GameRoot.addLockstepEvent))]
            //[HarmonyPostfix]
            //private static void GameRoot_addLockstepEvent(GameRoot.LockstepEvent e)
            //{
            //    Debug.Log("====== GameRoot.addLockstepEvent ======");
            //    Debug.Log(e.getDbgInfo());
            //}
        }
    }
}
