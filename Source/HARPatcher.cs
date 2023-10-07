using AlienRace.ApparelGraphics;
using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace GeneTools
{
    public class HARPatcher
    {
        public static void patch() 
        {
            Harmony harmony = new Harmony("GeneTools");
            try
            {
                ((Action)(() =>
                {
                    harmony.Patch((MethodBase)AccessTools.Method(typeof(ApparelGraphicsOverrides), "TryGetBodyTypeFallback"), prefix: new HarmonyMethod(typeof(GtPatches.GtTryGetBodyTypeFallbackHARPatch), "Prefix"));
                    harmony.Patch((MethodBase)AccessTools.Method(typeof(AlienRace.HarmonyPatches), "ResolveAllGraphicsPrefix"), prefix: new HarmonyMethod(typeof(GtPatches.GtResolveAllGraphicsStopHAR), "Prefix"));
                }))();
            }
            catch (TypeLoadException e) {
                Log.Error("[GeneTools] TypeLoadException:");
                Log.Error(e.StackTrace);
            }
        } 
    }
}
