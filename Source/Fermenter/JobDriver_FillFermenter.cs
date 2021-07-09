using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Fermenter
{
    // Token: 0x0200000E RID: 14
    public class JobDriver_FillFermenter : JobDriver
    {
        // Token: 0x0400002C RID: 44
        private const TargetIndex FermenterInd = TargetIndex.A;

        // Token: 0x0400002D RID: 45
        private const TargetIndex IngredientInd = TargetIndex.B;

        // Token: 0x0400002E RID: 46
        private const int Duration = 200;

        // Token: 0x17000016 RID: 22
        // (get) Token: 0x0600007C RID: 124 RVA: 0x000046A8 File Offset: 0x000028A8
        protected Thing Fermenter => job.GetTarget(TargetIndex.A).Thing;

        // Token: 0x17000017 RID: 23
        // (get) Token: 0x0600007D RID: 125 RVA: 0x000046CC File Offset: 0x000028CC
        protected Thing Ingredient => job.GetTarget(TargetIndex.B).Thing;

        // Token: 0x0600007E RID: 126 RVA: 0x000046F0 File Offset: 0x000028F0
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Fermenter, job, 1, -1, null, errorOnFailed) &&
                   pawn.Reserve(Ingredient, job, 1, -1, null, errorOnFailed);
        }

        // Token: 0x0600007F RID: 127 RVA: 0x00004741 File Offset: 0x00002941
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var actor = GetActor();
            var comp = Fermenter.TryGetComp<CompFerment>();
            this.FailOn(() => comp == null);
            this.FailOn(() => !comp.Filling);
            this.FailOn(() => !comp.producing);
            this.FailOn(() => comp.spaceLeft <= 0);
            this.FailOn(() => comp.Fermented);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
            var ingrToil = Toils_Reserve.Reserve(TargetIndex.B);
            yield return ingrToil;
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true)
                .FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(ingrToil, TargetIndex.B, TargetIndex.None, true);
            yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.A);
            yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .WithProgressBarToilDelay(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate
                {
                    if (!AddIngredient(actor, Fermenter, Ingredient))
                    {
                        EndJobWith(JobCondition.Incompletable);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        // Token: 0x06000080 RID: 128 RVA: 0x00004754 File Offset: 0x00002954
        private bool AddIngredient(Pawn actor, Thing fermenter, Thing ingredient)
        {
            var comp = fermenter.TryGetComp<CompFerment>();
            if (comp == null)
            {
                return false;
            }

            var space = comp.spaceLeft;
            if (space <= 0)
            {
                return false;
            }

            if (ingredient.stackCount <= space)
            {
                comp.spaceLeft -= ingredient.stackCount;
                comp.ingredients += ingredient.stackCount;
                ingredient.Destroy();
            }
            else
            {
                comp.spaceLeft = 0;
                comp.ingredients += ingredient.stackCount - space;
                ingredient.stackCount -= space;
                GenPlace.TryPlaceThing(ingredient, actor.Position, actor.Map, ThingPlaceMode.Near);
            }

            return false;
        }
    }
}