using UnityEngine;
using static Unfoundry.Plugin;

namespace Unfoundry
{
    public class LazyIconSprite
    {
        private Sprite sprite = null;
        private string iconName;
        private AssetBundleProxy bundleMain = null;

        public LazyIconSprite(string iconName)
        {
            this.iconName = iconName;
        }

        public LazyIconSprite(AssetBundleProxy bundleMain, string iconName)
        {
            this.bundleMain = bundleMain;
            this.iconName = iconName;
        }

        private Sprite FetchSprite()
        {
            if (bundleMain == null)
            {
                sprite = ResourceDB.getIcon(iconName, 0);
                if (sprite == null) log.LogWarning((string)$"Failed to find icon '{iconName}'");

                return sprite;
            }
            else
            {
                sprite = bundleMain.LoadAsset<Sprite>(iconName);
                if (sprite == null) log.LogWarning((string)$"Failed to find icon '{iconName}'");

                return sprite;
            }
        }

        public Sprite Sprite => sprite == null ? FetchSprite() : sprite;
    }
}
