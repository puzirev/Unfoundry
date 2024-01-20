using C3.ModKit;
using HarmonyLib;
using UnityEngine;

namespace Unfoundry
{
    public abstract class UnfoundryPlugin
    {
        public abstract void Load(Mod mod);
    }
}
