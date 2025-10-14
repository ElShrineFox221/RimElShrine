using System;
using System.Collections.Generic;
using System.Linq;

namespace RimElShrine.Data
{
    public static class TreeHelper
    {
        #region Tree Modify
        public static ITreeNode<T> GetRoot<T>(this ITreeNode<T> node) where T : class, ITreeNode<T>
        {
            var parent = node.Parent;
            while (parent?.Parent is not null) parent = parent.Parent;
            return parent ?? node;
        }
        #endregion

        #region Tree Build
        public static IReadOnlyList<TNode> BuildForest<TNode>(this IEnumerable<TNode> nodes) where TNode : class, ITreeNode<TNode>
        {
            var nodeList = nodes.ToList();
            if (!nodeList.Any()) return [];
            DetectCycles(nodeList);
            var roots = nodeList.Where(node => node.Parent == null || !nodeList.Contains(node.Parent)).ToList();
            return roots.AsReadOnly();
        }
        public static TNode BuildTree<TNode>(this IEnumerable<TNode> nodes) where TNode : class, ITreeNode<TNode>
        {
            var nodeList = nodes.ToList();
            if (!nodeList.Any()) throw new InvalidOperationException("No nodes");
            var roots = nodeList.BuildForest();
            if (roots.Count == 0) throw new InvalidOperationException("No trees");
            if (roots.Count > 1) throw new InvalidOperationException($"Plural trees, treesCount={roots.Count},trees=[{roots.BuildString()}]");
            return roots[0];
        }
        private static void DetectCycles<TNode>(IList<TNode> nodes) where TNode : class, ITreeNode<TNode>
        {
            foreach (var node in nodes)
            {
                var path = new List<TNode>();
                var _hasCycle = false;
                var parent = node.Parent;
                while (parent is not null)
                {
                    var contained = path.Contains(parent);
                    path.Add(parent);
                    parent = parent.Parent;
                    if (contained)
                    {
                        _hasCycle = true;
                        break;
                    }
                }
                if (_hasCycle) throw new InvalidOperationException($"Cycle detected, info=[{path.BuildString(split: ",\n", end: string.Empty)}]");
            }
        }
        public static IReadOnlyList<TNode> BuildNewForest<TNode>(this IEnumerable<TNode> sourceNodes, Func<TNode, TNode> copyFunc, Predicate<TNode> predicate) where TNode : class , ITreeNode<TNode>
        {
            var newForest = new List<TNode>();
            foreach (var sourceNode in sourceNodes)
            {
                var newRoots = BuildNewTreeInternal(sourceNode, null, copyFunc, predicate);
                newForest.AddRange(newRoots);
            }
            return newForest.AsReadOnly();
            static IEnumerable<TNode> BuildNewTreeInternal(TNode sourceNode, TNode? newRoot, Func<TNode, TNode> copyFunc, Predicate<TNode> predicate)
            {
                List<TNode> newRoots = [];
                TNode? newNode = null;
                //
                if (predicate(sourceNode))
                {
                    newNode = copyFunc(sourceNode);
                    if (newRoot is not null) newNode.Parent = newRoot;
                    else newRoots.Add(newNode);
                }
                foreach (var child in sourceNode.Children)
                {
                    var subNewRoot = BuildNewTreeInternal(child, newNode, copyFunc, predicate);
                    newRoots.AddRange(subNewRoot);
                }
                return newRoots;
            }
        }
        #endregion

        #region Tree Traversal
        public enum TraversalState
        {
            Continue,
            SkipSubtree,
            Stop
        }
        public static bool TraverseDLR<TNode>(this TNode treeRoot, Func<TNode, TraversalState> visitor)
            where TNode : class, ITreeNode<TNode>
        {
            var action = visitor(treeRoot);
            switch (action)
            {
                case TraversalState.Stop: return false; 
                case TraversalState.SkipSubtree: return true; 
                case TraversalState.Continue:
                default:
                    foreach (var child in treeRoot.Children)
                    {
                        if (!TraverseDLR(child, visitor)) return false;
                    }
                    return true;
            }
        }
        public static bool TraverseDLR<TNode>(this IEnumerable<TNode> forestRoots, Func<TNode, TraversalState> visitor)
            where TNode : class, ITreeNode<TNode>
        {
            foreach (var root in forestRoots)
            {
                var _continue = root.TraverseDLR(visitor);
                if (!_continue) return false;
            }
            return true;
        }
        #endregion
    }
}
