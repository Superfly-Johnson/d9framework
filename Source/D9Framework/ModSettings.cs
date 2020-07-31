﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace D9Framework
{
    /// <summary>
    /// <c>ModSettings</c> class for D9 Framework. Mainly handles which Harmony patches should be applied and saves all specified settings.
    /// </summary>
    /// <remarks>Static for convenience.</remarks>
    public class D9FModSettings : ModSettings
    {        
        public static bool DEBUG = false; //for release set false by default
        public static bool PrintPatchedMethods => DEBUG && printPatchedMethods;
        public static bool printPatchedMethods = false;

        // despite being public, please don't fuck with these. Access patch application settings with ShouldPatch, and don't touch SettingsUIKeys.
        // They're only public so I can use them in the mod settings screen.
        public static Dictionary<string, bool> PatchApplicationSettings;
        public static Dictionary<string, (string labelKey, string descKey)> SettingsUIKeys;

        public override void ExposeData()
        {
            base.ExposeData();
            PatchApplicationSettings = new Dictionary<string, bool>();
            // backwards compatibility
            if(Scribe.mode != LoadSaveMode.Saving)
            {
                bool cur = false;
                string[] keysToLook = { "ApplyCompFromStuff", "ApplyOrbitalTradeHook", "ApplyDeconstructReturnFix", "ApplyCarryMassFramework", "ApplyNegativeFertilityPatch" };
                foreach(string key in keysToLook)
                {
                    Scribe_Values.Look(ref cur, key);
                    PatchApplicationSettings.Add(key, cur);
                }
            }
            Scribe_Values.Look(ref DEBUG, "debug", false);
            Scribe_Collections.Look(ref PatchApplicationSettings, "Patches");
        }

        public static bool ShouldPatch(string patchkey)
        {
            if (!DEBUG) return true;
            return PatchApplicationSettings[patchkey];
        }
    }
    /// <summary>
    /// <c>Mod</c> class for D9 Framework. Mainly handles the settings screen.
    /// </summary>
    public class D9FrameworkMod : Mod
    {
        D9FModSettings settings;
        public D9FrameworkMod(ModContentPack con) : base(con)
        {
            this.settings = GetSettings<D9FModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled("D9FSettingsDebug".Translate(), ref D9FModSettings.DEBUG, "D9FSettingsDebugTooltip".Translate());
            if (D9FModSettings.DEBUG)
            {
                listing.CheckboxLabeled("D9FSettingsPPM".Translate(), ref D9FModSettings.printPatchedMethods, "D9FSettingsPPMTooltip".Translate());
                listing.Label("D9FSettingsApplyAtOwnRisk".Translate());
                listing.Label("D9FSettingsRestartToApply".Translate());
                listing.Label("D9FSettingsDebugModeRequired".Translate());
                foreach(string key in D9FModSettings.PatchApplicationSettings.Keys)
                {
                    // This probably won't work, but it's worth a try.
                    bool cur = D9FModSettings.PatchApplicationSettings[key];
                    listing.CheckboxLabeled(D9FModSettings.SettingsUIKeys[key].labelKey.Translate(), ref cur, D9FModSettings.SettingsUIKeys[key].descKey.Translate());
                    D9FModSettings.PatchApplicationSettings[key] = cur;
                }
            }
            listing.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "D9FSettingsCategory".Translate();
        }
    }
}
