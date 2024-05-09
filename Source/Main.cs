using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using Verse;

namespace GeneTools
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        static readonly bool HARactive = (LoadedModManager.RunningModsListForReading
            .Any<ModContentPack>((Predicate<ModContentPack>)(x => x.Name.Contains("Humanoid Alien Races")))
            || LoadedModManager.RunningModsListForReading
            .Any<ModContentPack>((Predicate<ModContentPack>)(x => x.PackageId == "erdelf.HumanoidAlienRaces"))
            );
        static Main() 
        {
            Harmony harmony = new Harmony("GeneTools");
            try
            {
                ((Action)(() =>
                {
                    harmony.Patch((MethodBase)AccessTools.Method(typeof(PawnGenerator), "GetBodyTypeFor"), postfix: new HarmonyMethod(typeof(GtPatches.GtGetBodyTypeFor), "Postfix"));
                    harmony.Patch((MethodBase)AccessTools.Method(typeof(Pawn_GeneTracker), "Notify_GenesChanged"), postfix: new HarmonyMethod(typeof(GtPatches.GtNotify_GenesChanged), "Postfix"));
                    harmony.Patch((MethodBase)AccessTools.Method(typeof(LifeStageWorker_HumanlikeAdult), "Notify_LifeStageStarted"), prefix: new HarmonyMethod(typeof(GtPatches.GtNotify_LifeStageStarted), "Prefix"));
                    harmony.Patch((MethodBase)AccessTools.Method(typeof(ApparelRequirement), "AllowedForPawn"), postfix: new HarmonyMethod(typeof(GtPatches.GtAllowedForPawn), "Postfix"));
                    harmony.Patch((MethodBase)AccessTools.Method(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain"), postfix: new HarmonyMethod(typeof(GtPatches.GtApparelScoreGain), "Postfix"));
                    harmony.Patch((MethodBase)AccessTools.Method(typeof(PawnApparelGenerator), "CanUsePair"), postfix: new HarmonyMethod(typeof(GtPatches.GtCanUsePair), "Postfix"));
                    harmony.Patch((MethodBase)AccessTools.Method(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel"), prefix: new HarmonyMethod(typeof(GtPatches.GtResolveApparelGraphic), "Prefix"));
                    harmony.Patch(typeof(EquipmentUtility).GetMethod("CanEquip", new[] { typeof(Thing), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool) }), postfix: new HarmonyMethod(typeof(GtPatches.GtCanEquip), "Postfix"));
                    harmony.Patch((MethodBase)AccessTools.Method(typeof(HeadTypeDef), "GetGraphic"), prefix: new HarmonyMethod(typeof(GtPatches.GtHeadDefGetGraphic), "Prefix"));
                    harmony.Patch((MethodBase)AccessTools.Method(typeof(HairDef), "GraphicFor"), prefix: new HarmonyMethod(typeof(GtPatches.GtHairDefGetGraphicFor), "Prefix"));
                    harmony.Patch((MethodBase)AccessTools.Method(typeof(PawnRenderNode_Body), "GraphicFor"), postfix: new HarmonyMethod(typeof(GtPatches.GtBodyNodeGraphicFor), "Postfix"));
                    harmony.Patch((MethodBase)AccessTools.Method(typeof(FurDef), "GetFurBodyGraphicPath"), postfix: new HarmonyMethod(typeof(GtPatches.GtResolveFurGraphic), "Postfix"));
                    if (!GeneToolsSettings.disableTranspile)
                    { harmony.Patch((MethodBase)AccessTools.Method(typeof(Pawn_StoryTracker), "ExposeData"), transpiler: new HarmonyMethod(typeof(GtPatches.GtDisableLoadChildBodyFailsafe), "Transpiler"));
                    } else
                    { harmony.Patch((MethodBase)AccessTools.Method(typeof(Pawn_StoryTracker), "ExposeData"), postfix: new HarmonyMethod(typeof(GtPatches.GtFixChildBodyOnLoad), "Postfix"));
                    }

                    if (HARactive)
                    {
                        Log.Message("[GeneTools] Detected HAR!");
                        HARPatcher.patch();
                    }
                    Log.Message("[GeneTools] Active.");
                }))();
            }
            catch (TypeLoadException e) {
                Log.Error("[GeneTools] TypeLoadException:");
                Log.Error(e.StackTrace);
            }
        } 
    }
}
