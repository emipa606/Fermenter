using System;
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
			if (p.IsHashIntervalTick(120) && CookOilUtility.ProductsHaveMeals(__instance))
			{
				Job job2;
				if (p == null)
				{
					job2 = null;
				}
				else
				{
					Pawn_JobTracker jobs = p.jobs;
					job2 = ((jobs != null) ? jobs.curJob : null);
				}
				Job job = job2;
				if (job != null && job.def == JobDefOf.DoBill && job.targetA.HasThing)
				{
					Thing t = job.targetA.Thing;
					if (!t.DestroyedOrNull() && t is Building && t.def.building.isMealSource)
					{
						JobDriver jobDriver;
						if (p == null)
						{
							jobDriver = null;
						}
						else
						{
							Pawn_JobTracker jobs2 = p.jobs;
							jobDriver = ((jobs2 != null) ? jobs2.curDriver : null);
						}
						JobDriver jd = jobDriver;
						Thing Oil;
						if (jd != null && jd is JobDriver_DoBill && CookOilUtility.OilIsHandy(p, out Oil) && (jd as JobDriver_DoBill).workLeft > 0f)
						{
							CookOilUtility.UseOil(p, jd as JobDriver_DoBill, Oil);
						}
					}
				}
			}
		}
	}
}
