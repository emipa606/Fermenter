using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace Fermenter
{
	// Token: 0x02000011 RID: 17
	public class WorkGiver_FillFermenter : WorkGiver_Scanner
	{
		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600008C RID: 140 RVA: 0x00004B9A File Offset: 0x00002D9A
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(ThingDef.Named("Fermenter"));
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x0600008D RID: 141 RVA: 0x00004BAB File Offset: 0x00002DAB
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00004BB0 File Offset: 0x00002DB0
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!t.Spawned)
			{
				return false;
			}
			CompFerment compFerment = t.TryGetComp<CompFerment>();
			if (compFerment == null)
			{
				return false;
			}
			if (!compFerment.Filling)
			{
				return false;
			}
			if (!compFerment.producing)
			{
				return false;
			}
			if (compFerment.Fermented || compFerment.spaceLeft <= 0)
			{
				return false;
			}
			if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, forced))
			{
				return false;
			}
			if (t is Building_Fermenter)
			{
				if (t.IsBrokenDown())
				{
					return false;
				}
				CompPowerTrader compPow = t.TryGetComp<CompPowerTrader>();
				if (compPow == null)
				{
					return false;
				}
				if (!compPow.PowerOn)
				{
					return false;
				}
			}
			if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
			{
				return false;
			}
			ThingDef ing;
			if (this.FindIngredient(pawn, t, out ing) == null)
			{
				JobFailReason.Is("Fermenter.NoIngredients".Translate(ing.label.CapitalizeFirst()), null);
				return false;
			}
			return !t.IsBurning();
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00004C9C File Offset: 0x00002E9C
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			ThingDef ing;
			Thing t2 = this.FindIngredient(pawn, t, out ing);
			int num = t.TryGetComp<CompFerment>().spaceLeft;
			if (num > t2.def.stackLimit)
			{
				num = t2.def.stackLimit;
			}
			return new Job(DefDatabase<JobDef>.GetNamed("FillFermenter", true), t, t2)
			{
				count = num
			};
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00004D00 File Offset: 0x00002F00
		private Thing FindIngredient(Pawn pawn, Thing fermenter, out ThingDef ingredientdef)
		{
			string defname;
			if (fermenter is Building_Fermenter)
			{
				defname = (fermenter as Building_Fermenter).FermentResource;
			}
			else
			{
				defname = (fermenter as Building_FermenterVat).FermentResource;
			}
			ingredientdef = DefDatabase<ThingDef>.GetNamed(defname, false);
			ThingFilter filter = new ThingFilter();
			filter.SetAllow(ingredientdef, true);
			Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && filter.Allows(x);
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, filter.BestThingRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
		}
	}
}
