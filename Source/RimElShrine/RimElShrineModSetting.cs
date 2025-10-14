using RimElShrine.Data;
using System.Collections.Generic;
using Verse;
using static RimElShrine.Settings;

namespace RimElShrine
{
    public class RimElShrineModSetting : ModSettings
    {
        public new void Write()
        {
            if (string.IsNullOrEmpty(FolderName)) ELSLog.Error($"{nameof(FolderName)} is empty or null.");
            else if (string.IsNullOrEmpty(HandleName)) ELSLog.Error($"{nameof(HandleName)} is empty or null.");
            else LoadedModManager.WriteModSettings(FolderName, HandleName, this);
        }

        private Dictionary<string, BoxedItem> BoxedItems { get; set; } = [];

        private void UpdateBoxedItems(bool doClear)
        {
            if (doClear) BoxedItems.Clear();
            foreach (var (k, v) in SettingItemsByLabel)
            {
                BoxedItems.SetOrAdd(k, BoxedItem.Create(v.GetValue()));
            }
        }
        public override void ExposeData()
        {
            var isRead = Scribe.mode == LoadSaveMode.LoadingVars;
            var isWrite = Scribe.mode == LoadSaveMode.Saving;
            var msg0 = isWrite ? "Saved" : "Loaded";
            var msg1 = ".";
            UpdateBoxedItems(true);
            if (isRead)
            {
                if (!DataContractSerializeHelper.TryRead(typeof(Dictionary<string, BoxedItem>), out var boxedItems, DataContractSerializationPath, BoxedItem.KnownTypes)) ELSLog.Warn("Failed to read serialization files.");
                else if(boxedItems is not null)
                {
                    foreach (var (k, v) in (Dictionary<string, BoxedItem>)boxedItems)
                    {
                        BoxedItems.SetOrAdd(k, v);
                    }
                    foreach (var (k, v) in BoxedItems)
                    {
                        if(SettingItemsByLabel.TryGetValue(k, out var si))
                        {
                            si.SetValue(v.GetValue());
                        }
                    }
                }
            }
            if (isWrite)
            {
                if (!DataContractSerializeHelper.TryWrite(typeof(Dictionary<string, BoxedItem>), BoxedItems, DataContractSerializationPath, BoxedItem.KnownTypes)) ELSLog.Warn("Failed to write serialization files.");
            }
            //
            base.ExposeData();
            this.ExposeAttributedData(true);
            int missCount = 0;
            string missList = string.Empty;
            if (missCount > 0) msg1 = $", but {missCount} of those have no matched target.\nmissList={missList}";
            var hasMsg1 = msg1 != ".";
            var msg = $"{msg0} {BoxedItems.Count} setting items{msg1}";
            if (hasMsg1) ELSLog.Warn(msg);
            else ELSLog.Msg(msg);
        }
    }
}
