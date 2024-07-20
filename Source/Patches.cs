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
            if (GeneToolsSettings.bypassApparel)
                return true;
            //Can I cache this or something? ._.
            //Log.Message("GtCanPawnEquip for " + pawn.Name + "," + apparel.defName);
            BodyTypeDef bodyType = pawn.story.bodyType;
            HeadTypeDef headType = pawn.story.headType;
            bool appaHasMod = apparel.HasModExtension<GeneToolsApparelDef>();
            bool isHat = apparel.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) || apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead) || apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes) ? true : false;

            if (isHat)
            {
                //Apparel hard whitelist check
                if (appaHasMod && apparel.GetModExtension<GeneToolsApparelDef>().forcedHeadTypes != null && !apparel.GetModExtension<GeneToolsApparelDef>().forcedHeadTypes.Contains(headType))
                    return false;

                //Head enforcement check
                if (!appaHasMod || !apparel.GetModExtension<GeneToolsHeadTypeDef>().enforceHelmets)
                    return true;

                //Invisible check
                bool headHasMod = headType.HasModExtension<GeneToolsHeadTypeDef>();
                bool isInvisible = apparel.apparel.wornGraphicPath == BaseContent.PlaceholderImagePath || apparel.apparel.wornGraphicPath == BaseContent.PlaceholderGearImagePath || apparel.apparel.wornGraphicPath == "" ? true : false;
                if (isInvisible && !(headHasMod && headType.GetModExtension<GeneToolsHeadTypeDef>().enforceOnInvisibleApparel))
                    return true;

                //Apparel whitelist check
                if (appaHasMod && apparel.GetModExtension<GeneToolsApparelDef>().allowedHeadTypes.Contains(headType) || apparel.GetModExtension<GeneToolsApparelDef>().forcedHeadTypes.Contains(headType))
                    return true;

                return false;

            } else
            {
                //Apparel hard whitelist check
                bool bodyHasMod = bodyType.HasModExtension<GeneToolsBodyTypeDef>();
                BodyTypeDef bodyTypeSub = !bodyHasMod ? null : bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody;
                if (appaHasMod) {
                    List<BodyTypeDef> forcedTypes = apparel.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes;
                    if (forcedTypes != null && !forcedTypes.Contains(bodyType) && !forcedTypes.Contains(bodyTypeSub))
                        return false;
                }

                //Vanilla body bypass check
                //Pending a more elegant solution, femaleBB is in here. The built-in patch in femaleBB is going to throw errors on startup though...
                string[] vanillaBodies = { "Child", "Male", "Female", "Thin", "Fat", "Hulk", "FemaleBB" };
                if (vanillaBodies.Contains(bodyType.defName) || (bodyTypeSub != null && vanillaBodies.Contains(bodyTypeSub.defName)))
                    return true;

                //Invisible check
                bool isInvisible = apparel.apparel.wornGraphicPath == BaseContent.PlaceholderImagePath || apparel.apparel.wornGraphicPath == BaseContent.PlaceholderGearImagePath || apparel.apparel.wornGraphicPath == "" ? true : false;
                if (isInvisible && !(bodyHasMod && bodyType.GetModExtension<GeneToolsBodyTypeDef>().enforceOnInvisibleApparel))
                    return true;

                //Apparel whitelist existance check
                if (!appaHasMod)
                    return false;

                //Apparel whitelist check
                List<BodyTypeDef> allowedTypes = apparel.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes;
                if (allowedTypes.Contains(bodyType) || allowedTypes.Contains(bodyTypeSub)) 
                    return true;
                return false;
            }
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
                            ___pawn.Drawer.renderer.SetAllGraphicsDirty();
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
            public static void Prefix(ref Pawn pawn, ref LifeStageDef previousLifeStage)
            {
                //Log.Message("Notify_LifeStageStarted for " + pawn.Name);
                if (previousLifeStage == null) //wtf? addiction checker calls this with null lifeStage on load. 1.5 bug?
                    return;
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
                if (___pawn.genes != null && ___pawn.DevelopmentalStage == DevelopmentalStage.Child)
                {
                    List<Gene> genesListForReading = ___pawn.genes.GenesListForReading;
                    foreach (Gene gene in genesListForReading)
                    {
                        if (gene.Active && gene.def.HasModExtension<GeneToolsGeneDef>()
                            && gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild != null)
                        {
                            __instance.bodyType = gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild[UnityEngine.Random.Range(0, gene.def.GetModExtension<GeneToolsGeneDef>().forcedBodyTypesChild.Count)];
                            //Log.Message("[GeneTools] overwrote child body on load.");
                            return;
                        }
                    }
                }
            }
        }
        //1.5 very helpfully added getGraphic to headDefs, so this is much cleaner
        public static class GtHeadDefGetGraphic
        {
            [HarmonyPrefix]
            public static bool Prefix(ref HeadTypeDef __instance, ref Pawn pawn,/* ref Color color,*/ ref Graphic_Multi __result)
            {
                //Log.Message("GtHeadDefGetGraphic for " + pawn.Name);
                //humanlike check unneccesary?
                if (pawn.RaceProps.Humanlike && __instance.HasModExtension<GeneToolsHeadTypeDef>())
                {
                    Color skinColor = pawn.story.SkinColor;
                    Color hairColor = pawn.story.HairColor;
                    //List<KeyValuePair<Color, Graphic_Multi>> graphics = Traverse.Create(__instance).Field("graphics").GetValue() as List<KeyValuePair<Color, Graphic_Multi>>;
                    if (__instance.GetModExtension<GeneToolsHeadTypeDef>().useShader == true)
                    {
                        Graphic_Multi graphic_Multi = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(pawn.story.headType.graphicPath, ShaderDatabase.CutoutComplex, Vector2.one, skinColor, hairColor);
                        //graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphic_Multi));
                        __result = graphic_Multi;
                        return false;
                    }
                    else if (__instance.GetModExtension<GeneToolsHeadTypeDef>().colorHead == false)
                    {
                        Graphic_Multi graphic_Multi = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(pawn.story.headType.graphicPath, ShaderUtility.GetSkinShader(pawn), Vector2.one, Color.white);
                        //graphics.Add(new KeyValuePair<Color, Graphic_Multi>(color, graphic_Multi));
                        __result = graphic_Multi;
                        return false;
                    }
                }
                return true;
            }
        }
        public static class GtHairDefGetGraphicFor
        {
            [HarmonyPrefix]
            public static bool Prefix(ref HairDef __instance, ref Pawn pawn, ref Color color, ref Graphic __result)
            {
                //Log.Message("GtHairDefGetGraphicFor for " + pawn.Name);
                //humanlike check unneccesary?
                if (pawn.RaceProps.Humanlike && __instance.HasModExtension<GeneToolsHairDef>())
                {
                    Color skinColor = pawn.story.SkinColor;
                    Color hairColor = pawn.story.HairColor;
                    if (__instance.GetModExtension<GeneToolsHairDef>().useSkinColor == true)
                    {
                        Graphic_Multi graphic_Multi = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(__instance.texPath, ShaderDatabase.CutoutComplex, Vector2.one, hairColor, skinColor);
                        __result = graphic_Multi;
                        return false;
                    }
                }
                return true;
            }
        }
        //Unlike this, for bodies
        public static class GtBodyNodeGraphicFor
        {
            [HarmonyPostfix]
            public static void Postfix(ref PawnRenderNode_Body __instance, ref Pawn pawn, ref Graphic __result)
            {
                //Log.Message("GtBodyNodeGraphicFor for " + pawn.Name);
                //humanlike check unneccesary?
                if (pawn.RaceProps.Humanlike && pawn.story.bodyType.HasModExtension<GeneToolsBodyTypeDef>()
                    && pawn.Drawer.renderer.CurRotDrawMode != RotDrawMode.Dessicated
                    && !(ModsConfig.AnomalyActive && pawn.IsMutant && !pawn.mutant.Def.bodyTypeGraphicPaths.NullOrEmpty<BodyTypeGraphicData>())
                    && !(ModsConfig.AnomalyActive && pawn.IsCreepJoiner && pawn.story.bodyType != null && !pawn.creepjoiner.form.bodyTypeGraphicPaths.NullOrEmpty<BodyTypeGraphicData>())
                    )
                {
                    Color skinColor = pawn.story.SkinColor;
                    Color hairColor = pawn.story.HairColor;
                    if (pawn.story.bodyType.GetModExtension<GeneToolsBodyTypeDef>().useShader == true)
                        __result = GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyNakedGraphicPath, ShaderDatabase.CutoutComplex, Vector2.one, skinColor, hairColor);
                    else if (pawn.story.bodyType.GetModExtension<GeneToolsBodyTypeDef>().colorBody == false)
                        __result = GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(pawn), Vector2.one, Color.white);
                }
            }
        }
        /* Prevent player from making pawn equip apparel that doesn't fit the body */
        public static class GtCanEquip 
        {
            [HarmonyPostfix]
            public static void Postfix(ref Thing thing, ref Pawn pawn, ref string cantReason, ref bool __result)
            {
                //Log.Message("GtCanEquip for " + pawn.Name);
                if (pawn.RaceProps.Humanlike && __result && thing.def.IsApparel && __result)
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
                //Check if applicable
                if (!bodyType.HasModExtension<GeneToolsBodyTypeDef>() 
                    || bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody == null)
                    return;
                //Check if it's an unforced vanilla body or forced custom
                string[] vanillaBodies = { "Child", "Male", "Female", "Thin", "Fat", "Hulk", "FemaleBB" };
                bool appHasMod = apparel.def.HasModExtension<GeneToolsApparelDef>();
                if (vanillaBodies.Contains(bodyType.defName) && 
                    (!appHasMod || (apparel.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes == null) || apparel.def.GetModExtension<GeneToolsApparelDef>().forcedBodyTypes.Contains(bodyType)))
                    return;
                //Check if it's an allowed body
                if (appHasMod && apparel.def.GetModExtension<GeneToolsApparelDef>().allowedBodyTypes.Contains(bodyType))
                    return;
                //Change to substitute
                bodyType = bodyType.GetModExtension<GeneToolsBodyTypeDef>().substituteBody;
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
        public static class GtBodyGraphicForPrefixHARPostfix
        {
            //Avoids an error being thrown by HAR
            [HarmonyPrefix]
            public static bool Prefix(object[] __args)
            {
                return !((Pawn)__args[1]).story.bodyType.HasModExtension<GeneToolsBodyTypeDef>();
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