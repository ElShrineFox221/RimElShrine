using System;

namespace RimElShrine.Data
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ExposeDataAttribute<T> : Attribute where T : ExposeData
    {
        public bool Ignore = false;
    }
    public class ExposeDataAttribute : ExposeDataAttribute<ExposeData> { }
}
