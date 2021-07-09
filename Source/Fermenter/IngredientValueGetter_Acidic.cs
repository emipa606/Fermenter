using RimWorld;
using Verse;

namespace Fermenter
{
    // Token: 0x0200000C RID: 12
    public class IngredientValueGetter_Acidic : IngredientValueGetter
    {
        // Token: 0x06000076 RID: 118 RVA: 0x0000451F File Offset: 0x0000271F
        public override float ValuePerUnitOf(ThingDef t)
        {
            if (t.IsNutritionGivingIngestible)
            {
                return t.GetStatValueAbstract(StatDefOf.Nutrition);
            }

            if (t.defName == "FRMT_AceticAcidBacteria")
            {
                return 1f;
            }

            return 0f;
        }

        // Token: 0x06000077 RID: 119 RVA: 0x00004554 File Offset: 0x00002754
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
}