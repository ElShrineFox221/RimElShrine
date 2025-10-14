using RimElShrine.Data;
using RimElShrine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Verse;

namespace RimElShrine
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public abstract class LoadingControlAttribute : Attribute
    {
        public abstract void DoAction(LoadController loaderController, object? settingItemValue);
    }
    public enum ClassListMode
    {
        None,
        Whitelist,
        Blacklist
    }
    public class XmlClassesControlAttribute : LoadingControlAttribute
    {
        public string Class = string.Empty;
        public ClassListMode Enabled = ClassListMode.Whitelist;
        public ClassListMode Disabled = ClassListMode.Blacklist;
        public override void DoAction(LoadController loaderController, object? settingItemValue)
        {
            if (string.IsNullOrEmpty(Class)) return;
            if(settingItemValue is bool enabled)
            {
                var mode = enabled ? Enabled : Disabled;
                var tarList = mode switch
                {
                    ClassListMode.Blacklist => loaderController.newXmlAttrsClasses.classesToIgnore,
                    ClassListMode.Whitelist => loaderController.newXmlAttrsClasses.classesToLoad,
                    _ => null
                };
                var classes = LoadController.ParseXmlAttrClasses(Class);
                tarList?.AddRange(classes);
    }
        }
    }

    public class LoadController
    {
        public const string elsXmlAttrName = "ELSClass";

        public XmlClassesList currentXmlCalsses = new();
        public XmlClassesList newXmlAttrsClasses = new();
        public Dictionary<string, Def> DefDatabase = [];
        public LoadController()
        {
            Update(true);
        }


        #region Control with xml attr classes
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:删除未使用的参数", Justification = "<挂起>")]
        public bool LoadingXML(XmlNode xmlNode, string elsClass)
        {
            var classes = ParseXmlAttrClasses(elsClass);
            var shouldLoad = false;
            foreach (var c in classes)
            {
                if (newXmlAttrsClasses.classesToIgnore.Contains(c))
                {
                    shouldLoad = false;
                    break;
                }
                if (!shouldLoad && newXmlAttrsClasses.classesToLoad.Contains(c)) shouldLoad = true;
            }
            return shouldLoad;
        }

        public void UpdateList() => Update(false);
        private bool Update(bool fromInit)
        {
            newXmlAttrsClasses = [];
            var settingItems = Settings.SettingItemsByLabel.Values;
            foreach (var item in settingItems)
            {
                var attrs = item.Data.Info.GetCustomAttributes<LoadingControlAttribute>(true);
                var value = item.GetValue();
                foreach (var attr in attrs)
                {
                    attr.DoAction(this, value);
                }
            }
            newXmlAttrsClasses.Sort();
            bool mismatched = fromInit;
            if (fromInit) currentXmlCalsses = newXmlAttrsClasses.Clone();
            else
            {
                //compare cahced with new
                mismatched = currentXmlCalsses.Count != newXmlAttrsClasses.Count;
                if (!mismatched)
                {
                    int i = 0;
                    foreach (var (c, l) in newXmlAttrsClasses)
                    {
                        var cached = currentXmlCalsses[i];
                        if (c != cached.Item1 || l ^ cached.Item2)
                        {
                            mismatched = true;
                            break;
                        }
                        i++;
                    }
                }
                if (mismatched) ReloadWindow.ReloadConfirmation();
            }
            return mismatched;
        }
        
        public static IEnumerable<string> ParseXmlAttrClasses(string str)
        {
            var msg = $"Invaild xml attr class str: '{str}'";
            if (string.IsNullOrEmpty(str)) ELSLog.Error(msg);
            var sli = str.Split(',');
            foreach (var item in sli)
            {
                var s = item.Trim();
                if (string.IsNullOrEmpty(s) || s.Contains(' ')) ELSLog.Error(msg);
                yield return s;
            }
            yield break;
        }
        #endregion

        public class XmlClassesList : IEnumerable<(string, bool)>, ICloneable<XmlClassesList>
        {
            public List<string> classesToLoad = [];
            public List<string> classesToIgnore = [];

            public int Count => classesToLoad.Count + classesToIgnore.Count;
            public void Sort()
            {
                classesToLoad.Sort();
                classesToIgnore.Sort();
            }
            /// <summary>
            /// Do sort and clone
            /// </summary>
            /// <returns>Copy of this</returns>
            public XmlClassesList Clone()
            {
                Sort();
                var clone = new XmlClassesList
                {
                    classesToLoad = [.. classesToLoad],
                    classesToIgnore = [.. classesToIgnore]
                };
                return clone;
            }

            public (string, bool) this[int index]
            {
                get
                {
                    if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} is negative");
                    if (index < classesToLoad.Count) return (classesToLoad[index], true);
                    int ignoreIndex = index - classesToLoad.Count;
                    if (ignoreIndex < classesToIgnore.Count) return (classesToIgnore[ignoreIndex], false);
                    throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} exceeds total count");
                }
                set
                {
                    if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} is negative: {index}");
                    var (className, shouldLoad) = value;
                    if (shouldLoad)
                    {
                        if (index >= classesToLoad.Count) throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} ({index}) exceeds {nameof(classesToLoad)} count ({classesToLoad.Count})");
                        classesToLoad[index] = className;
                    }
                    else
                    {
                        int ignoreIndex = index - classesToLoad.Count;
                        if (ignoreIndex >= classesToIgnore.Count) throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} ({index}) exceeds total count ({classesToLoad.Count + classesToIgnore.Count})");
                        classesToIgnore[ignoreIndex] = className;
                    }
                }
            }
            public IEnumerator<(string, bool)> GetEnumerator()
            {
                foreach (var item in classesToLoad)
                {
                    yield return (item, true);
                }
                foreach (var item in classesToIgnore)
                {
                    yield return (item, false);
                }
                yield break;
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
