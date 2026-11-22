using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Fermenter;

public class JobDriver_TakeProductFermenter : JobDriver
{
    private const TargetIndex FermenterInd = TargetIndex.A;

    private const TargetIndex ProductToHaulInd = TargetIndex.B;

    private const TargetIndex StorageCellInd = TargetIndex.C;

    private const int Duration = 200;

    private Thing Fermenter => job.GetTarget(FermenterInd).Thing;

    protected Thing Product => job.GetTarget(ProductToHaulInd).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Fermenter, job);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var comp = Fermenter.TryGetComp<CompFerment>();
        this.FailOn(() => comp == null);
        this.FailOn(() => !comp.Removing && !comp.Spoiled && !comp.Cancelling);
        this.FailOn(() => !comp.Fermented && (!comp.Cancelling || comp.ingredients < 0));
        this.FailOnDestroyedNullOrForbidden(FermenterInd);
        yield return Toils_Reserve.Reserve(FermenterInd);
        yield return Toils_Goto.GotoThing(FermenterInd, PathEndMode.ClosestTouch);
        yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(FermenterInd)
            .WithProgressBarToilDelay(FermenterInd);
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
                        job.SetTarget(ProductToHaulInd, thing);
                        job.count = thing.stackCount;
                        job.SetTarget(StorageCellInd, c);
                        return;
                    }
                }

                EndJobWith(JobCondition.Incompletable);
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        yield return Toils_Reserve.Reserve(ProductToHaulInd);
        yield return Toils_Reserve.Reserve(StorageCellInd);
        yield return Toils_Goto.GotoThing(ProductToHaulInd, PathEndMode.ClosestTouch);
        yield return Toils_Haul.StartCarryThing(ProductToHaulInd);
        var carry = Toils_Haul.CarryHauledThingToCell(StorageCellInd);
        yield return carry;
        yield return Toils_Haul.PlaceHauledThingInCell(StorageCellInd, carry, true);
    }

    private static Thing TakeOutProduct(Thing fermenter)
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