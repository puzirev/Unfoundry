using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using static Unfoundry.Plugin;

namespace Unfoundry
{
    public static class ResourceExt
    {
        static Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();
        static Texture2D[] allTextures1 = null;
        static Texture2D[] allTextures2 = null;

        private static Dictionary<int, int> iconSizes = new Dictionary<int, int>() {
            { 0, 1024 },
            { 512, 512 },
            { 256, 256 },
            { 128, 128 },
            { 96, 96 },
            { 64, 64 }
        };

        public static void RegisterTexture(string name, Texture2D texture)
        {
            loadedTextures[name] = texture;
        }

        public static Sprite CreateSprite(Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        }

        public static Texture2D ResizeTexture(Texture2D inputTexture, int width, int height)
        {
            return TextureScale.Bilinear(inputTexture, width, height);
        }

        public static T LoadAsset<T>(this Dictionary<string, UnityEngine.Object> bundle, string identifier) where T : UnityEngine.Object
        {
            if (!bundle.TryGetValue(identifier, out var asset))
            {
                Debug.Log($"Missing asset '{identifier}'");
                return null;
            }

            return (T)asset;
        }

        public static Sprite LoadIcon(Dictionary<string, UnityEngine.Object> bundle, string identifier)
        {
            var originalSprite = bundle.LoadAsset<Sprite>(identifier);
            Sprite mainSprite = null;

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            var iconTexture = originalSprite.texture;
            int index = 0;
            foreach (var entry in iconSizes)
            {
                var sizeId = entry.Key;
                var size = entry.Value;
                var sizeIdentifier = identifier + ((sizeId > 0) ? "_" + sizeId.ToString() : "");
                var texture = (sizeId > 0) ? ResizeTexture(iconTexture, size, size) : iconTexture;
                texture.name = sizeIdentifier;
                var sprite = CreateSprite(texture);
                (Traverse.Create(typeof(ResourceDB)).Field("dict_icons").GetValue<Dictionary<int, Dictionary<ulong, Sprite>>>())[sizeId][GameRoot.generateStringHash64(sizeIdentifier)] = sprite;
                if (sizeId == 0) mainSprite = sprite;

                ++index;
            }

            watch.Stop();
            Debug.Log((string)$"Loading icon '{identifier}' from asset bundle took {watch.ElapsedMilliseconds}ms");

            return mainSprite;
        }

        public static Sprite LoadIcon(string identifier, string iconFolderPath)
        {
            string iconPath = Path.Combine(iconFolderPath, identifier);
            Sprite mainSprite = null;

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            var iconTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            iconTexture.LoadImage(File.ReadAllBytes(iconPath), true);
            int index = 0;
            foreach (var entry in iconSizes)
            {
                var sizeId = entry.Key;
                var size = entry.Value;
                var sizeIdentifier = identifier + ((sizeId > 0) ? "_" + sizeId.ToString() : "");
                var texture = (sizeId > 0) ? ResizeTexture(iconTexture, size, size) : iconTexture;
                texture.name = sizeIdentifier;
                var sprite = CreateSprite(texture);
                (Traverse.Create(typeof(ResourceDB)).Field("dict_icons").GetValue<Dictionary<int, Dictionary<ulong, Sprite>>>())[sizeId][GameRoot.generateStringHash64(sizeIdentifier)] = sprite;
                if(sizeId == 0) mainSprite = sprite;

                ++index;
            }

            watch.Stop();
            Debug.Log((string)$"Loading icon '{identifier}' from '{iconPath}' took {watch.ElapsedMilliseconds}ms");

            return mainSprite;
        }

        public static Sprite LoadIcon(string identifier, byte[] data)
        {
            Sprite mainSprite = null;

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            var iconTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            iconTexture.LoadImage(data, true);
            if (iconTexture == null) return null;

            int index = 0;
            foreach (var entry in iconSizes)
            {
                var sizeId = entry.Key;
                var size = entry.Value;
                var sizeIdentifier = identifier + ((sizeId > 0) ? "_" + sizeId.ToString() : "");
                var texture = (sizeId > 0) ? ResizeTexture(iconTexture, size, size) : iconTexture;
                texture.name = sizeIdentifier;
                var sprite = CreateSprite(texture);
                (Traverse.Create(typeof(ResourceDB)).Field("dict_icons").GetValue<Dictionary<int, Dictionary<ulong, Sprite>>>())[sizeId][GameRoot.generateStringHash64(sizeIdentifier)] = sprite;
                if (sizeId == 0) mainSprite = sprite;

                ++index;
            }

            watch.Stop();
            Debug.Log((string)$"Loading icon '{identifier}' from manifest resource took {watch.ElapsedMilliseconds}ms");

            return mainSprite;
        }

        public static Sprite LoadIcon(string identifier, Stream stream)
        {
            var data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            return LoadIcon(identifier, data);
        }

        //public static Sprite LoadManifestIcon(string identifier)
        //{
        //    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        //    return LoadIcon(identifier.Replace('-', '_'), LoadAsset $""{identifier}.png"));
        //}

