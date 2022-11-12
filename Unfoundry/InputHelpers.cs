using System;
using UnityEngine;

namespace Unfoundry
{
    public static class InputHelpers
    {
        public static bool IsShiftHeld => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        public static bool IsControlHeld => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        public static bool IsAltHeld => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        public static bool IsKeyboardInputAllowed => !GlobalStateManager.IsInputFieldFocused() && !(EscapeMenu.singleton != null && EscapeMenu.singleton.enabled);
        public static bool IsMouseInputAllowed => !GlobalStateManager.isCursorOverUIElement() && !(EscapeMenu.singleton != null && EscapeMenu.singleton.enabled);

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
    }
}
