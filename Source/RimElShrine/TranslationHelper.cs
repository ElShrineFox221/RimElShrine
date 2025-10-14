using RimElShrine.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Verse;

namespace RimElShrine
{
    public static class TranslationHelper
    {
        private class UntranslatedKeyList : List<string> { }
        private static readonly UntranslatedKeyList untranslatedKeys = [];
        public static IReadOnlyList<string> UntranslatedKeys => untranslatedKeys;

        public static TaggedString ES_Translate(this string key, params NamedArgument[] namedArguments)
        {
            var r = key.TryTranslate(out var tagStr);
            if (!r)
            {
                var notContained = !untranslatedKeys.Contains(key);
                if (!string.IsNullOrEmpty(key))
                {
                    if (notContained)
                    {
                        ELSLog.Debug($"Key has not been translated, use default value, key={key}");
                        untranslatedKeys.Add(key);
                        var path = Path.Combine(GenFilePaths.ConfigFolderPath, GenText.SanitizeFilename(string.Format("Mod_{0}_{1}.xml", nameof(RimElShrineMod), nameof(UntranslatedKeys))));
                        DataContractSerializeHelper.TryWrite(typeof(UntranslatedKeyList), untranslatedKeys, path);
                    }
                    LanguageDatabase.defaultLanguage.TryGetTextFromKey(key, out tagStr);
                }
                else if (notContained) ELSLog.Debug($"Key should not be a null or empty value, trace={new StackTrace()}");
            }
            tagStr = tagStr.Formatted(namedArguments);
            return tagStr;
        }
    }
}
