using System.Collections.Generic;
using Verse;

namespace Fermenter;

[StaticConstructorOnStartup]
internal static class CheckFermentationDefs
{
    static CheckFermentationDefs()
    {
        LongEventHandler.QueueLongEvent(CheckDefs, "LibraryStartup", false, null);
    }

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

    private static bool CheckThing(string FdefName, List<ThingDef> things)
    {
        var fermentDef = DefDatabase<ThingDef>.GetNamed(FdefName, false);
        return fermentDef != null && things.Contains(fermentDef);
    }

    private static bool CheckIngestible(string FdefName)
    {
        var fermentDef = DefDatabase<ThingDef>.GetNamed(FdefName, false);
        return fermentDef?.ingestible != null;
    }

    private static void ErrorMsg(string FName, string ttype, string thing)
    {
        Log.Error("Fermenter.ErrThing".Translate(FName, ttype, thing));
    }

    private static void ErrorMsgIng(string FName, string ttype, string thing)
    {
        Log.Warning("Fermenter.ErrThingIng".Translate(FName, ttype, thing));
    }

    private static bool CheckResearch(string res, List<ResearchProjectDef> projects)
    {
        var thingRes = DefDatabase<ResearchProjectDef>.GetNamed(res, false);
        return thingRes != null && projects.Contains(thingRes);
    }

    private static void RErrorMsg(string FName, string FRes)
    {
        Log.Error("Fermenter.ErrResearch".Translate(FName, FRes));
    }
}