using System.Collections.Generic;

namespace RimElShrine.Data
{
    public interface ITreeNode<T> where T : class, ITreeNode<T>
    {
        public T? Parent { get; set; }
        public IReadOnlyList<T> Children { get; }
    }
}