        public static Texture2D FindTexture(string name)
        {
            Texture2D result;
            if (loadedTextures.TryGetValue(name, out result)) return result;
            Debug.Log(string.Format("Searching for texture '{0}'", name));

            if (allTextures1 == null) allTextures1 = Resources.FindObjectsOfTypeAll<Texture2D>();
            foreach (Texture2D texture in allTextures1)
            {
                if (texture.name == name)
                {
                    loadedTextures.Add(name, texture);
                    return texture;
                }
            }

            if (allTextures2 == null) allTextures2 = Resources.LoadAll<Texture2D>("");
            foreach (Texture2D texture in allTextures2)
            {
                if (texture.name == name)
                {
                    loadedTextures.Add(name, texture);
                    return texture;
                }
            }

            var icon = ResourceDB.getIcon(name);
            if(icon != null && icon.texture != null)
            {
                loadedTextures.Add(name, icon.texture);
                return icon.texture;
            }

            loadedTextures.Add(name, null);
            Debug.LogError("Could not find texture: " + name);
            return null;
        }


        static System.Collections.Generic.Dictionary<string, TMP_FontAsset> loadedFonts = new System.Collections.Generic.Dictionary<string, TMP_FontAsset>();
        static TMP_FontAsset[] allFonts1 = null;
        static TMP_FontAsset[] allFonts2 = null;

        public static void RegisterFont(string name, TMP_FontAsset font)
        {
            loadedFonts[name] = font;
        }

        public static TMP_FontAsset FindFont(string name)
        {
            TMP_FontAsset result;
            if (loadedFonts.TryGetValue(name, out result))
            {
                return result;
            }
            else
            {
                Debug.Log(string.Format("Searching for font '{0}'", name));

                if (allFonts1 == null) allFonts1 = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                foreach (TMP_FontAsset font in allFonts1)
                {
                    if (font.name == name)
                    {
                        loadedFonts.Add(name, font);
                        return font;
                    }
                }

                if (allFonts2 == null) allFonts2 = Resources.LoadAll<TMP_FontAsset>("");
                foreach (TMP_FontAsset font in allFonts2)
                {
                    if (font.name == name)
                    {
                        loadedFonts.Add(name, font);
                        return font;
                    }
                }

                loadedFonts.Add(name, null);
                Debug.LogError("Could not find font: " + name);
                return null;
            }
        }
    }

    public class TextureScale
    {
        private static Color[] texColors;
        private static Color[] newColors;
        private static int w;
        private static float ratioX;
        private static float ratioY;
        private static int w2;
        private static int finishCount;
        private static Mutex mutex;

        public static Texture2D Point(Texture2D tex, int newWidth, int newHeight)
        {
            return ThreadedScale(tex, newWidth, newHeight, false);
        }

        public static Texture2D Bilinear(Texture2D tex, int newWidth, int newHeight)
        {
            return ThreadedScale(tex, newWidth, newHeight, true);
        }

        private static Texture2D ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear)
        {
            texColors = tex.GetPixels();
            newColors = new Color[newWidth * newHeight];
            if (useBilinear)
            {
                ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
                ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
            }
            else
            {
                ratioX = (float)tex.width / newWidth;
                ratioY = (float)tex.height / newHeight;
            }

            w = tex.width;
            w2 = newWidth;
            int cores = Mathf.Min(SystemInfo.processorCount, newHeight);
            int slice = newHeight / cores;

            finishCount = 0;
            if (mutex == null)
            {
                mutex = new Mutex(false);
            }

            if (cores > 1)
            {
                int i = 0;
                ThreadData threadData;
                for (i = 0; i < cores - 1; i++)
                {
                    threadData = new ThreadData(slice * i, slice * (i + 1));
                    ParameterizedThreadStart
                        ts = useBilinear ? BilinearScale : new ParameterizedThreadStart(PointScale);
                    Thread thread = new Thread(ts);
                    thread.Start(threadData);
                }

                threadData = new ThreadData(slice * i, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }

                while (finishCount < cores)
                {
                    Thread.Sleep(1);
                }
            }
            else
            {
                ThreadData threadData = new ThreadData(0, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
            }

            var rtex = new Texture2D(newWidth, newHeight, tex.format, false);
#pragma warning disable UNT0017 // SetPixels invocation is slow
            rtex.SetPixels(newColors);
#pragma warning restore UNT0017 // SetPixels invocation is slow
            rtex.Apply();

            texColors = null;
            newColors = null;

            return rtex;
        }

        public static void BilinearScale(object obj)
        {
            ThreadData threadData = (ThreadData)obj;
            for (int y = threadData.start; y < threadData.end; y++)
            {
                int yFloor = (int)Mathf.Floor(y * ratioY);
                int y1 = yFloor * w;
                int y2 = (yFloor + 1) * w;
                int yw = y * w2;

                for (int x = 0; x < w2; x++)
                {
                    int xFloor = (int)Mathf.Floor(x * ratioX);
                    float xLerp = (x * ratioX) - xFloor;
                    newColors[yw + x] = ColorLerpUnclamped(
                        ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                        ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
                        (y * ratioY) - yFloor);
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        public static void PointScale(object obj)
        {
            ThreadData threadData = (ThreadData)obj;
            for (int y = threadData.start; y < threadData.end; y++)
            {
                int thisY = (int)(ratioY * y) * w;
                int yw = y * w2;
                for (int x = 0; x < w2; x++)
                {
                    newColors[yw + x] = texColors[(int)(thisY + (ratioX * x))];
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
        {
            return new Color(c1.r + ((c2.r - c1.r) * value),
                c1.g + ((c2.g - c1.g) * value),
                c1.b + ((c2.b - c1.b) * value),
                c1.a + ((c2.a - c1.a) * value));
        }

        public class ThreadData
        {
            public int end;
            public int start;

            public ThreadData(int s, int e)
            {
                this.start = s;
                this.end = e;
            }
        }
    }

}
