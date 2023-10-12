using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HarmonyLib
{
    public enum ArgumentType
    {
        /// <summary>This is a normal argument</summary>
        Normal,
        /// <summary>This is a reference argument (ref)</summary>
        Ref,
        /// <summary>This is an out argument (out)</summary>
        Out,
        /// <summary>This is a pointer argument (&amp;)</summary>
        Pointer
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HarmonyPrefix : Attribute
    {
    }

    /// <summary>Specifies the Postfix function in a patch class</summary>
    ///
    [AttributeUsage(AttributeTargets.Method)]
    public class HarmonyPostfix : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Delegate | AttributeTargets.Method, AllowMultiple = true)]
    public class HarmonyPatch : HarmonyAttribute
    {
        /// <summary>An empty annotation can be used together with TargetMethod(s)</summary>
        ///
        public HarmonyPatch()
        {
        }

        /// <summary>An annotation that specifies a class to patch</summary>
        /// <param name="declaringType">The declaring class/type</param>
        ///
        public HarmonyPatch(Type declaringType)
        {
            info.declaringType = declaringType;
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="declaringType">The declaring class/type</param>
        /// <param name="argumentTypes">The argument types of the method or constructor to patch</param>
        ///
        public HarmonyPatch(Type declaringType, Type[] argumentTypes)
        {
            info.declaringType = declaringType;
            info.argumentTypes = argumentTypes;
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="declaringType">The declaring class/type</param>
        /// <param name="methodName">The name of the method, property or constructor to patch</param>
        ///
        public HarmonyPatch(Type declaringType, string methodName)
        {
            info.declaringType = declaringType;
            info.methodName = methodName;
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="declaringType">The declaring class/type</param>
        /// <param name="methodName">The name of the method, property or constructor to patch</param>
        /// <param name="argumentTypes">An array of argument types to target overloads</param>
        ///
        public HarmonyPatch(Type declaringType, string methodName, params Type[] argumentTypes)
        {
            info.declaringType = declaringType;
            info.methodName = methodName;
            info.argumentTypes = argumentTypes;
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="declaringType">The declaring class/type</param>
        /// <param name="methodName">The name of the method, property or constructor to patch</param>
        /// <param name="argumentTypes">An array of argument types to target overloads</param>
        /// <param name="argumentVariations">Array of <see cref="ArgumentType"/></param>
        ///
        public HarmonyPatch(Type declaringType, string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations)
        {
            info.declaringType = declaringType;
            info.methodName = methodName;
            ParseSpecialArguments(argumentTypes, argumentVariations);
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="declaringType">The declaring class/type</param>
        /// <param name="methodType">The <see cref="MethodType"/></param>
        ///
        public HarmonyPatch(Type declaringType, MethodType methodType)
        {
            info.declaringType = declaringType;
            info.methodType = methodType;
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="declaringType">The declaring class/type</param>
        /// <param name="methodType">The <see cref="MethodType"/></param>
        /// <param name="argumentTypes">An array of argument types to target overloads</param>
        ///
        public HarmonyPatch(Type declaringType, MethodType methodType, params Type[] argumentTypes)
        {
            info.declaringType = declaringType;
            info.methodType = methodType;
            info.argumentTypes = argumentTypes;
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="declaringType">The declaring class/type</param>
        /// <param name="methodType">The <see cref="MethodType"/></param>
        /// <param name="argumentTypes">An array of argument types to target overloads</param>
        /// <param name="argumentVariations">Array of <see cref="ArgumentType"/></param>
        ///
        public HarmonyPatch(Type declaringType, MethodType methodType, Type[] argumentTypes, ArgumentType[] argumentVariations)
        {
            info.declaringType = declaringType;
            info.methodType = methodType;
            ParseSpecialArguments(argumentTypes, argumentVariations);
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="declaringType">The declaring class/type</param>
        /// <param name="methodName">The name of the method, property or constructor to patch</param>
        /// <param name="methodType">The <see cref="MethodType"/></param>
        ///
        public HarmonyPatch(Type declaringType, string methodName, MethodType methodType)
        {
            info.declaringType = declaringType;
            info.methodName = methodName;
            info.methodType = methodType;
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="methodName">The name of the method, property or constructor to patch</param>
        ///
        public HarmonyPatch(string methodName)
        {
            info.methodName = methodName;
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="methodName">The name of the method, property or constructor to patch</param>
        /// <param name="argumentTypes">An array of argument types to target overloads</param>
        ///
        public HarmonyPatch(string methodName, params Type[] argumentTypes)
        {
            info.methodName = methodName;
            info.argumentTypes = argumentTypes;
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="methodName">The name of the method, property or constructor to patch</param>
        /// <param name="argumentTypes">An array of argument types to target overloads</param>
        /// <param name="argumentVariations">An array of <see cref="ArgumentType"/></param>
        ///
        public HarmonyPatch(string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations)
        {
            info.methodName = methodName;
            ParseSpecialArguments(argumentTypes, argumentVariations);
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="methodName">The name of the method, property or constructor to patch</param>
        /// <param name="methodType">The <see cref="MethodType"/></param>
        ///
        public HarmonyPatch(string methodName, MethodType methodType)
        {
            info.methodName = methodName;
            info.methodType = methodType;
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="methodType">The <see cref="MethodType"/></param>
        ///
        public HarmonyPatch(MethodType methodType)
        {
            info.methodType = methodType;
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="methodType">The <see cref="MethodType"/></param>
        /// <param name="argumentTypes">An array of argument types to target overloads</param>
        ///
        public HarmonyPatch(MethodType methodType, params Type[] argumentTypes)
        {
            info.methodType = methodType;
            info.argumentTypes = argumentTypes;
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="methodType">The <see cref="MethodType"/></param>
        /// <param name="argumentTypes">An array of argument types to target overloads</param>
        /// <param name="argumentVariations">An array of <see cref="ArgumentType"/></param>
        ///
        public HarmonyPatch(MethodType methodType, Type[] argumentTypes, ArgumentType[] argumentVariations)
        {
            info.methodType = methodType;
            ParseSpecialArguments(argumentTypes, argumentVariations);
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="argumentTypes">An array of argument types to target overloads</param>
        ///
        public HarmonyPatch(Type[] argumentTypes)
        {
            info.argumentTypes = argumentTypes;
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="argumentTypes">An array of argument types to target overloads</param>
        /// <param name="argumentVariations">An array of <see cref="ArgumentType"/></param>
        ///
        public HarmonyPatch(Type[] argumentTypes, ArgumentType[] argumentVariations)
        {
            ParseSpecialArguments(argumentTypes, argumentVariations);
        }

        /// <summary>An annotation that specifies a method, property or constructor to patch</summary>
        /// <param name="typeName">The full name of the declaring class/type</param>
        /// <param name="methodName">The name of the method, property or constructor to patch</param>
        /// <param name="methodType">The <see cref="MethodType"/></param>
        ///
        public HarmonyPatch(string typeName, string methodName, MethodType methodType = MethodType.Normal)
        {
            info.declaringType = TypeByName(typeName);
            info.methodName = methodName;
            info.methodType = methodType;
        }

        public static Type TypeByName(string name)
        {
            var type = Type.GetType(name, false);
            if (type is null)
                type = AllTypes().FirstOrDefault(t => t.FullName == name);
            if (type is null)
                type = AllTypes().FirstOrDefault(t => t.Name == name);
            if (type is null) FileLog.Debug($"AccessTools.TypeByName: Could not find type named {name}");
            return type;
        }

        public static IEnumerable<Assembly> AllAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Microsoft.VisualStudio") is false);
        }

        public static IEnumerable<Type> AllTypes()
        {
            return AllAssemblies().SelectMany(a => AccessTools.GetTypesFromAssembly(a));
        }

        void ParseSpecialArguments(Type[] argumentTypes, ArgumentType[] argumentVariations)
        {
            if (argumentVariations is null || argumentVariations.Length == 0)
            {
                info.argumentTypes = argumentTypes;
                return;
            }

            if (argumentTypes.Length < argumentVariations.Length)
                throw new ArgumentException("argumentVariations contains more elements than argumentTypes", nameof(argumentVariations));

            var types = new List<Type>();
            for (var i = 0; i < argumentTypes.Length; i++)
            {
                var type = argumentTypes[i];
                switch (argumentVariations[i])
                {
                    case ArgumentType.Normal:
                        break;
                    case ArgumentType.Ref:
                    case ArgumentType.Out:
                        type = type.MakeByRefType();
                        break;
                    case ArgumentType.Pointer:
                        type = type.MakePointerType();
                        break;
                }
                types.Add(type);
            }
            info.argumentTypes = types.ToArray();
        }
    }
}