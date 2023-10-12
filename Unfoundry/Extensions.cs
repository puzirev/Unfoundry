using HarmonyLib;
using System;
using System.Reflection;
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

        //public static Traverse Resolve(this Traverse self)
        //{
        //    if (Traverse.Create(self).Field("_root").GetValue<object>() is null)
        //    {
        //        var _info = Traverse.Create(self).Field("_info").GetValue<MemberInfo>()
        //        if (_info is FieldInfo fieldInfo && fieldInfo.IsStatic)
        //            return new Traverse(self.GetValue());
        //        if (_info is PropertyInfo propertyInfo && propertyInfo.GetGetMethod().IsStatic)
        //            return new Traverse(self.GetValue());
        //        var _method = Traverse.Create(self).Field("_method").GetValue<MethodInfo>()
        //        if (_method is object && _method.IsStatic)
        //            return new Traverse(self.GetValue());

        //        var _type = Traverse.Create(self).Field("_type").GetValue<Type>()
        //        if (_type is object)
        //            return self;
        //    }
        //    return new Traverse(self.GetValue());
        //}

        //public static Traverse Method(this Traverse self, string name, Type[] paramTypes, object[] arguments = null)
        //{
        //    if (name is null) throw new ArgumentNullException(nameof(name));
        //    var resolved = self.Resolve();
        //    var _type = Traverse.Create(resolved).Field("_type").GetValue<Type>()
        //    if (_type is null) return new Traverse();
        //    var Cache = Traverse.Create(typeof(Traverse)).Field("Cache").GetValue();
        //    var method = Cache.GetMethodInfo(_type, name, paramTypes);
        //    if (method is null) return new Traverse();
        //    return new Traverse(resolved._root, (MethodInfo)method, arguments);
        //}
    }
}
