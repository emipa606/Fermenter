using RimWorld;
using Verse;
using Verse.AI;

namespace Fermenter;

public class WorkGiver_TakeProductFermenter : WorkGiver_Scanner
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
        return compFerment != null && (compFerment.Removing || compFerment.Spoiled || compFerment.Cancelling) &&
               (compFerment.Fermented || compFerment.Cancelling && compFerment.ingredients > 0) && !t.IsBurning() &&
               !t.IsForbidden(pawn) &&
               pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, forced);
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return new Job(DefDatabase<JobDef>.GetNamed("TakeProductFermenter"), t);
    }
}