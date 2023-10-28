using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using UnityEngine;
using System.Reflection.Emit;
using System.Linq;

namespace GeneTools 
{
    public static class GtPatches
    {
        public static bool GtCanPawnEquip(ref ThingDef apparel, ref Pawn pawn)
        {
            //Log.Message("GtCanPawnEquip for " + pawn.Name + "," + apparel.defName);
            BodyTypeDef bodyType = pawn.story.bodyType;
            HeadTypeDef headType = pawn.story.headType;
            bool useSubstitute = apparel.HasModExtension<GeneToolsApparelDef>() && apparel.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null && !apparel.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes.Contains(bodyType) && bodyType.HasModExtension<GeneToolsBodyTypeDef>() && bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody != null && apparel.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes.Contains(bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody) ? true : false;
            bool useSubstituteForced = apparel.HasModExtension<GeneToolsApparelDef>() && apparel.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes != null && !apparel.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes.Contains(bodyType) && bodyType.HasModExtension<GeneToolsBodyTypeDef>() && bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody != null && apparel.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes.Contains(bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody) ? true : false;
            bool isHat = apparel.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) || apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead) || apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes) ? true : false;
            bool isInvisible = apparel.apparel.wornGraphicPath == BaseContent.PlaceholderImagePath || apparel.apparel.wornGraphicPath == BaseContent.PlaceholderGearImagePath || apparel.apparel.wornGraphicPath == "" ? true : false;
            
            bool enforceInvisibleBody = bodyType.HasModExtension<GeneToolsBodyTypeDef>() && bodyType.GetModExtension<GeneToolsBodyTypeDef>().enforceOnInvisibleApparel;
            bool enforceInvisibleHead = headType.HasModExtension<GeneToolsHeadTypeDef>() && headType.GetModExtension<GeneToolsHeadTypeDef>().enforceOnInvisibleApparel;
            //Log.Message(bodyType.defName + "," + headType.defName + "," + useSubstitute + "," + isHat + "," + isInvisible);

            if (!GeneToolsSettings.bypassApparel && (!isInvisible || enforceInvisibleBody) && !isHat && apparel.HasModExtension<GeneToolsApparelDef>() && apparel.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes != null)
            {
                List<BodyTypeDef> forcedBodies = apparel.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes;
                if (!forcedBodies.Contains(bodyType) && !useSubstituteForced)
                    return false;
            }
            if (!GeneToolsSettings.bypassApparel && (!isInvisible || enforceInvisibleBody) && !isHat && apparel.HasModExtension<GeneToolsApparelDef>() && apparel.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null)
            {
                List<BodyTypeDef> allowedBodies = apparel.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes;
                if (!allowedBodies.Contains(bodyType) && !useSubstitute)
                    return false;
            }
            if (!GeneToolsSettings.bypassHelmets && (!isInvisible || enforceInvisibleHead) && isHat && apparel.HasModExtension<GeneToolsApparelDef>() && apparel.GetModExtension<GeneToolsApparelDef>().forcedHeadTypes != null)
            {
                List<HeadTypeDef> forcedHeads = apparel.GetModExtension<GeneToolsApparelDef>().forcedHeadTypes;
                if (!forcedHeads.Contains(headType))
                    return false;
            }
            if (!GeneToolsSettings.bypassHelmets && (!isInvisible || enforceInvisibleHead) && isHat && apparel.HasModExtension<GeneToolsApparelDef>() && apparel.GetModExtension<GeneToolsApparelDef>().allowedHeadTypes != null)
            {
                List<HeadTypeDef> allowedHeads = apparel.GetModExtension<GeneToolsApparelDef>().allowedHeadTypes;
                if (!allowedHeads.Contains(headType))
                    return false;
            }
            //Log.Message("TRUE");
            return true;
        }
        /* Set body type to any forced by genes */
        public static class GtGetBodyTypeFor
        {
            [HarmonyPostfix]
            public static void Postfix(ref Pawn pawn, ref BodyTypeDef __result)
            {
                //Don't forget, this has a side effect of possibly changing head type!
                //Log.Message("GtGetBodyTypeFor for " + pawn.Name);
                if (pawn != null && pawn.genes != null) //har bugfix - addiction checker calls this??? wack
                {
                    List<Gene> genesListForReading = pawn.genes.GenesListForReading;
                    foreach (Gene gene in genesListForReading)
                    {
                        if (gene.def.HasModExtension<GeneToolsGeneDef>() && gene.Active)
                        {
                            if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesBaby != null && pawn.DevelopmentalStage == DevelopmentalStage.Baby)
                                __result = gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesBaby[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesBaby.Count)];
                            else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild != null && pawn.DevelopmentalStage == DevelopmentalStage.Child)
                                __result = gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild.Count)];
                            else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesFemale != null && pawn.gender == Gender.Female && pawn.DevelopmentalStage == DevelopmentalStage.Adult)
                                __result = gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesFemale[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesFemale.Count)];
                            else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesMale != null && pawn.gender == Gender.Male && pawn.DevelopmentalStage == DevelopmentalStage.Adult)
                                __result = gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesMale[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesMale.Count)];
                            else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypes != null && pawn.DevelopmentalStage == DevelopmentalStage.Adult)
                                __result = gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypes[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypes.Count)];
                            if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesBaby != null && pawn.DevelopmentalStage == DevelopmentalStage.Baby)
                                pawn.story.headType = gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesBaby[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesBaby.Count)];
                            else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesChild != null && pawn.DevelopmentalStage == DevelopmentalStage.Child)
                                pawn.story.headType = gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesChild[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesChild.Count)];
                            else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesFemale != null && pawn.gender == Gender.Female && pawn.DevelopmentalStage == DevelopmentalStage.Adult)
                                pawn.story.headType = gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesFemale[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesFemale.Count)];
                            else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesMale != null && pawn.gender == Gender.Male && pawn.DevelopmentalStage == DevelopmentalStage.Adult)
                                pawn.story.headType = gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesMale[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypesMale.Count)];
                            else if (gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypes != null && pawn.DevelopmentalStage == DevelopmentalStage.Adult)
                                pawn.story.headType = gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypes[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedHeadTypes.Count)];
                        }
                    }
                }
            }
        }
        /* Update body if genes are changed */
        public static class GtNotify_GenesChanged
        {
            [HarmonyPostfix]
            public static void Postfix(ref Pawn ___pawn, ref GeneDef addedOrRemovedGene)
            {
                //Log.Message("GtNotify_GenesChanged for " + ___pawn.Name + " of " + addedOrRemovedGene.defName);
                if (___pawn.RaceProps.Humanlike)
                {
                    if (addedOrRemovedGene != null && addedOrRemovedGene.HasModExtension<GeneToolsGeneDef>())
                    {
                        // if ___pawn.genes.HasGene(addedOrRemovedGene) && ___pawn.genes.GetGene(addedOrRemovedGene).Active &&*/
                        // Possible for gene to not exist on pawn here - also this would break making it inactive anyway. What was I thinking? 
                        if (
                            addedOrRemovedGene.GetModExtension<GeneToolsGeneDef>().forcedBodyTypes != null 
                            || addedOrRemovedGene.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesFemale != null
                            || addedOrRemovedGene.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesMale != null
                            || addedOrRemovedGene.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild != null 
                            || addedOrRemovedGene.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesBaby != null)
                        {
                            ___pawn.story.bodyType = Verse.PawnGenerator.GetBodyTypeFor(___pawn);
                            ___pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
                        }
                    }
                }
            }
        }
        /* Update on age progression */
        public static class GtNotify_LifeStageStarted
        {
            //only runs on adults - LifeStageWorker_HumanlikeAdult
            [HarmonyPrefix]
            public static void Prefix(ref Pawn pawn)
            {
                //Log.Message("Notify_LifeStageStarted for " + pawn.Name);
                if (pawn.RaceProps.Humanlike)
                {
                    HeadTypeDef oldHead = pawn.story.headType;
                    BodyTypeDef newDef = Verse.PawnGenerator.GetBodyTypeFor(pawn);
                    if (newDef != pawn.story.bodyType || oldHead != pawn.story.headType)
                    {
                        //Log.Message("Change detected!");
                        pawn.story.bodyType = newDef;
                        Pawn p = pawn;
                        if (pawn.apparel != null)
                            pawn.apparel.DropAllOrMoveAllToInventory(
                                (Apparel apparel) => !GtCanPawnEquip(ref apparel.def, ref p) || !apparel.def.apparel.developmentalStageFilter.Has(DevelopmentalStage.Adult));
                    }
                    //___pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
                }
            }
        }
        public static class GtFixChildBodyOnLoad
        {   //Pawn_StoryTracker:ExposeData resets child bodies when the game loads
            //this is reloads it - possible randomizing the body
            //Obsolete by GtDisableLoadChildBodyFailsafe but present as an option in mod settings just in case transpiler breaks
            [HarmonyPostfix]
            public static void Postfix(ref Pawn_StoryTracker __instance, ref Pawn ___pawn)
            {
                if (___pawn.genes != null)
                {
                    List<Gene> genesListForReading = ___pawn.genes.GenesListForReading;
                    foreach (Gene gene in genesListForReading)
                    {
                        if (gene.Active && gene.def.HasModExtension<GeneToolsGeneDef>()
                            && gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild != null
                            && ___pawn.DevelopmentalStage == DevelopmentalStage.Child)
                        {
                            __instance.bodyType = gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild.Count)];
                            Log.Message("[GeneTools] overwrote child body on load.");
                            return;
                        }
                    }
                }
            }
        }
        /* Apply changes to body and head */
        public static class GtResolveAllGraphics
        {
            [HarmonyPostfix]
            [HarmonyAfter(new string[] { "butterfish.hairmoddingplus" })]
            public static void Postfix(ref PawnGraphicSet __instance)
            {
                //Log.Message("GtResolveAllGraphics for " + __instance.pawn.Name);
                if (__instance.pawn.RaceProps.Humanlike)
                {
                    Color skinColor = __instance.pawn.story.SkinColor;
                    Color hairColor = __instance.pawn.story.HairColor;
                    if (__instance.pawn.story.bodyType.HasModExtension<GeneToolsBodyTypeDef>() && __instance.pawn.story.bodyType.GetModExtension<GeneToolsBodyTypeDef>().useShader == true)
                        __instance.nakedGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.bodyType.bodyNakedGraphicPath, ShaderDatabase.CutoutComplex, Vector2.one, skinColor, hairColor);
                    else if (__instance.pawn.story.bodyType.HasModExtension<GeneToolsBodyTypeDef>() && __instance.pawn.story.bodyType.GetModExtension<GeneToolsBodyTypeDef>().colorBody == false)
                        __instance.nakedGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.bodyType.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(__instance.pawn.story.SkinColorOverriden), Vector2.one, Color.white);
                    if (__instance.pawn.story.headType.HasModExtension<GeneToolsHeadTypeDef>() && __instance.pawn.story.headType.GetModExtension<GeneToolsHeadTypeDef>().useShader == true)
                        __instance.headGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.headType.graphicPath, ShaderDatabase.CutoutComplex, Vector2.one, skinColor, hairColor);
                    else if (__instance.pawn.story.headType.HasModExtension<GeneToolsHeadTypeDef>() && __instance.pawn.story.headType.GetModExtension<GeneToolsHeadTypeDef>().colorHead == false)
                        __instance.headGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.pawn.story.headType.graphicPath, ShaderUtility.GetSkinShader(__instance.pawn.story.SkinColorOverriden), Vector2.one, Color.white);
                }
                GtCalculateHairMats.Postfix(__instance);
            }
        }
        /* Prevent player from making pawn equip apparel that doesn't fit the body */
        public static class GtCanEquip 
        {
            [HarmonyPostfix]
            public static void Postfix(ref Thing thing, ref Pawn pawn, ref string cantReason, ref bool __result)
            {
                //Log.Message("GtCanEquip for " + pawn.Name);
                if (pawn.RaceProps.Humanlike && __result && thing.def.IsApparel)
                {
                    bool isHat = thing.def.apparel.LastLayer == ApparelLayerDefOf.Overhead 
                        || thing.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) 
                        || thing.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead) 
                        || thing.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes) ? true : false;
                    __result = GtCanPawnEquip(ref thing.def, ref pawn);
                    if (!__result)
                        cantReason = (string)"does not fit " + (!isHat ? pawn.story.bodyType + " body" : pawn.story.headType + " head");
                }
            }
        }
        /* Check if apparel can be used for a pawn kind */
        public static class GtAllowedForPawn 
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn p, ThingDef apparel, ref bool __result)
            {
                //Log.Message("GtAllowedForPawn for " + p.Name);
                if (p.RaceProps.Humanlike && __result)
                    __result = GtCanPawnEquip(ref apparel, ref p);
            }
        }
        /* Prevent pawns from equiping clothes themselves that don't fit */
        internal static class GtApparelScoreGain
        {
            [HarmonyPostfix]
            public static void Postfix(ref Pawn pawn, ref Apparel ap, ref List<float> wornScoresCache, ref float __result)
            {
                //Log.Message("GtApparelScoreGain for " + pawn.Name);
                if (pawn.RaceProps.Humanlike)
                    __result = GtCanPawnEquip(ref ap.def, ref pawn) ? __result : - 1000f;
            }
        }

        /* Prevent pawns from spawning in with apparel that doesn't fit */
        internal static class GtCanUsePair
        {
            [HarmonyPostfix]
            public static void Postfix(ThingStuffPair pair, Pawn pawn, float moneyLeft, bool allowHeadgear, int fixedSeed, ref bool __result)
            {
                //Log.Message("GtCanUsePair for " + pawn.Name);
                if (__result && pawn.RaceProps.Humanlike && !GtCanPawnEquip(ref pair.thing, ref pawn))
                {
                    pair.thing.apparel.canBeGeneratedToSatisfyWarmth = false;
                    pair.thing.apparel.canBeGeneratedToSatisfyToxicEnvironmentResistance = false; //where is this used?
                    __result = false;
                }
            }
        }
        /* Allow body to use substitute apparel texture if available */
        [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
        public static class GtResolveApparelGraphic
        {
            [HarmonyPrefix]
            public static void Prefix(ref Apparel apparel, ref BodyTypeDef bodyType)
            {
                //Log.Message("GtResolveApparelGraphic for " + bodyType.defName);
                bool useSubstitute = apparel.def.HasModExtension<GeneToolsApparelDef>() 
                    && apparel.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes != null 
                    && !apparel.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes.Contains(bodyType) 
                    && bodyType.HasModExtension<GeneToolsBodyTypeDef>() 
                    && bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody != null 
                    && apparel.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes.Contains(bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody)
                    ? true : false;
                if (useSubstitute) //This applies to HAR aliens! Is this bad?
                {
                    bodyType = bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody;
                }
            }
        }
        /* Allow body to use substitute fur gene texture if available */
        [HarmonyPatch(typeof(FurDef), "GetFurBodyGraphicPath")]
        public static class GtResolveFurGraphic
        {
            [HarmonyPostfix]
            public static void Postfix(ref Pawn pawn, ref string __result, FurDef __instance)
            {
                if (__result == null && pawn.story.bodyType.HasModExtension<GeneToolsBodyTypeDef>() &&
                    pawn.story.bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody != null)
                {
                    for (int i = 0; i < __instance.bodyTypeGraphicPaths.Count; i++)
                    {
                        if (__instance.bodyTypeGraphicPaths[i].bodyType == 
                            pawn.story.bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody)
                        {
                            __result = __instance.bodyTypeGraphicPaths[i].texturePath;
                            break;
                        }
                    }
                }
            }
        }
        /* Patch to HAR's substitute body system */
        public static class GtTryGetBodyTypeFallbackHARPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref Pawn pawn, out BodyTypeDef def, ref bool __result)
            {
                //Log.Message("GtTryGetBodyTypeFallbackHARPatch for " + pawn.Name);
                def = (BodyTypeDef)null;
                if (pawn == null) //continue to original
                    return true;
                if (pawn.RaceProps.Humanlike)
                {
                    if (pawn.story.bodyType.HasModExtension<GeneToolsBodyTypeDef>() && pawn.story.bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody != null)
                    {
                        def = pawn.story.bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody;
                        __result = true;
                        return false; //skip original if human with patch (!!!)
                    }   //I think this skip could be removed with a transpiler?
                }       //As is, I think humans with Gene Tools substitutes can't use HAR substitutes?
                return true;
            }
        }
        public static class GtResolveAllGraphicsStopHAR
        {
            //HAR will absolutely crash the game if the ResolveAllGraphics patch runs on a GT pawn.
            //This stops the patch and forces it to continue to vanilla functions.
            [HarmonyPrefix]
            public static bool Prefix(object[] __args, ref bool __result)
            {
                PawnGraphicSet __instance = (PawnGraphicSet)__args[0]; //Can't access __instance normally since Harmony interprets that literally
                //Log.Message("GtResolveAllGraphicsStopHAR for " + __instance.pawn.Name);
                if (__instance.pawn.RaceProps.Humanlike)
                {
                    bool isGTHuman = false;
                    List<Gene> genesListForReading = __instance.pawn.genes.GenesListForReading;
                    foreach (Gene gene in genesListForReading)
                    {
                        if (gene.def.HasModExtension<GeneToolsGeneDef>())
                        {
                            isGTHuman = true;
                            break;
                        }
                    }
                    __result = isGTHuman;
                    return !isGTHuman;
                }
                return true;
            }
        }
        public static class GtCalculateHairMats {
            [HarmonyAfter(new string[] { "butterfish.hairmoddingplus" })]
            [HarmonyPostfix]
            public static void Postfix(PawnGraphicSet __instance)
            {
                if (__instance.pawn.RaceProps.Humanlike
                    && __instance.pawn.story.hairDef.HasModExtension<GeneToolsHairDef>() 
                    && __instance.pawn.story.hairDef.GetModExtension<GeneToolsHairDef>().useSkinColor == true)
                {
                    string hairTexturePath = __instance.pawn.story.hairDef.texPath;
                    __instance.hairGraphic = hairTexturePath == null ? null : GraphicDatabase.Get<Graphic_Multi>(hairTexturePath, ShaderDatabase.CutoutComplex, Vector2.one, __instance.pawn.story.HairColor, __instance.pawn.story.SkinColor);
                }
            }
        }
        public static class GtDisableLoadChildBodyFailsafe
        {
            //Rimworld resets child bodies on save load
            //This disables that feature
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                Log.Message("[GeneTools] Transpiling Pawn_StoryTracker.ExposeData");
                var foundBodyTypeBranch = false;
                var codes = new List<CodeInstruction>(instructions);
                int i = 0;
                for (; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && codes[i + 1].opcode == OpCodes.Ldsfld)
                    {
                        if (codes[i + 1].operand.ToString() == "RimWorld.BodyTypeDef Child")
                        {
                            foundBodyTypeBranch = true;
                            break;
                        }
                    }
                }
                //The problematic code:
                /*if (ModsConfig.BiotechActive && this.pawn.DevelopmentalStage.Child() && this.bodyType != BodyTypeDefOf.Child) {
                    ...
                    this.bodyType = BodyTypeDefOf.Child; }

                This swaps 'this.bodyType != BodyTypeDefOf.Child' into constant false
                 */
                if (foundBodyTypeBranch)
                {
                    codes[i].opcode = OpCodes.Nop; //Disable parameter setup
                    codes[i - 1].opcode = OpCodes.Nop; //Disable first parameter
                    codes[i + 1].opcode = OpCodes.Nop; //Disable second parameter
                    codes[i + 2].opcode = OpCodes.Br; //Switch branch to unconditional
                    Log.Message("[GeneTools] Transpile success.");
                } else
                {
                    Log.Error("[GeneTools] Transpile failed! Not able to find branch sequence!");
                }

                return codes.AsEnumerable();
            }
        }
    }
}