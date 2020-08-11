using System;
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
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000005 RID: 5 RVA: 0x00002082 File Offset: 0x00000282
		public CompFerment FermentComp
		{
			get
			{
				return base.GetComp<CompFerment>();
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000006 RID: 6 RVA: 0x0000208A File Offset: 0x0000028A
		// (set) Token: 0x06000007 RID: 7 RVA: 0x00002097 File Offset: 0x00000297
		public bool producing
		{
			get
			{
				return this.FermentComp.producing;
			}
			set
			{
				this.FermentComp.producing = value;
			}
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000020A8 File Offset: 0x000002A8
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<string>(ref this.FermentProduct, "FermentProduct", "", false);
			Scribe_Values.Look<string>(ref this.FermentResource, "FermentResource", "", false);
			Scribe_Values.Look<string>(ref this.SpoilProduct, "SpoilProduct", "", false);
			Scribe_Values.Look<float>(ref this.FermentHours, "FermentHours", 168f, false);
			Scribe_Values.Look<int>(ref this.StockLimit, "StockLimit", 0, false);
			Scribe_Values.Look<int>(ref this.BatchLimit, "BatchLimit", 250, false);
		}

		// Token: 0x06000009 RID: 9 RVA: 0x0000213C File Offset: 0x0000033C
		public override void Tick()
		{
			base.Tick();
			if (base.Spawned)
			{
				if (this.IsHashIntervalTick(120))
				{
					int ActualStockNum;
					this.FermentComp.stockLimit = Building_Fermenter.StockLimitReached(this, this.FermentProduct, this.StockLimit, out ActualStockNum);
				}
				if (this.FermentComp.Filling && this.FermentComp.producing)
				{
					ThingDef resDef = ThingDef.Named(this.FermentResource);
					if (!this.HaveAnyResource(this, resDef))
					{
						FermentUtility.ResNotFoundOverlay(this, resDef);
					}
				}
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000021B7 File Offset: 0x000003B7
		public override void TickRare()
		{
			base.TickRare();
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000021BF File Offset: 0x000003BF
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			IEnumerator<Gizmo> enumerator = null;
			if (base.Faction == Faction.OfPlayer)
			{
				string SelectDesc = "Fermenter.SelectDesc".Translate();
				if (this.FermentProduct == "")
				{
					string NoProd = "Fermenter.Select".Translate();
					yield return new Command_Action
					{
						defaultLabel = NoProd,
						icon = ContentFinder<Texture2D>.Get(FermentUtility.ProdTexPath, true),
						defaultDesc = SelectDesc,
						action = delegate()
						{
							this.FermenterSelectProd();
						}
					};
				}
				else
				{
					ThingDef FermentProductDef;
					Texture2D IconToUse = FermentUtility.GetProductIcon(this.FermentProduct, out FermentProductDef);
					string LabelDetail = FermentProductDef.label.CapitalizeFirst();
					float DonePercent = 0f;
					if (this.FermentHours > 0f && this.FermentComp.fermentTicks > 0)
					{
						DonePercent = (this.FermentHours * 2500f - (float)this.FermentComp.fermentTicks) / (this.FermentHours * 2500f);
					}
					LabelDetail = LabelDetail + " (" + DonePercent.ToStringPercent() + ")";
					yield return new Command_Action
					{
						defaultLabel = LabelDetail,
						icon = IconToUse,
						defaultDesc = SelectDesc,
						action = delegate()
						{
							this.FermenterSelectProd();
						}
					};
				}
				string BatchLabelDetail = "";
				string BatchTexPath = FermentUtility.FrontBatchPath;
				if (this.BatchLimit < 150)
				{
					this.BatchLimit = 150;
				}
				if (this.BatchLimit > 0)
				{
					BatchLabelDetail = "Fermenter.BatchLabel".Translate(this.BatchLimit.ToString());
					BatchTexPath += this.BatchLimit.ToString();
				}
				BatchTexPath += FermentUtility.EndLimitPath;
				Texture2D BatchIconToUse = ContentFinder<Texture2D>.Get(BatchTexPath, true);
				string SelectBatch = "Fermenter.SelectBatchLimit".Translate();
				yield return new Command_Action
				{
					defaultLabel = BatchLabelDetail,
					icon = BatchIconToUse,
					defaultDesc = SelectBatch,
					action = delegate()
					{
						this.FermenterSelectBatch();
					}
				};
				string LimitTexPath = FermentUtility.FrontLimitPath;
				string LimitLabelDetail;
				if (this.StockLimit > 0)
				{
					int ActualStockNum;
					Building_Fermenter.StockLimitReached(this, this.FermentProduct, this.StockLimit, out ActualStockNum);
					int LimitPct = ActualStockNum * 100 / this.StockLimit;
					LimitLabelDetail = "Fermenter.StockLabel".Translate(this.StockLimit.ToString(), LimitPct.ToString());
					LimitTexPath += this.StockLimit.ToString();
				}
				else
				{
					LimitLabelDetail = "Fermenter.StockLabelNL".Translate();
					LimitTexPath += "No";
				}
				LimitTexPath += FermentUtility.EndLimitPath;
				Texture2D LimitIconToUse = ContentFinder<Texture2D>.Get(LimitTexPath, true);
				string SelectLimit = "Fermenter.SelectStockLimit".Translate();
				yield return new Command_Action
				{
					defaultLabel = LimitLabelDetail,
					icon = LimitIconToUse,
					defaultDesc = SelectLimit,
					action = delegate()
					{
						this.FermenterSelectLimit();
					}
				};
				string LabelProduce = "Fermenter.Production".Translate() + " ";
				string LabelProduceDesc = "Fermenter.ProductionDesc".Translate();
				if (this.producing)
				{
					if (this.FermentProduct == "")
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
					icon = ContentFinder<Texture2D>.Get(FermentUtility.produceTexPath, true),
					defaultLabel = LabelProduce,
					defaultDesc = LabelProduceDesc,
					isActive = (() => this.producing),
					toggleAction = delegate()
					{
						this.ToggleProducing(this.producing);
					}
				};
				string CancelLabelDetail = "Fermenter.Cancel".Translate();
				string SelectCancel = "Fermenter.SelCancel".Translate();
				yield return new Command_Action
				{
					defaultLabel = CancelLabelDetail,
					icon = ContentFinder<Texture2D>.Get(FermentUtility.CancelTexPath, true),
					defaultDesc = SelectCancel,
					action = delegate()
					{
						this.FermenterCancelBatch();
					}
				};
			}
			yield break;
			yield break;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000021D0 File Offset: 0x000003D0
		public void FermenterSelectProd()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			string text = "Fermenter.SelNone".Translate();
			list.Add(new FloatMenuOption(text, delegate()
			{
				this.SetProdValues(null, false);
			}, MenuOptionPriority.Default, null, null, 29f, null, null));
			List<string> Choices = this.GetProdList();
			if (Choices.Count > 0)
			{
				foreach (string Choice in Choices)
				{
					FermentDef ChoiceProdDef = DefDatabase<FermentDef>.GetNamed(Choice, true);
					if (ChoiceProdDef != null)
					{
						ThingDef ProdDef = ThingDef.Named(ChoiceProdDef.Product);
						ThingDef ResDef = ThingDef.Named(ChoiceProdDef.Resource);
						string key = "Fermenter.MakeProcess";
						object[] array = new object[2];
						int num = 0;
						ThingDef prodDef = ProdDef;
						array[num] = ((prodDef != null) ? new TaggedString?(prodDef.LabelCap) : null);
						array[1] = ((ResDef != null) ? new TaggedString?(ResDef.LabelCap) : null);
						text = key.Translate(array);
						list.Add(new FloatMenuOption(text, delegate()
						{
							this.SetProdValues(ChoiceProdDef, true);
						}, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, ProdDef), null));
					}
				}
			}
			List<FloatMenuOption> sortedlist = (from fmo in list
			orderby fmo.Label
			select fmo).ToList<FloatMenuOption>();
			Find.WindowStack.Add(new FloatMenu(sortedlist));
		}

		// Token: 0x0600000D RID: 13 RVA: 0x0000239C File Offset: 0x0000059C
		public void SetProdValues(FermentDef fdef, bool process)
		{
			string Msg = "Fermenter.WaitChange".Translate();
			if (!this.FermentComp.Fermented && !this.FermentComp.producing)
			{
				if (process)
				{
					this.FermentProduct = fdef.Product;
					this.FermentResource = fdef.Resource;
					this.SpoilProduct = fdef.SpoilProduct;
					this.FermentHours = fdef.FermentHours;
					if (this.FermentHours < 24f)
					{
						this.FermentHours = 24f;
					}
				}
				else
				{
					this.producing = false;
					this.FermentProduct = "";
					this.FermentResource = "";
					this.SpoilProduct = "";
					this.FermentHours = 168f;
				}
				this.FermentComp.status = 7;
				return;
			}
			if (this.FermentComp.producing)
			{
				Msg += "Fermenter.IsProducingATM".Translate();
			}
			else if (this.FermentComp.Fermented)
			{
				Msg += "Fermenter.HasProductOrSpoilWait".Translate();
			}
			Messages.Message(Msg, MessageTypeDefOf.RejectInput, false);
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000024BC File Offset: 0x000006BC
		public List<string> GetProdList()
		{
			List<RecipeDef> allDefsListForReading = DefDatabase<RecipeDef>.AllDefsListForReading;
			List<string> list = new List<string>();
			foreach (FermentDef ferment in this.fermentList)
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
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<int> Choices = Building_Fermenter.GetBatches();
			if (Choices.Count > 0)
			{
				string text = "";
				for (int i = 0; i < Choices.Count; i++)
				{
					if (Choices[i] > 0)
					{
						text = Choices[i].ToString();
					}
					int value = Choices[i];
					list.Add(new FloatMenuOption(text, delegate()
					{
						this.SetBatchLimits(value);
					}, MenuOptionPriority.Default, null, null, 29f, null, null));
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
			this.BatchLimit = aBatchvalue;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002614 File Offset: 0x00000814
		public void FermenterSelectLimit()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<int> Choices = Building_Fermenter.GetMaxStock();
			if (Choices.Count > 0)
			{
				for (int i = 0; i < Choices.Count; i++)
				{
					string text;
					if (Choices[i] > 0)
					{
						text = Choices[i].ToString();
					}
					else
					{
						text = "Fermenter.StockNoLimit".Translate();
					}
					int value = Choices[i];
					list.Add(new FloatMenuOption(text, delegate()
					{
						this.SetStockLimits(value);
					}, MenuOptionPriority.Default, null, null, 29f, null, null));
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
			this.StockLimit = aStockLim;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x000027A8 File Offset: 0x000009A8
		public static bool StockLimitReached(Building b, string stockThing, int stockLim, out int ActualStockNum)
		{
			ActualStockNum = 0;
			if (stockLim > 0 && stockThing != "")
			{
				List<Thing> StockListing = b.Map.listerThings.ThingsOfDef(ThingDef.Named(stockThing));
				if (StockListing.Count > 0)
				{
					for (int i = 0; i < StockListing.Count; i++)
					{
						ActualStockNum += StockListing[i].stackCount;
					}
				}
				if (ActualStockNum >= stockLim)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002813 File Offset: 0x00000A13
		public void ToggleProducing(bool flag)
		{
			this.producing = !flag;
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00002820 File Offset: 0x00000A20
		public void FermenterCancelBatch()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			string text = "Fermenter.DoNothing".Translate();
			list.Add(new FloatMenuOption(text, delegate()
			{
				this.SetCancelBatch(this, false);
			}, MenuOptionPriority.Default, null, null, 29f, null, null));
			text = "Fermenter.Cancel".Translate();
			list.Add(new FloatMenuOption(text, delegate()
			{
				this.SetCancelBatch(this, true);
			}, MenuOptionPriority.Default, null, null, 29f, null, null));
			Find.WindowStack.Add(new FloatMenu(list));
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000028A8 File Offset: 0x00000AA8
		public void SetCancelBatch(Building b, bool cancel)
		{
			if (cancel && this.FermentComp != null)
			{
				if (this.FermentComp.Filling || this.FermentComp.Fermenting)
				{
					if (this.FermentComp.Filling)
					{
						this.FermentComp.DoCancel();
						return;
					}
					this.FermentComp.DoSpoil();
					return;
				}
				else
				{
					Messages.Message("Fermenter.InvalidCancelStatus".Translate(), MessageTypeDefOf.RejectInput, false);
				}
			}
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002919 File Offset: 0x00000B19
		public bool HaveAnyResource(Building b, ThingDef def)
		{
			return def != null && b.Map.listerThings.ThingsOfDef(def).Count > 0;
		}

		// Token: 0x04000001 RID: 1
		public string FermentProduct = "";

		// Token: 0x04000002 RID: 2
		public string FermentResource = "";

		// Token: 0x04000003 RID: 3
		public string SpoilProduct = "";

		// Token: 0x04000004 RID: 4
		public float FermentHours = 168f;

		// Token: 0x04000005 RID: 5
		public int StockLimit;

		// Token: 0x04000006 RID: 6
		public int BatchLimit = 250;

		// Token: 0x04000007 RID: 7
		public List<FermentDef> fermentList = DefDatabase<FermentDef>.AllDefsListForReading;
	}
}
