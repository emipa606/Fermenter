using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Fermenter;

public class Building_FermenterVat : Building
{
    public readonly List<FermentDef> fermentList = DefDatabase<FermentDef>.AllDefsListForReading;
    public int BatchLimit = 250;

    public float FermentHours = 168f;

    public string FermentProduct = "";

    public string FermentResource = "";

    public string SpoilProduct = "";

    public int StockLimit;

    public CompFerment FermentComp => GetComp<CompFerment>();

    public bool producing
    {
        get => FermentComp.producing;
        set => FermentComp.producing = value;
    }

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

            LabelDetail = $"{LabelDetail} ({DonePercent.ToStringPercent()})";
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
                text = key.Translate(ProdDef?.LabelCap.RawText, ResDef?.LabelCap.RawText);
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

    public static List<int> GetBatches()
    {
        return
        [
            50,
            75,
            100,
            150,
            200,
            250
        ];
    }

    public void SetBatchLimits(int aBatchvalue)
    {
        BatchLimit = aBatchvalue;
    }

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

    public static List<int> GetMaxStock()
    {
        return
        [
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
        ];
    }

    public void SetStockLimits(int aStockLim)
    {
        StockLimit = aStockLim;
    }

    public static bool StockLimitReached(Building b, string stockThing, int stockLim, out int ActualStockNum)
    {
        ActualStockNum = 0;
        if (stockLim <= 0 || stockThing == "")
        {
            return false;
        }

        var StockListing = b.Map.listerThings.ThingsOfDef(ThingDef.Named(stockThing));
        if (StockListing.Count <= 0)
        {
            return ActualStockNum >= stockLim;
        }

        foreach (var thing in StockListing)
        {
            ActualStockNum += thing.stackCount;
        }

        return ActualStockNum >= stockLim;
    }

    public void ToggleProducing(bool flag)
    {
        producing = !flag;
    }

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

    public bool HaveAnyResource(Building b, ThingDef thingDef)
    {
        return thingDef != null && b.Map.listerThings.ThingsOfDef(thingDef).Count > 0;
    }
}