using System.Collections.Generic;
using UnityEngine;
using static Unfoundry.Plugin;

namespace Unfoundry
{
    public class LazyIconSprite
    {
        private Sprite sprite = null;
        private string iconName;
        private Dictionary<string, Object> bundleMain = null;

        public LazyIconSprite(string iconName)
        {
            this.iconName = iconName;
        }

        public LazyIconSprite(Dictionary<string, Object> bundleMain, string iconName)
        {
            this.bundleMain = bundleMain;
            this.iconName = iconName;
        }

        private Sprite FetchSprite()
        {
            if (bundleMain == null)
            {
                sprite = ResourceDB.getIcon(iconName, 0);
                if (sprite == null) Debug.LogWarning((string)$"Failed to find icon '{iconName}'");

                return sprite;
            }
            else
            {
                sprite = bundleMain.LoadAsset<Sprite>(iconName);
                if (sprite == null) Debug.LogWarning((string)$"Failed to find icon '{iconName}'");

                return sprite;
            }
        }

        public Sprite Sprite => sprite == null ? FetchSprite() : sprite;
    }
}
