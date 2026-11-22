using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Fermenter;

public class JobDriver_FillFermenter : JobDriver
{
    private const TargetIndex FermenterInd = TargetIndex.A;

    private const TargetIndex IngredientInd = TargetIndex.B;

    private const int Duration = 200;

    private Thing Fermenter => job.GetTarget(FermenterInd).Thing;

    private Thing Ingredient => job.GetTarget(IngredientInd).Thing;

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
        this.FailOnDespawnedNullOrForbidden(FermenterInd);
        this.FailOnBurningImmobile(FermenterInd);
        this.FailOnDestroyedNullOrForbidden(IngredientInd);
        var ingrToil = Toils_Reserve.Reserve(IngredientInd);
        yield return ingrToil;
        yield return Toils_Reserve.Reserve(FermenterInd);
        yield return Toils_Goto.GotoThing(IngredientInd, PathEndMode.ClosestTouch)
            .FailOnSomeonePhysicallyInteracting(IngredientInd).FailOnDestroyedNullOrForbidden(IngredientInd);
        yield return Toils_Haul.StartCarryThing(IngredientInd, false, true)
            .FailOnDestroyedNullOrForbidden(IngredientInd);
        yield return Toils_Haul.CheckForGetOpportunityDuplicate(ingrToil, IngredientInd, TargetIndex.None, true);
        yield return Toils_Haul.CarryHauledThingToCell(FermenterInd);
        yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(FermenterInd)
            .WithProgressBarToilDelay(FermenterInd);
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

    private static bool AddIngredient(Pawn actor, Thing fermenter, Thing ingredient)
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