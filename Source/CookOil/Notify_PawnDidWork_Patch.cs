using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CookOil
{
    // Token: 0x02000004 RID: 4
    [HarmonyPatch(typeof(Bill), "Notify_PawnDidWork")]
    public class Notify_PawnDidWork_Patch
    {
        // Token: 0x06000008 RID: 8 RVA: 0x000024B8 File Offset: 0x000006B8
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

            var jd = jobDriver;
            if (jd is JobDriver_DoBill bill && CookOilUtility.OilIsHandy(p, out var Oil) && bill.workLeft > 0f)
            {
                CookOilUtility.UseOil(p, bill, Oil);
            }
        }
    }
}