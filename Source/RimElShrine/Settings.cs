using RimElShrine.Data;
using RimElShrine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace RimElShrine
{
    public static class Settings
    {
        private readonly static Dictionary<Type, object?> settingsByType = [];
        private readonly static Dictionary<string, SettingItem> settingItemsByLabel = [];
        private readonly static List<Control> uiForest = [];
        private readonly static Dictionary<SettingItem, ContentControl> uiForestNodesBySettingItem = [];
        private static List<Assembly> assemblies = [];

        public static IReadOnlyDictionary<Type, object?> SettingsByType => settingsByType;
        public static IReadOnlyDictionary<string, SettingItem> SettingItemsByLabel => settingItemsByLabel;
        public static IReadOnlyList<Control> UIForest => uiForest;
        public static IReadOnlyDictionary<SettingItem, ContentControl> UIForestNodesBySettingItem => uiForestNodesBySettingItem;
        
        static Settings() => UpdateSettingItems(out _);
        public static void UpdateSettingItems(out bool changed)
        {
            var asbs = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains(nameof(RimElShrine)));
            if (assemblies.Count != asbs.Count())
            {
                assemblies = [.. asbs];
                changed = true;
                settingsByType.Clear();
                settingItemsByLabel.Clear();
                uiForest.Clear();
                uiForestNodesBySettingItem.Clear();
                //get settingsByType & setting items
                var contentControls = new List<ContentControl>();
                foreach (var assembly in asbs)
                {
                    var settingClasses = assembly.GetTypes()
                        .Where(t => t.TryGetAttribute<SettingAttribute>(out var sa) && !sa.IgnoreSetting);
                    foreach (var sc in settingClasses)
                    {
                        var instance = sc.IsAbstract ? null : Activator.CreateInstance(sc);
                        var list = sc.GetExposeDataAttrs(ref instance);
                        settingsByType.Add(sc, instance);
                        foreach (var item in list)
                        {
                            var si = new SettingItem(item);
                            settingItemsByLabel.Add(item.Label, si);
                            var node = new ContentControl(si);
                            contentControls.Add(node);
                            uiForestNodesBySettingItem.Add(si, node);
                        }
                    }
                }
                //get nodes relations
                foreach (var node in contentControls)
                {
                    var nodeSI = (SettingItem)node.Content;
                    var isEmptyParentName = string.IsNullOrEmpty(nodeSI.SettingItemAttr.ParentName);
                    var parent = isEmptyParentName ? null : contentControls.Find(n =>
                    {
                        var nData = ((SettingItem)n.Content).Data;
                        return nData.OwnerType == nodeSI.SettingItemAttr.ParentDeclaringType && nData.Info.Name == nodeSI.SettingItemAttr.ParentName;
                    });
                    node.Parent = parent;
                }
                //build forest
                uiForest.AddRange(contentControls.Select(c => c as Control).BuildForest());
                //
                $"Updated settings, there are {settingItemsByLabel.Count} setting items in {settingsByType.Count} settings and {UIForest.Count} ui render trees now,".Msg();
            }
            else changed = false;
        }

        #region Settings & SettingItems
        public static T GetSetting<T>() where T : class 
            => GetSetting(typeof(T)) as T ?? throw new($"Type has no setting instance, t={typeof(T).FullName}");
        public static object? GetSetting(Type type)
        {
            object? instance = null;
            var warnMsg = string.Empty;
            if (type.IsAbstract) warnMsg = $"Setting type '{type.Name}' is abstract, there is no intance to get.";
            else if (!settingsByType.TryGetValue(type, out instance))
            {
                warnMsg = $"Found no setting with type '{type.Name}', try to create new one.";
                instance = type.IsAbstract ? null : Activator.CreateInstance(type);
                settingsByType.Add(type, instance);
            }
            if (warnMsg != string.Empty) warnMsg.Warn();
            return instance;
        }

        public static object? GetValue(this SettingItem settingItem)
            => settingItem.Data.GetValue(GetSetting(settingItem.Data.OwnerType));
        public static void SetValue(this SettingItem settingItem, object? value)
            => settingItem.Data.SetValue(GetSetting(settingItem.Data.OwnerType), value);
        #endregion

        #region File save
        public static string FolderName { get; set; } = string.Empty;
        public static string HandleName { get; set; } = string.Empty;
        public static string DataContractSerializationPath { get; set; } = string.Empty;
        #endregion
    }
}
