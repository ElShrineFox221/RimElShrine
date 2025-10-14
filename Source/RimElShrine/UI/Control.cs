using RimElShrine.Data;
using System.Collections.Generic;
using UnityEngine;

namespace RimElShrine.UI
{
    public abstract class Control : ITreeNode<Control>
    {
        public int TreeDepth { get; private set; } = 0;
        public int VisualTreeDepth { get; private set; } = 0;
        private Control? parent = null;
        public Control? Parent
        {
            get => parent;
            set
            {
                if (parent != value)
                {
                    parent?.children.Remove(this);
                    parent = value;
                    TreeDepth = (parent?.TreeDepth ?? -1) + 1;
                    VisualTreeDepth = -1;
                    parent?.children.Add(this);
                }
            }
        }
        private readonly List<Control> children = [];
        public IReadOnlyList<Control> Children => children;

        private bool isVisible = true;
        public bool IsVisible
        {
            get => isVisible && (Parent is null || Parent.IsVisible);
            set => isVisible = value;
        }
        private bool isEnabled = true;
        public bool IsEnabled
        {
            get => isEnabled && (Parent is null || Parent.IsEnabled);
            set => isEnabled = value;
        }

        public void DrawVisualTree(Rect rectAll, out Rect rectRemained, int curVisualDepth)
        {
            if (curVisualDepth < 0)  curVisualDepth = 0;
            if (IsVisible)
            {
                DrawControl(rectAll, out rectAll, out var skipped);
                if(!skipped) curVisualDepth++;
                DrawChildren(rectAll, out rectAll);
            }
            rectRemained = rectAll;
            VisualTreeDepth = curVisualDepth;
        }
        protected virtual void DrawControl(Rect rectAll, out Rect rectRemained, out bool skipped)
        {
            skipped = true;
            rectRemained = rectAll;
        }
        protected virtual void DrawChildren(Rect rectAll, out Rect rectRemained)
        {
            foreach (var child in Children)
            {
                child.DrawVisualTree(rectAll, out rectAll, VisualTreeDepth);
            }
            rectRemained = rectAll;
        }

        public override string ToString()
            => $"[items={Children.Count}, parent={Parent}]";
    }
}
