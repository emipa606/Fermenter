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
            LongEventHandler.QueueLongEvent(CheckDefs, "LibraryStartup", false, null);
        }

        // Token: 0x06000048 RID: 72 RVA: 0x000033A8 File Offset: 0x000015A8
        private static void CheckDefs()
        {
            var lowHrs = 24f;
            var highHrs = 720f;
            var things = DefDatabase<ThingDef>.AllDefsListForReading;
            var projects = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
            var count = 0;
            foreach (var fermentDef in DefDatabase<FermentDef>.AllDefsListForReading)
            {
                var FName = fermentDef.defName;
                var FProduct = fermentDef.Product;
                var FResource = fermentDef.Resource;
                var FSpoilProd = fermentDef.SpoilProduct;
                var FHours = fermentDef.FermentHours;
                var FResearch = fermentDef.ResearchProject;
                if (!CheckThing(FProduct, things))
                {
                    ErrorMsg(FName, "Fermenter.product".Translate(), FProduct);
                }

                if (!CheckThing(FResource, things))
                {
                    ErrorMsg(FName, "Fermenter.resource".Translate(), FResource);
                }

                if (FSpoilProd != "" && !CheckThing(FSpoilProd, things))
                {
                    ErrorMsg(FName, "Fermenter.spoilprod".Translate(), FSpoilProd);
                }

                if (FHours < lowHrs)
                {
                    Log.Warning("Fermenter.WarnLowHrs".Translate(FName, FHours.ToString("F2"), lowHrs.ToString("F2")));
                }
                else if (FHours > highHrs)
                {
                    Log.Warning("Fermenter.WarnHighHrs".Translate(FName, FHours.ToString("F2"),
                        highHrs.ToString("F2")));
                }

                if (FResearch != "" && !CheckResearch(FResearch, projects))
                {
                    RErrorMsg(FName, FResearch);
                }

                count++;
            }

            Log.Message("Fermenter.CheckProcess".Translate(count.ToString()));
        }

        // Token: 0x06000049 RID: 73 RVA: 0x00003594 File Offset: 0x00001794
        private static bool CheckThing(string FdefName, List<ThingDef> things)
        {
            var fermentDef = DefDatabase<ThingDef>.GetNamed(FdefName, false);
            return fermentDef != null && things.Contains(fermentDef);
        }

        // Token: 0x0600004A RID: 74 RVA: 0x000035B8 File Offset: 0x000017B8
        private static bool CheckIngestible(string FdefName)
        {
            var fermentDef = DefDatabase<ThingDef>.GetNamed(FdefName, false);
            return fermentDef?.ingestible != null;
        }

        // Token: 0x0600004B RID: 75 RVA: 0x000035E1 File Offset: 0x000017E1
        private static void ErrorMsg(string FName, string ttype, string thing)
        {
            Log.Error("Fermenter.ErrThing".Translate(FName, ttype, thing));
        }

        // Token: 0x0600004C RID: 76 RVA: 0x0000360A File Offset: 0x0000180A
        private static void ErrorMsgIng(string FName, string ttype, string thing)
        {
            Log.Warning("Fermenter.ErrThingIng".Translate(FName, ttype, thing));
        }

        // Token: 0x0600004D RID: 77 RVA: 0x00003634 File Offset: 0x00001834
        private static bool CheckResearch(string res, List<ResearchProjectDef> projects)
        {
            var thingRes = DefDatabase<ResearchProjectDef>.GetNamed(res, false);
            return thingRes != null && projects.Contains(thingRes);
        }

        // Token: 0x0600004E RID: 78 RVA: 0x00003658 File Offset: 0x00001858
        private static void RErrorMsg(string FName, string FRes)
        {
            Log.Error("Fermenter.ErrResearch".Translate(FName, FRes));
        }
    }
}