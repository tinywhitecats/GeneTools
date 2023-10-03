using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using System.Runtime;
using System.Reflection;
using System.Collections;
using Verse.Noise;

namespace GeneTools
{
    public class GeneToolsSettings : ModSettings
    {
        public static bool HARfix = true;      
        public static bool bypassHelmets = false;
        public static bool bypassApparel = false;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref HARfix, "HARfix");
            Scribe_Values.Look(ref bypassHelmets, "bypassHelmets");
            Scribe_Values.Look(ref bypassApparel, "bypassApparel");     
            base.ExposeData();
        }

    }

    public class GeneToolsMod : Mod
    {
        GeneToolsSettings settings;
        public GeneToolsMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<GeneToolsSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Enable Experimental HAR fix (Requires Restart)", ref GeneToolsSettings.HARfix, "Prevents HAR from affecting the rendering of a human pawn if said pawn has any genes or body types affected by this mod. Prevents this mod from affecting any alien race's rendering or apparel restrictions. Fixes rendering issues and error log spam with HAR but may cause issues with any other mod that affects pawn rendering.");
            listingStandard.CheckboxLabeled("Bypass Helmet Check", ref GeneToolsSettings.bypassHelmets, "Allows equipping of helmets regardless of headtype settings.");
            listingStandard.CheckboxLabeled("Bypass Apparel Check", ref GeneToolsSettings.bypassApparel, "Allows equipping of non-helmet apparel regardless of bodytype settings. Likely to cause bugs - bodytype should implement a substitute instead.");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return "Gene Tools";
        }
    }
}
