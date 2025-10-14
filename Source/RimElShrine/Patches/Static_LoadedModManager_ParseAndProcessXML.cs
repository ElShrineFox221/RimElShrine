using HarmonyLib;
using System.Collections.Generic;
using System.Xml;
using Verse;

namespace RimElShrine.Patches
{
    [HarmonyPatch(typeof(LoadedModManager), nameof(LoadedModManager.ParseAndProcessXML))]
    public static class Static_LoadedModManager_ParseAndProcessXML
    {
        [HarmonyPrefix]
#pragma warning disable IDE0060
        public static bool Prefix(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup, bool hotReload)
#pragma warning restore IDE0060
        {
            var nodesToRemove = new List<XmlNode>();
            foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes)
            {
                if (xmlNode.NodeType == XmlNodeType.Element)
                {
                    string? attrVal = xmlNode.Attributes?[LoadController.elsXmlAttrName]?.Value;
                    if(attrVal is not null && attrVal != string.Empty)
                    {
                        var load = RimElShrineMod.LoadController.LoadingXML(xmlNode, attrVal);
                        if (!load) nodesToRemove.Add(xmlNode);
                    }
                }
            }
            foreach (XmlNode node in nodesToRemove)
            {
                xmlDoc.DocumentElement.RemoveChild(node);
            }
            return true;
        }
    }
}
