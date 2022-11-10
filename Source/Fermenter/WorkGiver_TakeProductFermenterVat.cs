using Verse;

namespace Fermenter;

public class WorkGiver_TakeProductFermenterVat : WorkGiver_TakeProductFermenter
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("FermenterVat"));
}