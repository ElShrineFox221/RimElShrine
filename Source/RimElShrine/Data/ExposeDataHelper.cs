using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace RimElShrine.Data
{
    public static class ExposeDataHelper
    {
        private class ScribeMethods
        {
            public static void Scribe_DeepLook<T>(ref T target, string label, params object[] ctorArgs)
                => Scribe_Deep.Look(ref target, label, ctorArgs);
            public static void Scribe_CollectionLookList<T>(ref List<T> target, string label, LookMode lookMode, params object[] ctorArgs)
                => Scribe_Collections.Look(ref target, label, lookMode, ctorArgs);
            public static void Scribe_CollectionLookDict<K, V>(ref Dictionary<K, V> target, string label, LookMode lookModeKey, LookMode lookModeValue)
                => Scribe_Collections.Look(ref target, label, lookModeKey, lookModeValue);
            public static void Scribe_ValuesLook<T>(ref T target, string label, T defaultValue, bool forceSave)
                => Scribe_Values.Look(ref target, label, defaultValue, forceSave);

            public readonly static MethodInfo Scribe_DeepLookMethod
                = typeof(ScribeMethods).GetMethod(nameof(Scribe_DeepLook));
            public readonly static MethodInfo Scribe_CollectionsLookListMethod
                = typeof(ScribeMethods).GetMethod(nameof(Scribe_CollectionLookList));
            public readonly static MethodInfo Scribe_CollectionsLookDictMethod
                = typeof(ScribeMethods).GetMethod(nameof(Scribe_CollectionLookDict));
            public readonly static MethodInfo Scribe_ValuesLookMethod
                = typeof(ScribeMethods).GetMethod(nameof(Scribe_ValuesLook));
        }

        private readonly static Dictionary<Type, List<ExposeData>> ExposedDataDatabase = [];
        private readonly static Dictionary<(string, string), string> AutoLabels = [];
        public static string? GetAutoLabel(this (string className, string name) key)
            => AutoLabels.TryGetValue(key, out var label) ? label : null;

        public static List<ExposeData> GetExposeDataAttrs(this Type type, ref object? initInstance)
        {
            const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            List<ExposeData> dataSet = [];
            //Get or add data set.
            var got = ExposedDataDatabase.TryGetValue(type, out dataSet);
            if (!got)
            {
                var _initInstance = initInstance ?? (type.IsAbstract ? null : Activator.CreateInstance(type));
                //Initialize data set.
                var members = new List<MemberInfo>();
                members.AddRange(type.GetFields(FLAGS));
                members.AddRange(type.GetProperties(FLAGS));
                var attributeds = members.Select(memberInfo =>
                {
                    var attr = memberInfo.GetCustomAttribute<ExposeDataAttribute>(true);
                    var exposeData = attr is not null && !attr.Ignore ? new ExposeData(memberInfo, _initInstance) : null;
                    if (exposeData is not null) AutoLabels.Add((type.Name, memberInfo.Name), exposeData.Label);
                    return exposeData;
                }).Where(d => d is not null);
                initInstance = _initInstance;
                dataSet = [.. attributeds];
                ExposedDataDatabase.Add(type, dataSet);
            }
            return dataSet;
        }
        public static List<T> GetExposeDataAttrs<T>(this Type type, ref object? initInstance) where T : ExposeData
            => [.. GetExposeDataAttrs(type, ref initInstance).Select(a => a as T).Where(a => a != null)];

        #region ExposeAttributedData
        public static object? ExposeAttributedData<T>() where T : class 
            => ExposeAttributedData(typeof(T));
        public static object? ExposeAttributedData(this Type staticClass)
            => ExposeAttributedData(staticClass, null, false);
        public static object? ExposeAttributedData(this object instance, bool isInitInstance = false) 
            => ExposeAttributedData(instance.GetType(), instance, isInitInstance);
        private static object? ExposeAttributedData(Type classType, object? instance, bool isInitInstance)
        {
            var _instance = isInitInstance ? instance : null;
            var dataSet = GetExposeDataAttrs<ExposeData>(classType, ref _instance);
            instance ??= _instance;
            //Do exposeData method.
            foreach (var item in dataSet)
            {
                var value = item.GetValue(instance);
                var type = item.DataType;
                MethodInfo? genericMethodInfo, methodInfo = null;
                object?[] parameters;
                if (typeof(IExposable).IsAssignableFrom(type))
                {
                    genericMethodInfo = ScribeMethods.Scribe_DeepLookMethod;
                    parameters = [value, item.Label, null];
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    genericMethodInfo = ScribeMethods.Scribe_CollectionsLookListMethod;
                    var genericArgs = type.GetGenericArguments();
                    methodInfo = genericMethodInfo.MakeGenericMethod(genericArgs);
                    parameters = [value, item.Label, GetLookMode(genericArgs[0]), new object[] { }];
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    genericMethodInfo = ScribeMethods.Scribe_CollectionsLookDictMethod;
                    var genericArgs = type.GetGenericArguments();
                    methodInfo = genericMethodInfo.MakeGenericMethod(genericArgs);
                    parameters = [value, item.Label, GetLookMode(genericArgs[0]), GetLookMode(genericArgs[1])];
                }
                else
                {
                    genericMethodInfo = ScribeMethods.Scribe_ValuesLookMethod;
                    parameters = [value, item.Label, item.Default ?? default, false];
                }
                //Invoke
                methodInfo ??= genericMethodInfo.MakeGenericMethod(type);
                try
                {
                    methodInfo.Invoke(null, parameters);
                    item.SetValue(instance, parameters[0]);
                }
                catch (Exception e)
                {
                    ELSLog.Error($"Exception={e}, msg={e.Message} \ninfo=[methodInfo={methodInfo}, params={parameters.Length}, value={value}, default={item.Default}, label={item.Label}],\nTrace={e.StackTrace}");
                }
            }
            return instance;
        }
        #endregion

        private static LookMode GetLookMode(Type type)
        {
            if (type == typeof(TargetInfo)) return LookMode.TargetInfo;
            else if (typeof(Thing).IsAssignableFrom(type)) return LookMode.Reference;
            else if (typeof(IExposable).IsAssignableFrom(type)) return LookMode.Deep;
            else if (GenTypes.IsDef(type)) return LookMode.Def;
            else return LookMode.Undefined;
        }
    }
}
