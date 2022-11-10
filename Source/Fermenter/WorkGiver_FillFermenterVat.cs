using Verse;

namespace Fermenter;

public class WorkGiver_FillFermenterVat : WorkGiver_FillFermenter
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("FermenterVat"));
}