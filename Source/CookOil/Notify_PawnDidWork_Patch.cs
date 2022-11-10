using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CookOil;

[HarmonyPatch(typeof(Bill), "Notify_PawnDidWork")]
public class Notify_PawnDidWork_Patch
{
    [HarmonyPostfix]
    [HarmonyPriority(800)]
    public static void PostFix(ref Bill __instance, Pawn p)
    {
        if (!p.IsHashIntervalTick(120) || !CookOilUtility.ProductsHaveMeals(__instance))
        {
            return;
        }

        Job job2;
        if (p == null)
        {
            job2 = null;
        }
        else
        {
            var jobs = p.jobs;
            job2 = jobs?.curJob;
        }

        var job = job2;
        if (job == null || job.def != JobDefOf.DoBill || !job.targetA.HasThing)
        {
            return;
        }

        var t = job.targetA.Thing;
        if (t.DestroyedOrNull() || t is not Building || !t.def.building.isMealSource)
        {
            return;
        }

        var jobs2 = p.jobs;
        var jobDriver = jobs2?.curDriver;

        if (jobDriver is JobDriver_DoBill bill && CookOilUtility.OilIsHandy(p, out var Oil) && bill.workLeft > 0f)
        {
            CookOilUtility.UseOil(p, bill, Oil);
        }
    }
}