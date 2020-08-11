using System;
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
		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000026 RID: 38 RVA: 0x000029F5 File Offset: 0x00000BF5
		public CompFerment FermentComp
		{
			get
			{
				return base.GetComp<CompFerment>();
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000027 RID: 39 RVA: 0x000029FD File Offset: 0x00000BFD
		// (set) Token: 0x06000028 RID: 40 RVA: 0x00002A0A File Offset: 0x00000C0A
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

		// Token: 0x06000029 RID: 41 RVA: 0x00002A18 File Offset: 0x00000C18
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

		// Token: 0x0600002A RID: 42 RVA: 0x00002AAC File Offset: 0x00000CAC
		public override void Tick()
		{
			base.Tick();
			if (base.Spawned)
			{
				if (this.IsHashIntervalTick(120))
				{
					int ActualStockNum;
					this.FermentComp.stockLimit = Building_FermenterVat.StockLimitReached(this, this.FermentProduct, this.StockLimit, out ActualStockNum);
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

		// Token: 0x0600002B RID: 43 RVA: 0x00002B27 File Offset: 0x00000D27
		public override void TickRare()
		{
			base.TickRare();
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002B2F File Offset: 0x00000D2F
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
				if (this.BatchLimit < 50)
				{
					this.BatchLimit = 50;
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
					Building_FermenterVat.StockLimitReached(this, this.FermentProduct, this.StockLimit, out ActualStockNum);
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

		// Token: 0x0600002D RID: 45 RVA: 0x00002B40 File Offset: 0x00000D40
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
					if (ChoiceProdDef != null && !ChoiceProdDef.FermenterNeeded)
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

		// Token: 0x0600002E RID: 46 RVA: 0x00002D1C File Offset: 0x00000F1C
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

		// Token: 0x0600002F RID: 47 RVA: 0x00002E3C File Offset: 0x0000103C
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

		// Token: 0x06000030 RID: 48 RVA: 0x00002EBC File Offset: 0x000010BC
		public void FermenterSelectBatch()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<int> Choices = Building_FermenterVat.GetBatches();
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
			this.BatchLimit = aBatchvalue;
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00002FAC File Offset: 0x000011AC
		public void FermenterSelectLimit()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<int> Choices = Building_FermenterVat.GetMaxStock();
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
			this.StockLimit = aStockLim;
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00003140 File Offset: 0x00001340
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

		// Token: 0x06000037 RID: 55 RVA: 0x000031AB File Offset: 0x000013AB
		public void ToggleProducing(bool flag)
		{
			this.producing = !flag;
		}

		// Token: 0x06000038 RID: 56 RVA: 0x000031B8 File Offset: 0x000013B8
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

		// Token: 0x06000039 RID: 57 RVA: 0x00003240 File Offset: 0x00001440
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

		// Token: 0x0600003A RID: 58 RVA: 0x000032B1 File Offset: 0x000014B1
		public bool HaveAnyResource(Building b, ThingDef def)
		{
			return def != null && b.Map.listerThings.ThingsOfDef(def).Count > 0;
		}

		// Token: 0x04000008 RID: 8
		public string FermentProduct = "";

		// Token: 0x04000009 RID: 9
		public string FermentResource = "";

		// Token: 0x0400000A RID: 10
		public string SpoilProduct = "";

		// Token: 0x0400000B RID: 11
		public float FermentHours = 168f;

		// Token: 0x0400000C RID: 12
		public int StockLimit;

		// Token: 0x0400000D RID: 13
		public int BatchLimit = 250;

		// Token: 0x0400000E RID: 14
		public List<FermentDef> fermentList = DefDatabase<FermentDef>.AllDefsListForReading;
	}
}
