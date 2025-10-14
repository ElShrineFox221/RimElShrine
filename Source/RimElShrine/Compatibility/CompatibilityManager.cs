using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RimElShrine.Compatibility
{
    public static class CompatibilityManager
    {
        static CompatibilityManager()
        {
            var asbs = AppDomain.CurrentDomain.GetAssemblies();
            var compatibleClasses = new Dictionary<(Type, Assembly), List<Type>>();
            var compatibleItems = new Dictionary<Type, List<ICompatible?>>();
            var cachedAttrsByClass = new Dictionary<Type, CompatibleAttribute>();
            foreach (var asb in asbs)
            {
                Type[] types;
                try
                {
                    types = asb.GetTypes();
                }
                catch
                {
                    continue; // 某些程序集可能无法反射，跳过
                }
                foreach (var type in types)
                {
                    //validate class
                    if (!type.IsClass || type.IsAbstract) continue;
                    var compatibleInterface = typeof(ICompatible);
                    if (!compatibleInterface.IsAssignableFrom(type)) continue;
                    //validate attr
                    var attr = type.GetCustomAttribute<CompatibleAttribute>();
                    if (attr == null)
                    {
                        ELSLog.Error($"Failed to get valid attr from a compatible class, the attr '{nameof(CompatibleAttribute)}' is required. type={type.FullName}");
                        continue;
                    }
                    cachedAttrsByClass[type] = attr;
                    //initialize compatibleClasses
                    foreach (var iface in type.GetInterfaces())
                    {
                        if (compatibleInterface.IsAssignableFrom(iface))
                        {
                            var key = (iface, attr.Target);
                            if (!compatibleClasses.TryGetValue(key, out var list)) compatibleClasses[key] = list = [];
                            list.Add(type);
                        }
                    }
                    //initialize compatibleItems
                    if (attr.InstiateMode != InstiateMode.InstiateEveryTimes)
                    {
                        try
                        {
                            if (Activator.CreateInstance(type) is ICompatible instance)
                            {
                                if (!compatibleItems.TryGetValue(type, out var instList)) compatibleItems[type] = instList = [];
                                instList.Add(instance);
                            }
                        }
                        catch
                        {
                            ELSLog.Error($"Failed to get instance, make sure there is a none args ctor for instiating. type={type.FullName}");
                        }
                    }
                }
            }
            CompatibleClasses = compatibleClasses;
            CompatibleItems = compatibleItems;
            CachedAttrsByClass = cachedAttrsByClass;
        }

        private readonly static IReadOnlyDictionary<(Type interfaceType, Assembly targetAssembly), List<Type>> CompatibleClasses;
        private readonly static IReadOnlyDictionary<Type, List<ICompatible?>> CompatibleItems;
        private readonly static IReadOnlyDictionary<Type, CompatibleAttribute> CachedAttrsByClass;
        public static TICompatible? GetCompatibleItem<TICompatible>(this Assembly assembly, int insId, string className, int minPriority, int maxPriority) where TICompatible : ICompatible
        {
            var type = typeof(TICompatible);
            //get compatible classes list;
            if (!CompatibleClasses.TryGetValue((type, assembly), out var compatibleTypes) || compatibleTypes.Count == 0) return default;
            var filteredTypes = compatibleTypes
                .Select(compatibleType => (compatibleType, CachedAttrsByClass[compatibleType]))
                .Where(item =>
                {
                    var type = item.compatibleType;
                    var attr = item.Item2;
                    if (!string.IsNullOrEmpty(className) && type.Name != className && type.FullName != className) return false;
                    if (minPriority >= 0 && attr.Prioprity < minPriority) return false;
                    if (maxPriority >= 0 && attr.Prioprity > maxPriority) return false;
                    return true;
                })
                .OrderBy(x => x.Item2.Prioprity)
                .ToList();
            if (filteredTypes.Count == 0) return default;
            var targetType = filteredTypes[0].compatibleType;
            var attribute = filteredTypes[0].Item2;
            if (insId < 0) insId = 0;
            switch (attribute.InstiateMode)
            {
                case InstiateMode.InstiateSingleton:
                    if (!CompatibleItems.TryGetValue(targetType, out var singletonList) || singletonList.Count == 0) return default;
                    var singleton = singletonList[0] ??= (TICompatible?)Activator.CreateInstance(targetType);
                    return (TICompatible?)singleton;

                case InstiateMode.InstiateEveryTimes:
                    return (TICompatible?)Activator.CreateInstance(targetType);
                case InstiateMode.InstiateOnce:
                default:
                    if (!CompatibleItems.TryGetValue(targetType, out var instanceList)) return default;
                    while (instanceList.Count <= insId) instanceList.Add(null);
                    var existingInstance = instanceList[insId] ??= (TICompatible?)Activator.CreateInstance(targetType); ;
                    return (TICompatible?)existingInstance;
            }
        }
        public static TICompatible? GetCompatibleItem<TICompatible>(this Assembly assembly, int insId, string className) 
            where TICompatible : ICompatible
            => GetCompatibleItem<TICompatible>(assembly, insId, className, -1, -1);
        public static TICompatible? GetCompatibleItem<TICompatible>(this Assembly assembly, int insId = 0) 
            where TICompatible : ICompatible
            => GetCompatibleItem<TICompatible>(assembly, insId, string.Empty, -1, -1);
    }
}
