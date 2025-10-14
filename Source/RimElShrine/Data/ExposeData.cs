using System;
using System.Reflection;

namespace RimElShrine.Data
{
    public class ExposeData
    {
        public string Label;
        public object? Default;
        public MemberInfo Info;
        public Type DataType => Info is FieldInfo fi ? fi.FieldType : (Info is PropertyInfo pi ? pi.PropertyType : throw new());
        public Type OwnerType => Info.DeclaringType;
        public ExposeData(MemberInfo info, object? initInstance = null)
        {
            if (info is FieldInfo fi) Default = fi.GetValue(initInstance);
            else if (info is PropertyInfo pi) Default = pi.GetValue(initInstance);
            else throw new ArgumentException("The info should be FieldInfo or PropertyInfo");
            Label = $"{info.DeclaringType.Name}_{info.Name}";
            Info = info;
        }
        public object? GetValue(object? instance)
            => Info is FieldInfo fi ? fi.GetValue(instance) : (Info is PropertyInfo pi ? pi.GetValue(instance) : throw new());
        public void SetValue(object? instance, object? value)
        {
            if (Info is FieldInfo fi) fi.SetValue(instance, value);
            else if (Info is PropertyInfo pi) pi.SetValue(instance, value);
            else throw new();
        }
        public override string ToString()
            => $"[label={Label}, default={Default}, class={Info.DeclaringType.FullName}]";
    }
}
