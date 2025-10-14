using Verse;

namespace RimElShrine
{
    public static class ELSLog
    {
        public static void Msg(this string message)
        {
            Log.Message("[RimElShrine] " + message);
        }
        public static void Warn(this string message)
        {
            Log.Warning("[RimElShrine] " + message);
        }
        public static void Error(this string message)
        {
            Log.Error("[RimElShrine] " + message);
        }
        public static void Debug(this string message)
        {
            Log.Warning("[RimElShrine] [Debug] " + message);
        }
    }
}
