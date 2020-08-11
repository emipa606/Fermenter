using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CookOil
{
	// Token: 0x02000002 RID: 2
	public class CookOilUtility
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public static List<string> OilDefNames()
		{
			return new List<string>
			{
				"FRMT_CornOil",
				"FRMT_OliveOil",
				"FRMT_SesameOil",
				"FRMT_SunflowerOil",
				"FRMT_Butter",
				"FRMT_InsectButter",
				"SmokeleafButter",
				"SmokeleafSeedOil",
				"VCE_SmokeleafButter"
			};
		}

		// Token: 0x06000002 RID: 2 RVA: 0x000020C8 File Offset: 0x000002C8
		public static bool ProductsHaveMeals(Bill bill)
		{
			List<ThingDefCountClass> list2;
			if (bill == null)
			{
				list2 = null;
			}
			else
			{
				RecipeDef recipe = bill.recipe;
				list2 = ((recipe != null) ? recipe.products : null);
			}
			List<ThingDefCountClass> list = list2;
			if (list != null && list.Count > 0)
			{
				foreach (ThingDefCountClass tdcc in list)
				{
					if (((tdcc != null) ? tdcc.thingDef : null) != null && tdcc.thingDef.IsIngestible)
					{
						ThingDef thingDef = tdcc.thingDef;
						bool flag;
						if (thingDef == null)
						{
							flag = false;
						}
						else
						{
							IngestibleProperties ingestible = thingDef.ingestible;
							FoodTypeFlags? foodTypeFlags = (ingestible != null) ? new FoodTypeFlags?(ingestible.foodType) : null;
							FoodTypeFlags foodTypeFlags2 = FoodTypeFlags.Meal;
							flag = (foodTypeFlags.GetValueOrDefault() == foodTypeFlags2 & foodTypeFlags != null);
						}
						if (!flag)
						{
							ThingDef thingDef2 = tdcc.thingDef;
							bool flag2;
							if (thingDef2 == null)
							{
								flag2 = false;
							}
							else
							{
								IngestibleProperties ingestible2 = thingDef2.ingestible;
								FoodTypeFlags? foodTypeFlags = (ingestible2 != null) ? new FoodTypeFlags?(ingestible2.foodType) : null;
								FoodTypeFlags foodTypeFlags2 = FoodTypeFlags.Kibble;
								flag2 = (foodTypeFlags.GetValueOrDefault() == foodTypeFlags2 & foodTypeFlags != null);
							}
							if (!flag2)
							{
								continue;
							}
						}
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x000021FC File Offset: 0x000003FC
		public static bool OilIsHandy(Pawn p, out Thing oil)
		{
			oil = null;
			List<Thing> candidates = new List<Thing>();
			if (p != null && ((p != null) ? p.Map : null) != null && p.Spawned)
			{
				Map map = p.Map;
				IntVec3 position = p.Position;
				List<IntVec3> cells = GenAdj.CellsAdjacent8Way(p).ToList<IntVec3>();
				if (cells.Count > 0)
				{
					foreach (IntVec3 cell in cells)
					{
						if (cell.IsValid && cell.InBounds(map))
						{
							List<Thing> things = cell.GetThingList(map);
							if (things.Count > 0)
							{
								foreach (Thing thing in things)
								{
									if (CookOilUtility.OilDefNames().Contains(thing.def.defName) && !thing.IsForbidden(p) && !thing.IsBurning())
									{
										candidates.Add(thing);
									}
								}
							}
						}
					}
				}
			}
			if (candidates.Count > 0)
			{
				List<Thing> sortedthings = (from t in candidates
				orderby t.MarketValue descending
				select t).ToList<Thing>();
				if (sortedthings.Count > 0)
				{
					oil = sortedthings.First<Thing>();
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x0000237C File Offset: 0x0000057C
		public static void UseOil(Pawn p, JobDriver_DoBill jd, Thing oil)
		{
			float amountReduced = CookOilUtility.GetOilReduction(oil) * 2f;
			if (jd.workLeft >= amountReduced)
			{
				jd.workLeft -= amountReduced;
			}
			else
			{
				jd.workLeft = 0f;
			}
			if (oil.def.defName == "FRMT_InsectButter")
			{
				bool flag;
				if (p == null)
				{
					flag = (null != null);
				}
				else
				{
					Pawn_NeedsTracker needs = p.needs;
					flag = (((needs != null) ? needs.joy : null) != null);
				}
				if (flag && p.needs.joy.CurLevel < p.needs.joy.MaxLevel)
				{
					p.needs.joy.CurLevel = Math.Min(p.needs.joy.CurLevel * 1.02f, p.needs.joy.MaxLevel);
				}
			}
			if (oil.stackCount > 1)
			{
				oil.stackCount--;
				return;
			}
			oil.Destroy(DestroyMode.Vanish);
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002467 File Offset: 0x00000667
		public static float GetOilReduction(Thing oil)
		{
			return Mathf.Lerp(20f, 80f, Math.Max(0f, Math.Min(1f, oil.MarketValue / 12f)));
		}

		// Token: 0x04000001 RID: 1
		public const int OilTicks = 120;

		// Token: 0x04000002 RID: 2
		public const float OilValueMax = 12f;
	}
}
