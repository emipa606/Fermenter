using RimWorld;
using Verse;

namespace Fermenter;

public class IngredientValueGetter_Pickling : IngredientValueGetter
{
    public override float ValuePerUnitOf(ThingDef t)
    {
        if (t.IsNutritionGivingIngestible)
        {
            return t.GetStatValueAbstract(StatDefOf.Nutrition);
        }

        return t.defName == "FRMT_Vinegar" ? 1f : 0f;
    }

    public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing)
    {
        string desc;
        if (!ing.IsFixedIngredient)
        {
            desc = "BillRequiresNutrition".Translate(ing.GetBaseCount()) + " (" + ing.filter.Summary + ")";
        }
        else
        {
            desc = "BillRequires".Translate(ing.GetBaseCount(), ing.filter.Summary);
        }

        return desc;
    }
}