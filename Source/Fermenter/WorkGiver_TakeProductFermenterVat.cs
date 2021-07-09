using Verse;

namespace Fermenter
{
    // Token: 0x02000003 RID: 3
    public class WorkGiver_TakeProductFermenterVat : WorkGiver_TakeProductFermenter
    {
        // Token: 0x17000002 RID: 2
        // (get) Token: 0x06000003 RID: 3 RVA: 0x00002069 File Offset: 0x00000269
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("FermenterVat"));
    }
}