using System;
using Verse;

namespace Fermenter
{
	// Token: 0x02000002 RID: 2
	public class WorkGiver_FillFermenterVat : WorkGiver_FillFermenter
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(ThingDef.Named("FermenterVat"));
			}
		}
	}
}
