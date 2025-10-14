using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RimElShrine.Data
{
    [DataContract]
    public abstract class BoxedItem
    {
        public abstract object? GetValue();
        public abstract Type ValueType { get; }
        public static BoxedItem Create(object? value)
        {
            if (value == null) return new BoxedItem<object?>(null);
            var valueType = value.GetType();
            
            var boxedType = typeof(BoxedItem<>).MakeGenericType(valueType);
            if (!knownTypes.Contains(boxedType)) knownTypes.Add(boxedType);
            return (BoxedItem)Activator.CreateInstance(boxedType, value);
        }
        private readonly static List<Type> knownTypes = [];
        public static IReadOnlyList<Type> KnownTypes => knownTypes;
    }

    [DataContract]
    public class BoxedItem<TItem>(TItem item) : BoxedItem
    {
        [DataMember] public TItem Item { get; protected set; } = item;
        public BoxedItem() : this(default!) { }
        public override object? GetValue() => Item;
        public override Type ValueType => typeof(TItem);
    }
}
