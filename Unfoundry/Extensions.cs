using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Unfoundry
{
    public static class Extensions
    {
        public static bool IsNullOrEmpty(this Array array)
        {
            return array == null || array.Length == 0;
        }

        public static int Sum(this int[] array)
        {
            if (array == null || array.Length == 0) return 0;

            var sum = 0;
            foreach (var item in array) sum += item;

            return sum;
        }

        public static bool IntersectRay(this Bounds bounds, Ray ray, out float distance)
        {
            var boxMin = bounds.min;
            var boxMax = bounds.max;
            var rayOrigin = ray.origin;
            var rayDir = ray.direction;
            var tMin = new Vector3((boxMin.x - rayOrigin.x) / rayDir.x, (boxMin.y - rayOrigin.y) / rayDir.y, (boxMin.z - rayOrigin.z) / rayDir.z);
            var tMax = new Vector3((boxMax.x - rayOrigin.x) / rayDir.x, (boxMax.y - rayOrigin.y) / rayDir.y, (boxMax.z - rayOrigin.z) / rayDir.z);
            var t1 = new Vector3(Mathf.Min(tMin.x, tMax.x), Mathf.Min(tMin.y, tMax.y), Mathf.Min(tMin.z, tMax.z));
            var t2 = new Vector3(Mathf.Max(tMin.x, tMax.x), Mathf.Max(tMin.y, tMax.y), Mathf.Max(tMin.z, tMax.z));
            distance = Mathf.Max(Mathf.Max(t1.x, t1.y), t1.z);
            float tFar = Mathf.Min(Mathf.Min(t2.x, t2.y), t2.z);

            return distance <= tFar;
        }

        /// <summary>
        /// Strip illegal chars and reserved words from a candidate filename (should not include the directory path)
        /// </summary>
        /// <remarks>
        /// http://stackoverflow.com/questions/309485/c-sharp-sanitize-file-name
        /// </remarks>
        public static string CoerceValidFileName(this string filename)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"[{0}]+", invalidChars);

            var reservedWords = new[]
            {
                "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
                "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
                "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            };

            var sanitisedNamePart = Regex.Replace(filename, invalidReStr, "_");
            foreach (var reservedWord in reservedWords)
            {
                var reservedWordPattern = string.Format("^{0}(\\.|$)", reservedWord);
                sanitisedNamePart = Regex.Replace(sanitisedNamePart, reservedWordPattern, "_reservedWord_$1", RegexOptions.IgnoreCase);
            }

            return sanitisedNamePart;
        }

        public static string Escape(this string unescaped)
        {
            var escaped = new StringBuilder(unescaped.Length);
            foreach (var character in unescaped)
            {
                switch (character)
                {
                    case '\0': escaped.Append(@"\0"); break;
                    case '\a': escaped.Append(@"\a"); break;
                    case '\b': escaped.Append(@"\b"); break;
                    case '\f': escaped.Append(@"\f"); break;
                    case '\n': escaped.Append(@"\n"); break;
                    case '\r': escaped.Append(@"\r"); break;
                    case '\t': escaped.Append(@"\t"); break;
                    case '\v': escaped.Append(@"\v"); break;
                    case '\\': escaped.Append(@"\"); break;
                    default: escaped.Append(character); break;
                }
            }

            return escaped.ToString();
        }

        public static string Unescape(this string escaped)
        {
            var unescaped = new StringBuilder(escaped.Length);
            for (int characterIndex = 0; characterIndex < escaped.Length; ++characterIndex)
            {
                var character = escaped[characterIndex];
                if (character == '\\' && characterIndex < escaped.Length - 1)
                {
                    var nextCharacter = escaped[++characterIndex];
                    switch (nextCharacter)
                    {
                        case '0': unescaped.Append('\0'); break;
                        case 'a': unescaped.Append('\a'); break;
                        case 'b': unescaped.Append('\b'); break;
                        case 'f': unescaped.Append('\f'); break;
                        case 'n': unescaped.Append('\n'); break;
                        case 'r': unescaped.Append('\r'); break;
                        case 't': unescaped.Append('\t'); break;
                        case 'v': unescaped.Append('\v'); break;
                        case '\\': unescaped.Append('\\'); break;
                        default: unescaped.Append('\\').Append(nextCharacter); break;
                    }
                }
                else
                {
                    unescaped.Append(character);
                }
            }

            return unescaped.ToString();
        }

        public static Vector2 ToVector2(this string value)
        {
            var parts = value.Trim('(', ')').Split(',');
            if (parts.Length >= 2
                && float.TryParse(parts[0], out var x)
                && float.TryParse(parts[1], out var y))
            {
                return new Vector2(x, y);
            }

            return Vector2.zero;
        }

        public static Vector3 ToVector3(this string value)
        {
            var parts = value.Trim('(', ')').Split(',');
            if (parts.Length >= 3
                && float.TryParse(parts[0], out var x)
                && float.TryParse(parts[1], out var y)
                && float.TryParse(parts[2], out var z))
            {
                return new Vector3(x, y, z);
            }

            return Vector3.zero;
        }

        public static Vector4 ToVector4(this string value)
        {
            var parts = value.Trim('(', ')').Split(',');
            if (parts.Length >= 4
                && float.TryParse(parts[0], out var x)
                && float.TryParse(parts[1], out var y)
                && float.TryParse(parts[2], out var z)
                && float.TryParse(parts[3], out var w))
            {
                return new Vector4(x, y, z, w);
            }

            return Vector4.zero;
        }

        public static Vector2Int ToVector2Int(this string value)
        {
            var parts = value.Trim('(', ')').Split(',');
            if (parts.Length >= 2
                && int.TryParse(parts[0], out var x)
                && int.TryParse(parts[1], out var y))
            {
                return new Vector2Int(x, y);
            }

            return new Vector2Int(0, 0);
        }

        public static Vector3Int ToVector3Int(this string value)
        {
            var parts = value.Trim('(', ')').Split(',');
            if (parts.Length >= 3
                && int.TryParse(parts[0], out var x)
                && int.TryParse(parts[1], out var y)
                && int.TryParse(parts[2], out var z))
            {
                return new Vector3Int(x, y, z);
            }

            return Vector3Int.zero;
        }

        public static Vector4Int ToVector4Int(this string value)
        {
            var parts = value.Trim('(', ')').Split(',');
            if (parts.Length >= 4
                && int.TryParse(parts[0], out var x)
                && int.TryParse(parts[1], out var y)
                && int.TryParse(parts[2], out var z)
                && int.TryParse(parts[3], out var w))
            {
                return new Vector4Int(x, y, z, w);
            }

            return new Vector4Int(0, 0, 0, 0);
        }
    }
}
