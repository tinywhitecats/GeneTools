using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace GeneTools
{
    public class HARPatcher
    {
        //This class exists because it's not possible to instantiate it without HAR installed
        // C# runtime hates the reference to AlienRace stuff in here
        public static void patch() 
        {
            Harmony harmony = new Harmony("GeneTools");
            try
            {
                ((Action)(() =>
                {
                    harmony.Patch((MethodBase)AccessTools.Method(typeof(AlienRace.AlienRenderTreePatches), "BodyGraphicForPrefix"), prefix: new HarmonyMethod(typeof(GtPatches.GtBodyGraphicForPrefixHARPostfix), "Prefix"));
                    //harmony.Patch((MethodBase)AccessTools.Method(typeof(AlienRace.AlienRenderTreePatches), "HeadGraphicForPrefix"), postfix: new HarmonyMethod(typeof(GtPatches.GtHeadGraphicForPrefixHARPostfix), "Postfix"));
                }))();
            }
            catch (TypeLoadException e) {
                Log.Error("[GeneTools] TypeLoadException:");
                Log.Error(e.StackTrace);
            }
        } 
    }
}
