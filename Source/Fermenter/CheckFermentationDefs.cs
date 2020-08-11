using System;
using System.Collections.Generic;
using Verse;

namespace Fermenter
{
	// Token: 0x02000006 RID: 6
	[StaticConstructorOnStartup]
	internal static class CheckFermentationDefs
	{
		// Token: 0x06000047 RID: 71 RVA: 0x0000338D File Offset: 0x0000158D
		static CheckFermentationDefs()
		{
			LongEventHandler.QueueLongEvent(new Action(CheckFermentationDefs.CheckDefs), "LibraryStartup", false, null, true);
		}

		// Token: 0x06000048 RID: 72 RVA: 0x000033A8 File Offset: 0x000015A8
		private static void CheckDefs()
		{
			float lowHrs = 24f;
			float highHrs = 720f;
			List<ThingDef> things = DefDatabase<ThingDef>.AllDefsListForReading;
			List<ResearchProjectDef> projects = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
			int count = 0;
			foreach (FermentDef fermentDef in DefDatabase<FermentDef>.AllDefsListForReading)
			{
				string FName = fermentDef.defName;
				string FProduct = fermentDef.Product;
				string FResource = fermentDef.Resource;
				string FSpoilProd = fermentDef.SpoilProduct;
				float FHours = fermentDef.FermentHours;
				string FResearch = fermentDef.ResearchProject;
				if (!CheckFermentationDefs.CheckThing(FProduct, things))
				{
					CheckFermentationDefs.ErrorMsg(FName, "Fermenter.product".Translate(), FProduct);
				}
				if (!CheckFermentationDefs.CheckThing(FResource, things))
				{
					CheckFermentationDefs.ErrorMsg(FName, "Fermenter.resource".Translate(), FResource);
				}
				if (FSpoilProd != "" && !CheckFermentationDefs.CheckThing(FSpoilProd, things))
				{
					CheckFermentationDefs.ErrorMsg(FName, "Fermenter.spoilprod".Translate(), FSpoilProd);
				}
				if (FHours < lowHrs)
				{
					Log.Warning("Fermenter.WarnLowHrs".Translate(FName, FHours.ToString("F2"), lowHrs.ToString("F2")), false);
				}
				else if (FHours > highHrs)
				{
					Log.Warning("Fermenter.WarnHighHrs".Translate(FName, FHours.ToString("F2"), highHrs.ToString("F2")), false);
				}
				if (FResearch != "" && !CheckFermentationDefs.CheckResearch(FResearch, projects))
				{
					CheckFermentationDefs.RErrorMsg(FName, FResearch);
				}
				count++;
			}
			Log.Message("Fermenter.CheckProcess".Translate(count.ToString()), false);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00003594 File Offset: 0x00001794
		private static bool CheckThing(string FdefName, List<ThingDef> things)
		{
			ThingDef fermentDef = DefDatabase<ThingDef>.GetNamed(FdefName, false);
			return fermentDef != null && things.Contains(fermentDef);
		}

		// Token: 0x0600004A RID: 74 RVA: 0x000035B8 File Offset: 0x000017B8
		private static bool CheckIngestible(string FdefName)
		{
			ThingDef fermentDef = DefDatabase<ThingDef>.GetNamed(FdefName, false);
			return fermentDef != null && ((fermentDef != null) ? fermentDef.ingestible : null) != null;
		}

		// Token: 0x0600004B RID: 75 RVA: 0x000035E1 File Offset: 0x000017E1
		private static void ErrorMsg(string FName, string ttype, string thing)
		{
			Log.Error("Fermenter.ErrThing".Translate(FName, ttype, thing), false);
		}

		// Token: 0x0600004C RID: 76 RVA: 0x0000360A File Offset: 0x0000180A
		private static void ErrorMsgIng(string FName, string ttype, string thing)
		{
			Log.Warning("Fermenter.ErrThingIng".Translate(FName, ttype, thing), false);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00003634 File Offset: 0x00001834
		private static bool CheckResearch(string res, List<ResearchProjectDef> projects)
		{
			ResearchProjectDef thingRes = DefDatabase<ResearchProjectDef>.GetNamed(res, false);
			return thingRes != null && projects.Contains(thingRes);
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00003658 File Offset: 0x00001858
		private static void RErrorMsg(string FName, string FRes)
		{
			Log.Error("Fermenter.ErrResearch".Translate(FName, FRes), false);
		}
	}
}
