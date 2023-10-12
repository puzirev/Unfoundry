using System.Collections.Generic;
using System.IO;
using Unfoundry;
using UnityEngine;

namespace Duplicationer
{
    public class LazySprite
    {
        private string assetPath;
        Dictionary<string, Object> assetBundle;

        private Sprite sprite = null;

        public LazySprite(Dictionary<string, Object> assetBundle, string assetPath)
        {
            this.assetPath = assetPath;
            this.assetBundle = assetBundle;
        }

        public Sprite Sprite
        {
            get
            {
                if (sprite != null) return sprite;
                if (assetBundle == null) throw new System.ArgumentNullException(nameof(assetBundle));
                sprite = assetBundle.LoadAsset<Sprite>(assetPath);
                if (sprite == null) throw new FileLoadException(assetPath);
                return sprite;
            }
        }
    }
}
