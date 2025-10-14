using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RimElShrine.Reflection
{
    public static class ReflectionHelper
    {
        public const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        public const BindingFlags PubStatic = BindingFlags.Public | BindingFlags.Static;
        public const BindingFlags PubInstance = BindingFlags.Public | BindingFlags.Instance;
        public const BindingFlags NonPubStatic = BindingFlags.NonPublic | BindingFlags.Static;
        public const BindingFlags NonPubInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        public const BindingFlags AllStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        public const BindingFlags AllInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly static Dictionary<(Type type, string name, int id), FieldInfo> cachedFiledInfos = [];
        private readonly static Dictionary<(Type type, string name, int id), PropertyInfo> cachedPropertyInfos = [];
        private readonly static Dictionary<(Type type, string name, int id), MethodInfo> cachedMethodInfos = [];

        /// <summary>
        /// Get cached field info, the sameName and id ones would be replaced. 
        /// </summary>
        public static FieldInfo GetCachedFieldInfo(this Type type, string name, int id = 0)
        {
            var key = (type, name, id);
            if (!cachedFiledInfos.TryGetValue(key, out var fieldInfo)) cachedFiledInfos[key] = fieldInfo = type.GetField(name, All);
            return fieldInfo;
        }
        /// <summary>
        /// Get cached property info, the sameName and id ones would be replaced.
        /// </summary>
        public static PropertyInfo GetCachedPropertyInfo(this Type type, string name, int id = 0)
        {
            var key = (type, name, id);
            if (!cachedPropertyInfos.TryGetValue(key, out var propertyInfo)) cachedPropertyInfos[key] = propertyInfo = type.GetProperty(name, All);
            return propertyInfo;
        }
        /// <summary>
        /// Get cached method info, the sameName and id ones would be replaced.
        /// </summary>
        public static MethodInfo? GetCachedMethodInfo(this Type type, string name, int id = 0, Predicate<MethodInfo>? predicate = null)
        {
            var key = (type, name, id);
            if (!cachedMethodInfos.TryGetValue(key, out var methodInfo))
            {
                var methods = type.GetMethods(All);
                methodInfo = methods.FirstOrDefault(method =>
                {
                    if (predicate != null && !predicate(method)) return false;
                    if (method.Name != name) return false;
                    return true;
                });
                if(methodInfo is not null) cachedMethodInfos[key] = methodInfo;
            }
            return methodInfo;
        }

    }
}
