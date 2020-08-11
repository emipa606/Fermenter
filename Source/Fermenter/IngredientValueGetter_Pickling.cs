using System;
using RimWorld;
using Verse;

namespace Fermenter
{
	// Token: 0x0200000D RID: 13
	public class IngredientValueGetter_Pickling : IngredientValueGetter
	{
		// Token: 0x06000079 RID: 121 RVA: 0x000045E4 File Offset: 0x000027E4
		public override float ValuePerUnitOf(ThingDef t)
		{
			if (t.IsNutritionGivingIngestible)
			{
				return t.GetStatValueAbstract(StatDefOf.Nutrition, null);
			}
			if (t.defName == "FRMT_Vinegar")
			{
				return 1f;
			}
			return 0f;
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00004618 File Offset: 0x00002818
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
