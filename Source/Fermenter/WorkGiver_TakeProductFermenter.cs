using RimWorld;
using Verse;
using Verse.AI;

namespace Fermenter
{
    // Token: 0x02000012 RID: 18
    public class WorkGiver_TakeProductFermenter : WorkGiver_Scanner
    {
        // Token: 0x1700001C RID: 28
        // (get) Token: 0x06000092 RID: 146 RVA: 0x00004DBA File Offset: 0x00002FBA
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("Fermenter"));

        // Token: 0x1700001D RID: 29
        // (get) Token: 0x06000093 RID: 147 RVA: 0x00004DCB File Offset: 0x00002FCB
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        // Token: 0x06000094 RID: 148 RVA: 0x00004DD0 File Offset: 0x00002FD0
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

        // Token: 0x06000095 RID: 149 RVA: 0x00004E53 File Offset: 0x00003053
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(DefDatabase<JobDef>.GetNamed("TakeProductFermenter"), t);
        }
    }
}