using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Fermenter;

public class JobDriver_FillFermenter : JobDriver
{
    private const TargetIndex FermenterInd = TargetIndex.A;

    private const TargetIndex IngredientInd = TargetIndex.B;

    private const int Duration = 200;

    protected Thing Fermenter => job.GetTarget(TargetIndex.A).Thing;

    protected Thing Ingredient => job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Fermenter, job, 1, -1, null, errorOnFailed) &&
               pawn.Reserve(Ingredient, job, 1, -1, null, errorOnFailed);
    }

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