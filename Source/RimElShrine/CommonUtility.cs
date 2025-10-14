using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using static UnityEngine.Networking.UnityWebRequest;

namespace RimElShrine
{
    public static class CommonUtility
    {
        public static string BuildString<T>(this IEnumerable<T> values, Func<T, string>? toString = null, string split = ", ", string end = ".", StringBuilder? useStrBuilder = null) where T : class
        {
            useStrBuilder ??= new StringBuilder();
            toString ??= (o) => $"{o}<{o.GetHashCode()}>";
            foreach(var value in values)
            {
                useStrBuilder.Append(toString.Invoke(value));
                if (values.Last() != value) useStrBuilder.Append(split); 
                else useStrBuilder.Append(end);
            }
            return useStrBuilder.ToString();
        }

        public static bool IsValid<T>(this IList<T> list) => list != null && list.Count > 0;
        public static T GetOrCreate<T>(this IList<T> list, int index, Func<T> instantiate, bool modifySource= false)
        {
            T result;
            if (index >= list.Count || index < 0) result = instantiate();
            else result = list[index];
            if (modifySource) list.Add(result);
            return result;
        }
        public static T Clamped<T>(this IList<T> list, int index)
        {
            if (list.IsValid())
            {
                var clampedIndex = Math.Min(Math.Max(index, 0), list.Count - 1);
                return list[clampedIndex];
            }
            else throw new OverflowException();
        }

        public static void RemoveComps(this ThingWithComps thingWithComps, IEnumerable<ThingComp> comps)
        {
            if (comps == null) return;
            foreach (ThingComp comp in comps)
            {
                thingWithComps.AllComps.Remove(comp);
                comp.parent = null;
            }
        }
        public static void AddComps(this ThingWithComps thingWithComps, IEnumerable<ThingComp> comps)
        {
            if (comps == null) return;
            thingWithComps.AllComps.AddRange(comps);
            foreach (ThingComp comp in comps)
            {
                comp.parent = thingWithComps;
            }
        }
        public static void RemoveComps(this ThingWithComps thingWithComps, params ThingComp[] comps)
        {
            if (comps == null) return;
            foreach (ThingComp comp in comps)
            {
                thingWithComps.AllComps.Remove(comp);
                comp.parent = null;
            }
        }
        public static void AddComps(this ThingWithComps thingWithComps, params ThingComp[] comps)
        {
            if (comps == null) return;
            thingWithComps.AllComps.AddRange(comps);
            foreach (ThingComp comp in comps)
            {
                comp.parent = thingWithComps;
            }
        }


        #region Hediff
        private readonly static FieldInfo HediffBodyPartFieldInfo = typeof(Hediff).GetField("part", BindingFlags.Instance | BindingFlags.NonPublic);
        public static BodyPartRecord GetHediffBodyPartRecord(this Hediff hediff)
            => HediffBodyPartFieldInfo.GetValue(hediff) as BodyPartRecord ?? throw new();

        public static bool TryGetHediff(this Pawn pawn, HediffDef def, out Hediff hediff, List<BodyPartDef>? bodyPartsRange = null)
        {
            bool result = true;
            List<Hediff> hediffs = [];
            pawn.health.hediffSet.GetHediffs(ref hediffs, h => h.def == def && (bodyPartsRange is null || bodyPartsRange.Count == 0 || bodyPartsRange.Contains(h.GetHediffBodyPartRecord().def)));
            if (hediffs.Count == 0)
            {
                hediff = pawn.health.GetOrAddHediff(HediffDefOf.BloodLoss, null, null, null);
                hediff.Severity = 0;
                result = false;
            }
            else hediff = hediffs[0];
            return result;
        }
        #endregion

        public static bool IsPowerActive(this ThingComp comp)
        {
            var active = comp.parent.GetComp<CompFlickable>()?.SwitchIsOn ?? false;
            if (active)
            {
                var powerComp = comp.parent.GetComp<CompPower>();
                if (powerComp != null)
                {
                    if (powerComp is CompPowerTrader trader) active &= trader.PowerOn;
                    else if (powerComp is CompPowerBattery battery) active &= battery.StoredEnergy >= 1;
                    else active &= powerComp.TransmitsPowerNow;
                }
            }
            return active;
        }

        public static T DisabledByCompFaction<T>(this T gizmo, Faction faction) where T : Gizmo
        {
            if (faction != Faction.OfPlayer) gizmo.Disable("IsNotSameFaction".ES_Translate());
            return gizmo;
        }

        public static Rect TopPartByHeight(this Rect rect, float height, out bool full)
        {
            var newRect = new Rect(rect.x, rect.y, rect.width, Mathf.Min(rect.height, height));
            full = newRect.height == rect.height;
            return newRect;
        }
        public static Rect BottomPartByHeight(this Rect rect, float height, out bool full)
        {
            var newRect = new Rect(rect.x, rect.y + Mathf.Max(rect.height - height, 0), rect.width, Mathf.Min(rect.height, height));
            full = newRect.height == rect.height;
            return newRect;
        }

        public static float TimeToTicks(this float seconds, TickerType tickerType = TickerType.Normal, float fps = 60f)
            => tickerType switch
            {
                TickerType.Normal => seconds * fps / 1,
                TickerType.Rare => seconds * fps / 250,
                TickerType.Long => seconds * fps / 2000,
                _ => seconds * fps / 1,
            };

        public static bool InRadiusOf(this Vector3 pos, Vector3 centre, float radius, bool ignoreY = true)
        {
            return ignoreY ? InRadiusOf(pos.ToVector2(), centre.ToVector2(), radius) : Vector3.Distance(pos, centre) < radius;
        }
        public static bool InRadiusOf(this Vector2 pos, Vector2 centre, float radius)
            => Vector3.Distance(pos, centre) < radius;
    }
}
