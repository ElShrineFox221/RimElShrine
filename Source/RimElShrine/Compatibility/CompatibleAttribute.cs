using System;
using System.Reflection;

namespace RimElShrine.Compatibility
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CompatibleAttribute(Type anyTypeOfTargetAssembly) : Attribute
    {
        public InstiateMode InstiateMode = InstiateMode.InstiateOnce;
        /// <summary>
        /// Priority of the compatible class, the lowest priority will be used in filtering
        /// </summary>
        public int Prioprity = 50;
        /// <summary>
        /// Target assembly of the mod to be compatible with, usually use `typeof(SomeTypeInTargetMod).Assembly`.
        /// </summary>
        public Assembly Target = anyTypeOfTargetAssembly.Assembly;
    }
}
