using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Fermenter
{
    // Token: 0x02000004 RID: 4
    public class Building_Fermenter : Building
    {
        // Token: 0x04000006 RID: 6
        public int BatchLimit = 250;

        // Token: 0x04000004 RID: 4
        public float FermentHours = 168f;

        // Token: 0x04000007 RID: 7
        public List<FermentDef> fermentList = DefDatabase<FermentDef>.AllDefsListForReading;

        // Token: 0x04000001 RID: 1
        public string FermentProduct = "";

        // Token: 0x04000002 RID: 2
        public string FermentResource = "";

        // Token: 0x04000003 RID: 3
        public string SpoilProduct = "";

        // Token: 0x04000005 RID: 5
        public int StockLimit;

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x06000005 RID: 5 RVA: 0x00002082 File Offset: 0x00000282
        public CompFerment FermentComp => GetComp<CompFerment>();

        // Token: 0x17000004 RID: 4
        // (get) Token: 0x06000006 RID: 6 RVA: 0x0000208A File Offset: 0x0000028A
        // (set) Token: 0x06000007 RID: 7 RVA: 0x00002097 File Offset: 0x00000297
        public bool producing
        {
            get => FermentComp.producing;
            set => FermentComp.producing = value;
        }

        // Token: 0x06000008 RID: 8 RVA: 0x000020A8 File Offset: 0x000002A8
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

        // Token: 0x06000009 RID: 9 RVA: 0x0000213C File Offset: 0x0000033C
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

        // Token: 0x0600000A RID: 10 RVA: 0x000021B7 File Offset: 0x000003B7

        // Token: 0x0600000B RID: 11 RVA: 0x000021BF File Offset: 0x000003BF
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
            if (BatchLimit < 150)
            {
                BatchLimit = 150;
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

        // Token: 0x0600000C RID: 12 RVA: 0x000021D0 File Offset: 0x000003D0
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
                    if (ChoiceProdDef == null)
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

        // Token: 0x0600000D RID: 13 RVA: 0x0000239C File Offset: 0x0000059C
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

        // Token: 0x0600000E RID: 14 RVA: 0x000024BC File Offset: 0x000006BC
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

        // Token: 0x0600000F RID: 15 RVA: 0x0000253C File Offset: 0x0000073C
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

        // Token: 0x06000010 RID: 16 RVA: 0x000025E0 File Offset: 0x000007E0
        public static List<int> GetBatches()
        {
            return new List<int>
            {
                150,
                200,
                250
            };
        }

        // Token: 0x06000011 RID: 17 RVA: 0x00002608 File Offset: 0x00000808
        public void SetBatchLimits(int aBatchvalue)
        {
            BatchLimit = aBatchvalue;
        }

        // Token: 0x06000012 RID: 18 RVA: 0x00002614 File Offset: 0x00000814
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

        // Token: 0x06000013 RID: 19 RVA: 0x000026C4 File Offset: 0x000008C4
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

        // Token: 0x06000014 RID: 20 RVA: 0x0000279D File Offset: 0x0000099D
        public void SetStockLimits(int aStockLim)
        {
            StockLimit = aStockLim;
        }

        // Token: 0x06000015 RID: 21 RVA: 0x000027A8 File Offset: 0x000009A8
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

        // Token: 0x06000016 RID: 22 RVA: 0x00002813 File Offset: 0x00000A13
        public void ToggleProducing(bool flag)
        {
            producing = !flag;
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00002820 File Offset: 0x00000A20
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

        // Token: 0x06000018 RID: 24 RVA: 0x000028A8 File Offset: 0x00000AA8
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

        // Token: 0x06000019 RID: 25 RVA: 0x00002919 File Offset: 0x00000B19
        public bool HaveAnyResource(Building b, ThingDef thingDef)
        {
            return thingDef != null && b.Map.listerThings.ThingsOfDef(thingDef).Count > 0;
        }
    }
}