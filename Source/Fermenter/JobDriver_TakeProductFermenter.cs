using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Fermenter
{
    // Token: 0x0200000F RID: 15
    public class JobDriver_TakeProductFermenter : JobDriver
    {
        // Token: 0x0400002F RID: 47
        private const TargetIndex FermenterInd = TargetIndex.A;

        // Token: 0x04000030 RID: 48
        private const TargetIndex ProductToHaulInd = TargetIndex.B;

        // Token: 0x04000031 RID: 49
        private const TargetIndex StorageCellInd = TargetIndex.C;

        // Token: 0x04000032 RID: 50
        private const int Duration = 200;

        // Token: 0x17000018 RID: 24
        // (get) Token: 0x06000082 RID: 130 RVA: 0x00004808 File Offset: 0x00002A08
        protected Thing Fermenter => job.GetTarget(TargetIndex.A).Thing;

        // Token: 0x17000019 RID: 25
        // (get) Token: 0x06000083 RID: 131 RVA: 0x0000482C File Offset: 0x00002A2C
        protected Thing Product => job.GetTarget(TargetIndex.B).Thing;

        // Token: 0x06000084 RID: 132 RVA: 0x0000484D File Offset: 0x00002A4D
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Fermenter, job);
        }

        // Token: 0x06000085 RID: 133 RVA: 0x0000486F File Offset: 0x00002A6F
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var comp = Fermenter.TryGetComp<CompFerment>();
            this.FailOn(() => comp == null);
            this.FailOn(() => !comp.Removing && !comp.Spoiled && !comp.Cancelling);
            this.FailOn(() => !comp.Fermented && (!comp.Cancelling || comp.ingredients < 0));
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .WithProgressBarToilDelay(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate
                {
                    var thing = TakeOutProduct(Fermenter);
                    if (thing != null)
                    {
                        GenPlace.TryPlaceThing(thing, pawn.Position, Map, ThingPlaceMode.Near);
                        var currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
                        if (StoreUtility.TryFindBestBetterStoreCellFor(thing, pawn, Map, currentPriority, pawn.Faction,
                            out var c))
                        {
                            job.SetTarget(TargetIndex.B, thing);
                            job.count = thing.stackCount;
                            job.SetTarget(TargetIndex.C, c);
                            return;
                        }
                    }

                    EndJobWith(JobCondition.Incompletable);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return Toils_Reserve.Reserve(TargetIndex.B);
            yield return Toils_Reserve.Reserve(TargetIndex.C);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            var carry = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
            yield return carry;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carry, true);
        }

        // Token: 0x06000086 RID: 134 RVA: 0x00004880 File Offset: 0x00002A80
        private Thing TakeOutProduct(Thing fermenter)
        {
            var comp = fermenter.TryGetComp<CompFerment>();
            if (comp == null || (comp.products <= 0 || !comp.Spoiled && !comp.Removing) &&
                (comp.ingredients <= 0 || !comp.Cancelling))
            {
                return null;
            }

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

            if (Output == null)
            {
                return null;
            }

            int stackNum;
            if (!comp.Cancelling)
            {
                stackNum = comp.products >= Output.stackLimit ? Output.stackLimit : comp.products;
            }
            else if (comp.ingredients >= Output.stackLimit)
            {
                stackNum = Output.stackLimit;
            }
            else
            {
                stackNum = comp.ingredients;
            }

            if (stackNum <= 0)
            {
                return null;
            }

            var newthing = ThingMaker.MakeThing(Output);
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

            return newthing;
        }
    }
}