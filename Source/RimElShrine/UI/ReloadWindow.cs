using System;
using UnityEngine;
using Verse;

namespace RimElShrine.UI
{
    public class ReloadWindow : Dialog_Confirm
    {
        private const string titleKey = $"{nameof(ReloadWindow)}_Title";
        private const string noticeKey = $"{nameof(ReloadWindow)}_Notice";
        public static void ReloadConfirmation()
        {
            var window = new ReloadWindow(titleKey.ES_Translate(), () =>
            {
                ModsConfig.RestartFromChangedMods();
            });
            Find.WindowStack.Add(window);
        }
        private readonly Control control;
        
        public ReloadWindow(string title, Action onConfirm) : base(title, onConfirm)
        {
            control = new Label(noticeKey);
            ClassesTreeToPrint("NewXmlClassesList", RimElShrineMod.LoadController.newXmlAttrsClasses).Parent = control;
            ClassesTreeToPrint("CurrentXmlClassesList", RimElShrineMod.LoadController.currentXmlCalsses).Parent = control;
        }

        private static Control ClassesTreeToPrint(string listTitle, LoadController.XmlClassesList xmlClasses)
        {
            var label = new Label(listTitle);

            var classesToIgnore = new Label("XmlClassesToIgnore") { Color = LocalColorOf.darkRed };
            foreach (var item in xmlClasses.classesToIgnore)
            {
                new Label(item) { Color = Color.grey }.Parent = classesToIgnore;
            }
            var classesToLoad = new Label("XmlClassesToLoad") { Color = LocalColorOf.darkGreen };
            foreach (var item in xmlClasses.classesToLoad)
            {
                new Label(item) { Color = Color.grey }.Parent = classesToLoad;
            }

            classesToIgnore.Parent = label;
            classesToLoad.Parent = label;
            return label;
        }

        private float scrollHeight = 0;
        private Vector2 scrollPostion = Vector2.zero;
        public override void DoWindowContents(Rect inRect)
        {
            var initRect = inRect;
            const float maxWindowHeight = 600f, windowWidth = 500f, titleHeight = 40f, btnHeight = 30f, excdH = titleHeight + btnHeight;
            inRect.width = windowWidth;
            inRect.height = Mathf.Min(maxWindowHeight, Mathf.Max(inRect.height, scrollHeight + excdH));
            base.DoWindowContents(inRect);

            var middleRect = new Rect(inRect.x, inRect.y + titleHeight, inRect.width, inRect.height - excdH);
            var viewRect = new Rect(middleRect.x, middleRect.y, middleRect.width, Mathf.Max(scrollHeight, middleRect.height));
            Widgets.BeginScrollView(middleRect, ref scrollPostion, viewRect);
            control.DrawVisualTree(middleRect, out var rectRemained, 0);
            scrollHeight = rectRemained.y - inRect.y;
            Widgets.EndScrollView();
            //这里需要计算正确设置窗口大小
            float subH = inRect.height - initRect.height, subW = inRect.width - initRect.width;
            float subX = -subW / 2f, subY = -subH / 2f;
            windowRect.x += subX;
            windowRect.y += subY;
            windowRect.width += subW;
            windowRect.height += subH;
        }
    }
}
