using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Fermenter
{
	// Token: 0x0200000F RID: 15
	public class JobDriver_TakeProductFermenter : JobDriver
	{
		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000082 RID: 130 RVA: 0x00004808 File Offset: 0x00002A08
		protected Thing Fermenter
		{
			get
			{
				return this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000083 RID: 131 RVA: 0x0000482C File Offset: 0x00002A2C
		protected Thing Product
		{
			get
			{
				return this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		// Token: 0x06000084 RID: 132 RVA: 0x0000484D File Offset: 0x00002A4D
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.Fermenter, this.job, 1, -1, null, true);
		}

		// Token: 0x06000085 RID: 133 RVA: 0x0000486F File Offset: 0x00002A6F
		protected override IEnumerable<Toil> MakeNewToils()
		{
			CompFerment comp = this.Fermenter.TryGetComp<CompFerment>();
			this.FailOn(() => comp == null);
			this.FailOn(() => !comp.Removing && !comp.Spoiled && !comp.Cancelling);
			this.FailOn(() => !comp.Fermented && (!comp.Cancelling || comp.ingredients < 0));
			this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
			yield return Toils_General.Wait(200, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return new Toil
			{
				initAction = delegate()
				{
					Thing thing = this.TakeOutProduct(this.GetActor(), this.Fermenter);
					if (thing != null)
					{
						GenPlace.TryPlaceThing(thing, this.pawn.Position, this.Map, ThingPlaceMode.Near, null, null, default(Rot4));
						StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
						IntVec3 c;
						if (StoreUtility.TryFindBestBetterStoreCellFor(thing, this.pawn, this.Map, currentPriority, this.pawn.Faction, out c, true))
						{
							this.job.SetTarget(TargetIndex.B, thing);
							this.job.count = thing.stackCount;
							this.job.SetTarget(TargetIndex.C, c);
							return;
						}
					}
					this.EndJobWith(JobCondition.Incompletable);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
			yield return Toils_Reserve.Reserve(TargetIndex.C, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
			Toil carry = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
			yield return carry;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carry, true, false);
			yield break;
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00004880 File Offset: 0x00002A80
		private Thing TakeOutProduct(Pawn pawn, Thing Fermenter)
		{
			Thing newthing = null;
			CompFerment comp = Fermenter.TryGetComp<CompFerment>();
			if (comp != null && ((comp.products > 0 && (comp.Spoiled || comp.Removing)) || (comp.ingredients > 0 && comp.Cancelling)))
			{
				ThingDef Output;
				if (comp.Cancelling)
				{
					Output = comp.inputResourceDef;
				}
				else if (comp.Spoiled)
				{
					Output = comp.spoilProductDef;
				}
				else
				{
					Output = comp.outputProductDef;
				}
				if (Output != null)
				{
					int stackNum;
					if (!comp.Cancelling)
					{
						if (comp.products >= Output.stackLimit)
						{
							stackNum = Output.stackLimit;
						}
						else
						{
							stackNum = comp.products;
						}
					}
					else if (comp.ingredients >= Output.stackLimit)
					{
						stackNum = Output.stackLimit;
					}
					else
					{
						stackNum = comp.ingredients;
					}
					if (stackNum > 0)
					{
						newthing = ThingMaker.MakeThing(Output, null);
						newthing.stackCount = stackNum;
						if (!comp.Cancelling)
						{
							comp.products -= stackNum;
						}
						else
						{
							comp.ingredients -= stackNum;
						}
						comp.spaceLeft += stackNum;
					}
				}
			}
			return newthing;
		}

		// Token: 0x0400002F RID: 47
		private const TargetIndex FermenterInd = TargetIndex.A;

		// Token: 0x04000030 RID: 48
		private const TargetIndex ProductToHaulInd = TargetIndex.B;

		// Token: 0x04000031 RID: 49
		private const TargetIndex StorageCellInd = TargetIndex.C;

		// Token: 0x04000032 RID: 50
		private const int Duration = 200;
	}
}
