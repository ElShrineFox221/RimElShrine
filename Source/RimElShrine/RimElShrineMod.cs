using HarmonyLib;
using RimElShrine.UI;
using RimWorld;
using System.IO;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RimElShrine
{
    public abstract class RimElShrineMod : Mod
    {
        public RimElShrineMod(ModContentPack content) : base(content)
        {
            //..\..\Release\shields-v1.1.x\1.6\Assemblies\
            ModSettingAttachToMod(this);
            ELSLog.Msg($"Mod loaded. \nhandler={GetType().Name}, loadedDefs={LoadController.DefDatabase.Count}");
        }
#pragma warning disable CS8618
        static RimElShrineMod()
#pragma warning restore CS8618 
        {
            ModSettingsInit();
            LoadControllerInit();
            HarmonyInit();
        }

        #region Common
        public abstract string ModName { get; }
        #endregion

        #region Harmony
        public const string RimElShrineHarmonyName = "FoxMo.RimElSrhine";
        public static Harmony Harmony { get; private set; }

        private static void HarmonyInit()
        {
            Harmony = new Harmony(RimElShrineHarmonyName);
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            ELSLog.Msg("Harmony patches loaded.");
        }
        #endregion

        #region LoadController
        public static LoadController LoadController { get; private set; }
        public static void LoadControllerInit()
        {
            LoadController = new LoadController();
            ELSLog.Msg("Defs loader initialized.");
        }
        #endregion

        #region ModSettings
        public static RimElShrineModSetting ModSetting { get; protected set; }
        public static SettingsWindow? SettingsWindow = null;
        
        private static void ModSettingsInit()
        {
            ModSetting ??= new();
            string folderName = RimElShrineHarmonyName, handleName = typeof(RimElShrineMod).Name;
            var path = Path.Combine(GenFilePaths.ConfigFolderPath, GenText.SanitizeFilename(string.Format("Mod_{0}_{1}.xml", folderName, handleName)));
            Settings.FolderName = folderName;
            Settings.HandleName = handleName;
            Settings.DataContractSerializationPath = Path.GetDirectoryName(path) + "\\DataContract_" + Path.GetFileName(path);
            if (File.Exists(path))
            {
                ModSetting = LoadedModManager.ReadModSettings<RimElShrineModSetting>(folderName, handleName);
                ELSLog.Msg($"Settings loaded.");
            }
            else
            {
                ModSetting = new RimElShrineModSetting();
                LoadedModManager.WriteModSettings(folderName, handleName, ModSetting);
            }
            ELSLog.Msg("Settings initialized.");
        }
        private static void ModSettingAttachToMod(RimElShrineMod mod)
        {
            var privateSettingsField = typeof(Mod).GetField("modSettings", BindingFlags.Instance | BindingFlags.NonPublic);
            var privateModProperty = typeof(ModSettings).GetProperty(nameof(ModSettings.Mod), BindingFlags.Instance | BindingFlags.Public);
            privateSettingsField.SetValue(mod, ModSetting);
            privateModProperty.SetValue(ModSetting, mod);
            ELSLog.Msg($"Attached mod {mod.ModName} to mod settings.");
        }

        #region Visual
        public override void DoSettingsWindowContents(Rect canvas)
        {
            SettingsWindow ??= new();
            var index = Find.WindowStack.Windows.FirstIndexOf(w => w is Dialog_ModSettings);
            if (index != -1)
            {
                const float targetWidth = 800f;
                var target = Find.WindowStack.Windows[index];
                var subWidth = target.windowRect.width - targetWidth;
                target.windowRect.width = targetWidth;
                target.windowRect.x += subWidth / 2f;
                target.doCloseButton = false;
            }
            SettingsWindow.DoWindowContents(canvas);
        }
        public override void WriteSettings()
        {
            base.WriteSettings();
            LoadController.UpdateList();
        }
        public override string SettingsCategory() => ModName.ES_Translate();

        #endregion

        #endregion
    }
}
