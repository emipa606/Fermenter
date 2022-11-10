using RimWorld;
using Verse;
using Verse.AI;

namespace Fermenter;

public class WorkGiver_FillFermenter : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("Fermenter"));

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (!t.Spawned)
        {
            return false;
        }

        var compFerment = t.TryGetComp<CompFerment>();
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

        if (t.IsForbidden(pawn) ||
            !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, forced))
        {
            return false;
        }

        if (t is Building_Fermenter)
        {
            if (t.IsBrokenDown())
            {
                return false;
            }

            var compPow = t.TryGetComp<CompPowerTrader>();
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

        if (FindIngredient(pawn, t, out var ing) != null)
        {
            return !t.IsBurning();
        }

        JobFailReason.Is("Fermenter.NoIngredients".Translate(ing.label.CapitalizeFirst()));
        return false;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var t2 = FindIngredient(pawn, t, out _);
        var num = t.TryGetComp<CompFerment>().spaceLeft;
        if (num > t2.def.stackLimit)
        {
            num = t2.def.stackLimit;
        }

        return new Job(DefDatabase<JobDef>.GetNamed("FillFermenter"), t, t2)
        {
            count = num
        };
    }

    private Thing FindIngredient(Pawn pawn, Thing fermenter, out ThingDef ingredientdef)
    {
        var defname = fermenter is Building_Fermenter buildingFermenter
            ? buildingFermenter.FermentResource
            : (fermenter as Building_FermenterVat)?.FermentResource;

        ingredientdef = DefDatabase<ThingDef>.GetNamed(defname, false);
        var filter = new ThingFilter();
        filter.SetAllow(ingredientdef, true);

        bool Validator(Thing x)
        {
            return !x.IsForbidden(pawn) && pawn.CanReserve(x) && filter.Allows(x);
        }

        return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, filter.BestThingRequest,
            PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, Validator);
    }
}