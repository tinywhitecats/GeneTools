using Verse;
using UnityEngine;

namespace GeneTools
{
    public class GeneToolsSettings : ModSettings
    {
        public static bool HARfix = true;      
        public static bool bypassHelmets = false;
        public static bool bypassApparel = false;
        public static bool disableTranspile = false;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref bypassHelmets, "bypassHelmets");
            Scribe_Values.Look(ref bypassApparel, "bypassApparel");
            Scribe_Values.Look(ref disableTranspile, "disableTranspile");
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
            listingStandard.CheckboxLabeled("Bypass Helmet Check", ref GeneToolsSettings.bypassHelmets, "Allows equipping of helmets regardless of headtype settings.");
            listingStandard.CheckboxLabeled("Bypass Apparel Check", ref GeneToolsSettings.bypassApparel, "Allows equipping of non-helmet apparel regardless of bodytype settings. Likely to cause bugs - bodytype should implement a substitute instead.");
            listingStandard.CheckboxLabeled("Disable Child Transpiler", ref GeneToolsSettings.disableTranspile, "REQUIRES RESTART: Rimworld resets child bodies when you load a save, regardless of the actual data in the save. I completely disabled this with a transpiler. If you turn off the transpiler GeneTools will use a postfix patch instead to generate a new body, which may randomize it if there are multiple options. The option is here in case the transpiler is failing.");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return "Gene Tools";
        }
    }

}
