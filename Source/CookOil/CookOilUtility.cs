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
        // Token: 0x04000001 RID: 1
        public const int OilTicks = 120;

        // Token: 0x04000002 RID: 2
        public const float OilValueMax = 12f;

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
                var recipe = bill.recipe;
                list2 = recipe?.products;
            }

            var list = list2;
            if (list == null || list.Count <= 0)
            {
                return false;
            }

            foreach (var tdcc in list)
            {
                if (tdcc?.thingDef == null || !tdcc.thingDef.IsIngestible)
                {
                    continue;
                }

                var thingDef = tdcc.thingDef;
                bool b;
                if (thingDef == null)
                {
                    b = false;
                }
                else
                {
                    var ingestible = thingDef.ingestible;
                    b = ((ingestible != null ? new FoodTypeFlags?(ingestible.foodType) : null).GetValueOrDefault() ==
                         FoodTypeFlags.Meal) & ((ingestible != null ? new FoodTypeFlags?(ingestible.foodType) : null) !=
                                                null);
                }

                if (b)
                {
                    return true;
                }

                var thingDef2 = tdcc.thingDef;
                bool b1;
                if (thingDef2 == null)
                {
                    b1 = false;
                }
                else
                {
                    var ingestible2 = thingDef2.ingestible;
                    b1 = ((ingestible2 != null ? new FoodTypeFlags?(ingestible2.foodType) : null).GetValueOrDefault() ==
                          FoodTypeFlags.Kibble) &
                         ((ingestible2 != null ? new FoodTypeFlags?(ingestible2.foodType) : null) != null);
                }

                if (!b1)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        // Token: 0x06000003 RID: 3 RVA: 0x000021FC File Offset: 0x000003FC
        public static bool OilIsHandy(Pawn p, out Thing oil)
        {
            oil = null;
            var candidates = new List<Thing>();
            if (p?.Map != null && p.Spawned)
            {
                var map = p.Map;
                var cells = GenAdj.CellsAdjacent8Way(p).ToList();
                if (cells.Count > 0)
                {
                    foreach (var cell in cells)
                    {
                        if (!cell.IsValid || !cell.InBounds(map))
                        {
                            continue;
                        }

                        var things = cell.GetThingList(map);
                        if (things.Count <= 0)
                        {
                            continue;
                        }

                        foreach (var thing in things)
                        {
                            if (OilDefNames().Contains(thing.def.defName) && !thing.IsForbidden(p) &&
                                !thing.IsBurning())
                            {
                                candidates.Add(thing);
                            }
                        }
                    }
                }
            }

            if (candidates.Count <= 0)
            {
                return false;
            }

            var sortedthings = (from t in candidates
                orderby t.MarketValue descending
                select t).ToList();
            if (sortedthings.Count <= 0)
            {
                return false;
            }

            oil = sortedthings.First();
            return true;
        }

        // Token: 0x06000004 RID: 4 RVA: 0x0000237C File Offset: 0x0000057C
        public static void UseOil(Pawn p, JobDriver_DoBill jd, Thing oil)
        {
            var amountReduced = GetOilReduction(oil) * 2f;
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
                bool b;
                if (p == null)
                {
                    b = false;
                }
                else
                {
                    var needs = p.needs;
                    b = needs?.joy != null;
                }

                if (b && p.needs.joy.CurLevel < p.needs.joy.MaxLevel)
                {
                    p.needs.joy.CurLevel = Math.Min(p.needs.joy.CurLevel * 1.02f, p.needs.joy.MaxLevel);
                }
            }

            if (oil.stackCount > 1)
            {
                oil.stackCount--;
                return;
            }

            oil.Destroy();
        }

        // Token: 0x06000005 RID: 5 RVA: 0x00002467 File Offset: 0x00000667
        public static float GetOilReduction(Thing oil)
        {
            return Mathf.Lerp(20f, 80f, Math.Max(0f, Math.Min(1f, oil.MarketValue / 12f)));
        }
    }
}