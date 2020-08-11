using System;
using Verse;

namespace Fermenter
{
	// Token: 0x02000009 RID: 9
	public class FermentDef : Def
	{
		// Token: 0x0400001E RID: 30
		public string Product = "";

		// Token: 0x0400001F RID: 31
		public string Resource = "";

		// Token: 0x04000020 RID: 32
		public string SpoilProduct = "";

		// Token: 0x04000021 RID: 33
		public float FermentHours = 48f;

		// Token: 0x04000022 RID: 34
		public string ResearchProject = "";

		// Token: 0x04000023 RID: 35
		public bool FermenterNeeded;
	}
}
