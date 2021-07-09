using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Fermenter
{
    // Token: 0x02000005 RID: 5
    public class Building_FermenterVat : Building
    {
        // Token: 0x0400000D RID: 13
        public int BatchLimit = 250;

        // Token: 0x0400000B RID: 11
        public float FermentHours = 168f;

        // Token: 0x0400000E RID: 14
        public List<FermentDef> fermentList = DefDatabase<FermentDef>.AllDefsListForReading;

        // Token: 0x04000008 RID: 8
        public string FermentProduct = "";

        // Token: 0x04000009 RID: 9
        public string FermentResource = "";

        // Token: 0x0400000A RID: 10
        public string SpoilProduct = "";

        // Token: 0x0400000C RID: 12
        public int StockLimit;

        // Token: 0x17000005 RID: 5
        // (get) Token: 0x06000026 RID: 38 RVA: 0x000029F5 File Offset: 0x00000BF5
        public CompFerment FermentComp => GetComp<CompFerment>();

        // Token: 0x17000006 RID: 6
        // (get) Token: 0x06000027 RID: 39 RVA: 0x000029FD File Offset: 0x00000BFD
        // (set) Token: 0x06000028 RID: 40 RVA: 0x00002A0A File Offset: 0x00000C0A
        public bool producing
        {
            get => FermentComp.producing;
            set => FermentComp.producing = value;
        }

        // Token: 0x06000029 RID: 41 RVA: 0x00002A18 File Offset: 0x00000C18
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref FermentProduct, "FermentProduct", "");
            Scribe_Values.Look(ref FermentResource, "FermentResource", "");
            Scribe_Values.Look(ref SpoilProduct, "SpoilProduct", "");
            Scribe_Values.Look(ref FermentHours, "FermentHours", 168f);
            Scribe_Values.Look(ref StockLimit, "StockLimit");
            Scribe_Values.Look(ref BatchLimit, "BatchLimit", 250);
        }

        // Token: 0x0600002A RID: 42 RVA: 0x00002AAC File Offset: 0x00000CAC
        public override void Tick()
        {
            base.Tick();
            if (!Spawned)
            {
                return;
            }

            if (this.IsHashIntervalTick(120))
            {
                FermentComp.stockLimit = StockLimitReached(this, FermentProduct, StockLimit, out _);
            }

            if (!FermentComp.Filling || !FermentComp.producing)
            {
                return;
            }

            var resDef = ThingDef.Named(FermentResource);
            if (!HaveAnyResource(this, resDef))
            {
                FermentUtility.ResNotFoundOverlay(this, resDef);
            }
        }

        // Token: 0x0600002B RID: 43 RVA: 0x00002B27 File Offset: 0x00000D27

        // Token: 0x0600002C RID: 44 RVA: 0x00002B2F File Offset: 0x00000D2F
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (Faction != Faction.OfPlayer)
            {
                yield break;
            }

            string SelectDesc = "Fermenter.SelectDesc".Translate();
            if (FermentProduct == "")
            {
                string NoProd = "Fermenter.Select".Translate();
                yield return new Command_Action
                {
                    defaultLabel = NoProd,
                    icon = ContentFinder<Texture2D>.Get(FermentUtility.ProdTexPath),
                    defaultDesc = SelectDesc,
                    action = FermenterSelectProd
                };
            }
            else
            {
                var IconToUse = FermentUtility.GetProductIcon(FermentProduct, out var FermentProductDef);
                var LabelDetail = FermentProductDef.label.CapitalizeFirst();
                var DonePercent = 0f;
                if (FermentHours > 0f && FermentComp.fermentTicks > 0)
                {
                    DonePercent = ((FermentHours * 2500f) - FermentComp.fermentTicks) / (FermentHours * 2500f);
                }

                LabelDetail = LabelDetail + " (" + DonePercent.ToStringPercent() + ")";
                yield return new Command_Action
                {
                    defaultLabel = LabelDetail,
                    icon = IconToUse,
                    defaultDesc = SelectDesc,
                    action = FermenterSelectProd
                };
            }

            var BatchLabelDetail = "";
            var BatchTexPath = FermentUtility.FrontBatchPath;
            if (BatchLimit < 50)
            {
                BatchLimit = 50;
            }

            if (BatchLimit > 0)
            {
                BatchLabelDetail = "Fermenter.BatchLabel".Translate(BatchLimit.ToString());
                BatchTexPath += BatchLimit.ToString();
            }

            BatchTexPath += FermentUtility.EndLimitPath;
            var BatchIconToUse = ContentFinder<Texture2D>.Get(BatchTexPath);
            string SelectBatch = "Fermenter.SelectBatchLimit".Translate();
            yield return new Command_Action
            {
                defaultLabel = BatchLabelDetail,
                icon = BatchIconToUse,
                defaultDesc = SelectBatch,
                action = FermenterSelectBatch
            };
            var LimitTexPath = FermentUtility.FrontLimitPath;
            string LimitLabelDetail;
            if (StockLimit > 0)
            {
                StockLimitReached(this, FermentProduct, StockLimit, out var ActualStockNum);
                var LimitPct = ActualStockNum * 100 / StockLimit;
                LimitLabelDetail = "Fermenter.StockLabel".Translate(StockLimit.ToString(), LimitPct.ToString());
                LimitTexPath += StockLimit.ToString();
            }
            else
            {
                LimitLabelDetail = "Fermenter.StockLabelNL".Translate();
                LimitTexPath += "No";
            }

            LimitTexPath += FermentUtility.EndLimitPath;
            var LimitIconToUse = ContentFinder<Texture2D>.Get(LimitTexPath);
            string SelectLimit = "Fermenter.SelectStockLimit".Translate();
            yield return new Command_Action
            {
                defaultLabel = LimitLabelDetail,
                icon = LimitIconToUse,
                defaultDesc = SelectLimit,
                action = FermenterSelectLimit
            };
            string LabelProduce = "Fermenter.Production".Translate() + " ";
            string LabelProduceDesc = "Fermenter.ProductionDesc".Translate();
            if (producing)
            {
                if (FermentProduct == "")
                {
                    LabelProduce += "Fermenter.ProdNoProd".Translate();
                }
                else
                {
                    LabelProduce += FermentUtility.GetStatusDesc(this);
                }
            }
            else
            {
                LabelProduce += "Fermenter.ProdStopped".Translate();
            }

            yield return new Command_Toggle
            {
                icon = ContentFinder<Texture2D>.Get(FermentUtility.produceTexPath),
                defaultLabel = LabelProduce,
                defaultDesc = LabelProduceDesc,
                isActive = () => producing,
                toggleAction = delegate { ToggleProducing(producing); }
            };
            string CancelLabelDetail = "Fermenter.Cancel".Translate();
            string SelectCancel = "Fermenter.SelCancel".Translate();
            yield return new Command_Action
            {
                defaultLabel = CancelLabelDetail,
                icon = ContentFinder<Texture2D>.Get(FermentUtility.CancelTexPath),
                defaultDesc = SelectCancel,
                action = FermenterCancelBatch
            };
        }

        // Token: 0x0600002D RID: 45 RVA: 0x00002B40 File Offset: 0x00000D40
        public void FermenterSelectProd()
        {
            var list = new List<FloatMenuOption>();
            string text = "Fermenter.SelNone".Translate();
            list.Add(new FloatMenuOption(text, delegate { SetProdValues(null, false); }, MenuOptionPriority.Default,
                null, null, 29f));
            var Choices = GetProdList();
            if (Choices.Count > 0)
            {
                foreach (var Choice in Choices)
                {
                    var ChoiceProdDef = DefDatabase<FermentDef>.GetNamed(Choice);
                    if (ChoiceProdDef == null || ChoiceProdDef.FermenterNeeded)
                    {
                        continue;
                    }

                    var ProdDef = ThingDef.Named(ChoiceProdDef.Product);
                    var ResDef = ThingDef.Named(ChoiceProdDef.Resource);
                    var key = "Fermenter.MakeProcess";
                    var array = new object[2];
                    var num = 0;
                    var prodDef = ProdDef;
                    array[num] = prodDef != null ? new TaggedString?(prodDef.LabelCap) : null;
                    array[1] = ResDef != null ? new TaggedString?(ResDef.LabelCap) : null;
                    text = key.Translate(array);
                    list.Add(new FloatMenuOption(text, delegate { SetProdValues(ChoiceProdDef, true); },
                        MenuOptionPriority.Default, null, null, 29f,
                        rect => Widgets.InfoCardButton(rect.x + 5f, rect.y + ((rect.height - 24f) / 2f), ProdDef)));
                }
            }

            var sortedlist = (from fmo in list
                orderby fmo.Label
                select fmo).ToList();
            Find.WindowStack.Add(new FloatMenu(sortedlist));
        }

        // Token: 0x0600002E RID: 46 RVA: 0x00002D1C File Offset: 0x00000F1C
        public void SetProdValues(FermentDef fdef, bool process)
        {
            string Msg = "Fermenter.WaitChange".Translate();
            if (!FermentComp.Fermented && !FermentComp.producing)
            {
                if (process)
                {
                    FermentProduct = fdef.Product;
                    FermentResource = fdef.Resource;
                    SpoilProduct = fdef.SpoilProduct;
                    FermentHours = fdef.FermentHours;
                    if (FermentHours < 24f)
                    {
                        FermentHours = 24f;
                    }
                }
                else
                {
                    producing = false;
                    FermentProduct = "";
                    FermentResource = "";
                    SpoilProduct = "";
                    FermentHours = 168f;
                }

                FermentComp.status = 7;
                return;
            }

            if (FermentComp.producing)
            {
                Msg += "Fermenter.IsProducingATM".Translate();
            }
            else if (FermentComp.Fermented)
            {
                Msg += "Fermenter.HasProductOrSpoilWait".Translate();
            }

            Messages.Message(Msg, MessageTypeDefOf.RejectInput, false);
        }

        // Token: 0x0600002F RID: 47 RVA: 0x00002E3C File Offset: 0x0000103C
        public List<string> GetProdList()
        {
            var list = new List<string>();
            foreach (var ferment in fermentList)
            {
                if (ThingDef.Named(ferment.Product) != null && FermentUtility.IsResearched(ferment.ResearchProject))
                {
                    list.AddDistinct(ferment.defName);
                }
            }

            return list;
        }

        // Token: 0x06000030 RID: 48 RVA: 0x00002EBC File Offset: 0x000010BC
        public void FermenterSelectBatch()
        {
            var list = new List<FloatMenuOption>();
            var Choices = GetBatches();
            if (Choices.Count > 0)
            {
                var text = "";
                foreach (var i in Choices)
                {
                    if (i > 0)
                    {
                        text = i.ToString();
                    }

                    var value = i;
                    list.Add(new FloatMenuOption(text, delegate { SetBatchLimits(value); }, MenuOptionPriority.Default,
                        null, null, 29f));
                }
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        // Token: 0x06000031 RID: 49 RVA: 0x00002F60 File Offset: 0x00001160
        public static List<int> GetBatches()
        {
            return new List<int>
            {
                50,
                75,
                100,
                150,
                200,
                250
            };
        }

        // Token: 0x06000032 RID: 50 RVA: 0x00002FA0 File Offset: 0x000011A0
        public void SetBatchLimits(int aBatchvalue)
        {
            BatchLimit = aBatchvalue;
        }

        // Token: 0x06000033 RID: 51 RVA: 0x00002FAC File Offset: 0x000011AC
        public void FermenterSelectLimit()
        {
            var list = new List<FloatMenuOption>();
            var Choices = GetMaxStock();
            if (Choices.Count > 0)
            {
                foreach (var i in Choices)
                {
                    string text;
                    if (i > 0)
                    {
                        text = i.ToString();
                    }
                    else
                    {
                        text = "Fermenter.StockNoLimit".Translate();
                    }

                    var value = i;
                    list.Add(new FloatMenuOption(text, delegate { SetStockLimits(value); }, MenuOptionPriority.Default,
                        null, null, 29f));
                }
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        // Token: 0x06000034 RID: 52 RVA: 0x0000305C File Offset: 0x0000125C
        public static List<int> GetMaxStock()
        {
            return new List<int>
            {
                50,
                100,
                150,
                200,
                250,
                300,
                400,
                500,
                750,
                1000,
                1500,
                2000,
                2500,
                3000,
                4000,
                5000,
                7500,
                10000,
                0
            };
        }

        // Token: 0x06000035 RID: 53 RVA: 0x00003135 File Offset: 0x00001335
        public void SetStockLimits(int aStockLim)
        {
            StockLimit = aStockLim;
        }

        // Token: 0x06000036 RID: 54 RVA: 0x00003140 File Offset: 0x00001340
        public static bool StockLimitReached(Building b, string stockThing, int stockLim, out int ActualStockNum)
        {
            ActualStockNum = 0;
            if (stockLim <= 0 || stockThing == "")
            {
                return false;
            }

            var StockListing = b.Map.listerThings.ThingsOfDef(ThingDef.Named(stockThing));
            if (StockListing.Count > 0)
            {
                foreach (var thing in StockListing)
                {
                    ActualStockNum += thing.stackCount;
                }
            }

            if (ActualStockNum >= stockLim)
            {
                return true;
            }

            return false;
        }

        // Token: 0x06000037 RID: 55 RVA: 0x000031AB File Offset: 0x000013AB
        public void ToggleProducing(bool flag)
        {
            producing = !flag;
        }

        // Token: 0x06000038 RID: 56 RVA: 0x000031B8 File Offset: 0x000013B8
        public void FermenterCancelBatch()
        {
            var list = new List<FloatMenuOption>();
            string text = "Fermenter.DoNothing".Translate();
            list.Add(new FloatMenuOption(text, delegate { SetCancelBatch(this, false); }, MenuOptionPriority.Default,
                null, null, 29f));
            text = "Fermenter.Cancel".Translate();
            list.Add(new FloatMenuOption(text, delegate { SetCancelBatch(this, true); }, MenuOptionPriority.Default,
                null, null, 29f));
            Find.WindowStack.Add(new FloatMenu(list));
        }

        // Token: 0x06000039 RID: 57 RVA: 0x00003240 File Offset: 0x00001440
        public void SetCancelBatch(Building b, bool cancel)
        {
            if (!cancel || FermentComp == null)
            {
                return;
            }

            if (FermentComp.Filling || FermentComp.Fermenting)
            {
                if (FermentComp.Filling)
                {
                    FermentComp.DoCancel();
                    return;
                }

                FermentComp.DoSpoil();
            }
            else
            {
                Messages.Message("Fermenter.InvalidCancelStatus".Translate(), MessageTypeDefOf.RejectInput, false);
            }
        }

        // Token: 0x0600003A RID: 58 RVA: 0x000032B1 File Offset: 0x000014B1
        public bool HaveAnyResource(Building b, ThingDef thingDef)
        {
            return thingDef != null && b.Map.listerThings.ThingsOfDef(thingDef).Count > 0;
        }
    }
}