using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using C3.ModKit;
using C3;
using System.Linq;

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
            VERSION = "0.3.14";

        private static readonly Dictionary<string, UnfoundryPlugin> _unfoundryPlugins = new Dictionary<string, UnfoundryPlugin>();
        private static Config _config = null;
        private static TypedConfigEntry<int> _configMaxQueuedEventsPerFrame = null;

        internal static Dictionary<System.Type, Dictionary<string, UnityEngine.Object>> bundleMainAssets;

        private static string[] _texturesToRegister = new string[]
        {
            "assembler_iii",
            "biomass",
            "corner_cut",
            "corner_cut_fully_inset",
            "corner_cut_outline",
            "cross",
            "dirt",
            "download",
            "floor",
            "icons8-chevron-left-filled-100_white",
            "icons8-error-100",
            "icons8-info-512",
            "solid_square_white",
            "upload",
            "checkmark_208x208",
            "icon_plus",
            "icon_minus"
        };

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
        private static readonly Dictionary<ulong, HandheldData> handheldData = new Dictionary<ulong, HandheldData>();

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

        public static T GetAsset<T>(string name) where T : UnityEngine.Object
        {
            if (!bundleMainAssets.TryGetValue(typeof(T), out var assetDict))
            {
                Debug.Log($"Missing asset dictionary for type '{typeof(T)}'");
                return null;
            }

            if (!assetDict.TryGetValue(name, out var asset))
            {
                Debug.Log($"Missing asset with '{name}' and type '{typeof(T)}'");
                return null;
            }

            return (T)asset;
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
                bool assetsFound = false;
                foreach (var mod in allMods)
                {
                    if (mod.modInfo.identifier == "erkle64.unfoundry")
                    {
                        Debug.Log("Unfoundry loading common assets.");
                        bundleMainAssets = new Dictionary<System.Type, Dictionary<string, Object>>();
                        foreach (KeyValuePair<AssetBundle, global::UnityEngine.Object[]> keyValuePair in AssetManager.getAllLoadedAssetBundles())
                        {
                            if (!mod.modInfo.assetBundles.Any(s => keyValuePair.Key.name.Equals(s)))
                                continue;
                            global::UnityEngine.Object[] value = keyValuePair.Value;
                            for (int i = 0; i < value.Length; i++)
                            {
                                Texture2D texture2D = value[i] as Texture2D;
                                Debug.Log(string.Format("Unfoundry found {0} asset {1}, index={2}", texture2D == null ? "non-texture" : "texture", value[i], i));
                                if (!bundleMainAssets.TryGetValue(value[i].GetType(), out var assetDict))
                                {
                                    assetDict = new Dictionary<string, Object>();
                                    bundleMainAssets.Add(value[i].GetType(), assetDict);
                                }

                                if (assetDict.TryGetValue(value[i].name, out var currentVal))
                                    Debug.Log($"Unfoundry already have registered asset with name '{value[i].name}' and type '{value[i].GetType()}': '{currentVal}'");
                                else
                                    assetDict.Add(value[i].name, value[i]);
                                if (texture2D != null)
                                {
                                    if (_texturesToRegister.Any(s => texture2D.name.Equals(s)))
                                    {
                                        Debug.Log(string.Format("Unfoundry registering texture {0}", texture2D));
                                        ResourceExt.RegisterTexture(texture2D.name, texture2D);
                                    }
                                }
                            }
                        }

                        assetsFound = true;

                        break;
                    }
                }
                if (!assetsFound)
                {
                    Debug.Log("Unfoundry failed to load common assets.");
                }

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
                if (__instance == null) return;
                if (!(GameRoot.getClientCharacter() is Character character)) return;
                if (__instance.characterHash != character.usernameHash) return;
                lastSpawnedBuildableWrapperEntityId = 0;
            }

            [HarmonyPatch(typeof(BuildEntityEvent), nameof(BuildEntityEvent.processEvent))]
            [HarmonyPostfix]
            private static void BuildEntityEvent_processEvent_postfix(BuildEntityEvent __instance)
            {
                if (__instance == null) return;
                if (!(GameRoot.getClientCharacter() is Character character)) return;
                if (__instance.characterHash != character.usernameHash) return;
                ActionManager.InvokeAndRemoveBuildEvent(__instance, lastSpawnedBuildableWrapperEntityId);
            }

            [HarmonyPatch(typeof(BuildingManager), nameof(BuildingManager.buildingManager_constructBuildableWrapper))]
            [HarmonyPostfix]
            private static void BuildingManager_buildingManager_constructBuildableWrapper(ulong __result)
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

                    ChunkManager.getChunkIdxAndTerrainArrayIdxFromWorldCoords(__instance.worldPos.x, __instance.worldPos.y, __instance.worldPos.z, out ulong chunkIndex, out uint blockIndex);

                    byte terrainType = 0;
                    ChunkManager.chunks_removeTerrainBlock(chunkIndex, blockIndex, ref terrainType);
                    ChunkManager.flagChunkVisualsAsDirty(chunkIndex, true, true);
                    return false;
                }

                return true;
            }
        }
    }
}
