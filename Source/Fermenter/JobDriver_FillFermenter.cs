using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Fermenter
{
	// Token: 0x0200000E RID: 14
	public class JobDriver_FillFermenter : JobDriver
	{
		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600007C RID: 124 RVA: 0x000046A8 File Offset: 0x000028A8
		protected Thing Fermenter
		{
			get
			{
				return this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600007D RID: 125 RVA: 0x000046CC File Offset: 0x000028CC
		protected Thing Ingredient
		{
			get
			{
				return this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		// Token: 0x0600007E RID: 126 RVA: 0x000046F0 File Offset: 0x000028F0
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.Fermenter, this.job, 1, -1, null, errorOnFailed) && this.pawn.Reserve(this.Ingredient, this.job, 1, -1, null, errorOnFailed);
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00004741 File Offset: 0x00002941
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Pawn actor = base.GetActor();
			CompFerment comp = this.Fermenter.TryGetComp<CompFerment>();
			this.FailOn(() => comp == null);
			this.FailOn(() => !comp.Filling);
			this.FailOn(() => !comp.producing);
			this.FailOn(() => comp.spaceLeft <= 0);
			this.FailOn(() => comp.Fermented);
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
			Toil ingrToil = Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
			yield return ingrToil;
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(ingrToil, TargetIndex.B, TargetIndex.None, true, null);
			yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.A);
			yield return Toils_General.Wait(200, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return new Toil
			{
				initAction = delegate()
				{
					if (!this.AddIngredient(actor, this.Fermenter, this.Ingredient))
					{
						this.EndJobWith(JobCondition.Incompletable);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			yield break;
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00004754 File Offset: 0x00002954
		private bool AddIngredient(Pawn actor, Thing Fermenter, Thing Ingredient)
		{
			CompFerment comp = Fermenter.TryGetComp<CompFerment>();
			if (comp != null)
			{
				int space = comp.spaceLeft;
				if (space > 0)
				{
					if (Ingredient.stackCount <= space)
					{
						comp.spaceLeft -= Ingredient.stackCount;
						comp.ingredients += Ingredient.stackCount;
						Ingredient.Destroy(DestroyMode.Vanish);
					}
					else
					{
						comp.spaceLeft = 0;
						comp.ingredients += Ingredient.stackCount - space;
						Ingredient.stackCount -= space;
						GenPlace.TryPlaceThing(Ingredient, actor.Position, actor.Map, ThingPlaceMode.Near, null, null, default(Rot4));
					}
				}
			}
			return false;
		}

		// Token: 0x0400002C RID: 44
		private const TargetIndex FermenterInd = TargetIndex.A;

		// Token: 0x0400002D RID: 45
		private const TargetIndex IngredientInd = TargetIndex.B;

		// Token: 0x0400002E RID: 46
		private const int Duration = 200;
	}
}
