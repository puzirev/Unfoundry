using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace Unfoundry
{
    public static class InputHelpers
    {
        public static bool IsShiftHeld => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        public static bool IsControlHeld => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        public static bool IsAltHeld => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        public static bool IsKeyboardInputAllowed => !GlobalStateManager.IsInputFieldFocused() && !(Traverse.Create(typeof(EscapeMenu)).Field("singleton").GetValue<EscapeMenu>() != null && Traverse.Create(typeof(EscapeMenu)).Field("singleton").GetValue<EscapeMenu>().enabled);
        public static bool IsMouseInputAllowed => !GlobalStateManager.isCursorOverUIElement() && !(Traverse.Create(typeof(EscapeMenu)).Field("singleton").GetValue<EscapeMenu>() != null && Traverse.Create(typeof(EscapeMenu)).Field("singleton").GetValue<EscapeMenu>().enabled);

        public static KeyCode ParseKeyCode(string keyName, KeyCode defaultKeyCode)
        {
            try
            {
                return (KeyCode)Enum.Parse(typeof(KeyCode), keyName, true);
            }
            catch (ArgumentException)
            {
                return defaultKeyCode;
            }
        }

        private static readonly FieldInfo _idField = typeof(Rewired.Player).Assembly.GetType("qZKXuuGyixNLZyXOGAGWHRSrJhcH").GetField("nZidcytkGLdDfkBquTBAErtFyMxj", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _mapperField = typeof(Rewired.Player).GetField("VyTQlKbBAXhbvKnCpPDJCYQVMTSFA", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo _mapperMethod = typeof(Rewired.Player).Assembly.GetType("HBcqQbvzirjxijYaryIcymzDDCv").GetMethod("whBoJUATYkeVKaCEAYNhhmQmFNw", new Type[] { typeof(int), typeof(string), typeof(bool) });
        public static int GetActionId(string actionName)
        {
            if (_mapperField == null)
            {
                Debug.LogWarning($"Unfoundry: No mapper field found");
                return -1;
            }
            if (_mapperMethod == null)
            {
                Debug.LogWarning($"Unfoundry: No mapper method found");
                return -1;
            }
            if (_idField == null)
            {
                Debug.LogWarning($"Unfoundry: No id field found");
                return -1;
            }

            var player = GlobalStateManager.getRewiredPlayer0();
            if (player == null) return -1;

            var id = player.id;

            var mapper = _mapperField.GetValue(player);
            if (mapper == null)
            {
                Debug.LogWarning($"Unfoundry: No mapper found for player {id}");
                return -1;
            }

            var action = _mapperMethod.Invoke(mapper, new object[] { id, actionName, true });
            if (action == null)
            {
                Debug.LogWarning($"Unfoundry: No action found for player {id}, action '{actionName}'");
                return -1;
            }

            return (int)_idField.GetValue(action);
        }
    }
}
