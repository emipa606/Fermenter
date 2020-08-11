using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Fermenter
{
	// Token: 0x02000007 RID: 7
	public class CompFerment : ThingComp
	{
		// Token: 0x0600004F RID: 79 RVA: 0x0000367C File Offset: 0x0000187C
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref this.spaceLeft, "spaceLeft", 240, false);
			Scribe_Values.Look<int>(ref this.ingredients, "ingredients", 0, false);
			Scribe_Values.Look<int>(ref this.products, "products", 0, false);
			Scribe_Values.Look<int>(ref this.status, "status", 0, false);
			Scribe_Values.Look<bool>(ref this.producing, "producing", false, false);
			Scribe_Values.Look<int>(ref this.prevStatus, "prevStatus", 0, false);
			Scribe_Values.Look<int>(ref this.fermentTicks, "fermentTicks", 0, false);
			Scribe_Values.Look<int>(ref this.spoilTicks, "spoilTicks", 0, false);
			Scribe_Values.Look<int>(ref this.resettingTicks, "resettingTicks", 0, false);
			Scribe_Values.Look<int>(ref this.cleanTicks, "cleanTicks", 0, false);
			Scribe_Values.Look<bool>(ref this.stockLimit, "stockLimit", false, false);
			Scribe_Values.Look<int>(ref this.batchsize, "batchsize", 250, false);
			Scribe_Values.Look<bool>(ref this.stopDueToStock, "stopDueToStock", false, false);
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000050 RID: 80 RVA: 0x00003781 File Offset: 0x00001981
		public CompProperties_Ferment Props
		{
			get
			{
				return (CompProperties_Ferment)this.props;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000051 RID: 81 RVA: 0x0000378E File Offset: 0x0000198E
		public Thing Fermenter
		{
			get
			{
				return this.parent;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000052 RID: 82 RVA: 0x00003796 File Offset: 0x00001996
		public bool Idling
		{
			get
			{
				return this.status == 0;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000053 RID: 83 RVA: 0x000037A1 File Offset: 0x000019A1
		public bool Fermenting
		{
			get
			{
				return this.status == 2;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000054 RID: 84 RVA: 0x000037AC File Offset: 0x000019AC
		public bool Fermented
		{
			get
			{
				return this.products > 0;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000055 RID: 85 RVA: 0x000037B7 File Offset: 0x000019B7
		public bool Filling
		{
			get
			{
				return this.status == 1;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000056 RID: 86 RVA: 0x000037C2 File Offset: 0x000019C2
		public bool Removing
		{
			get
			{
				return this.status == 3;
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000057 RID: 87 RVA: 0x000037CD File Offset: 0x000019CD
		public bool Cleaning
		{
			get
			{
				return this.status == 4;
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000058 RID: 88 RVA: 0x000037D8 File Offset: 0x000019D8
		public bool Resetting
		{
			get
			{
				return this.status == 7;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000059 RID: 89 RVA: 0x000037E3 File Offset: 0x000019E3
		public bool Spoiled
		{
			get
			{
				return this.status == 6;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600005A RID: 90 RVA: 0x000037EE File Offset: 0x000019EE
		public bool Stopped
		{
			get
			{
				return this.status == 5;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600005B RID: 91 RVA: 0x000037F9 File Offset: 0x000019F9
		public bool Cancelling
		{
			get
			{
				return this.status == 8;
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600005C RID: 92 RVA: 0x00003804 File Offset: 0x00001A04
		public ThingDef spoilProductDef
		{
			get
			{
				string defname;
				if (this.Fermenter is Building_Fermenter)
				{
					defname = (this.Fermenter as Building_Fermenter).SpoilProduct;
				}
				else
				{
					defname = (this.Fermenter as Building_FermenterVat).SpoilProduct;
				}
				return DefDatabase<ThingDef>.GetNamed(defname, false);
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600005D RID: 93 RVA: 0x00003850 File Offset: 0x00001A50
		public ThingDef outputProductDef
		{
			get
			{
				string defname;
				if (this.Fermenter is Building_Fermenter)
				{
					defname = (this.Fermenter as Building_Fermenter).FermentProduct;
				}
				else
				{
					defname = (this.Fermenter as Building_FermenterVat).FermentProduct;
				}
				return DefDatabase<ThingDef>.GetNamed(defname, false);
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x0600005E RID: 94 RVA: 0x0000389C File Offset: 0x00001A9C
		public ThingDef inputResourceDef
		{
			get
			{
				string defname;
				if (this.Fermenter is Building_Fermenter)
				{
					defname = (this.Fermenter as Building_Fermenter).FermentResource;
				}
				else
				{
					defname = (this.Fermenter as Building_FermenterVat).FermentResource;
				}
				return DefDatabase<ThingDef>.GetNamed(defname, false);
			}
		}

		// Token: 0x0600005F RID: 95 RVA: 0x000038E8 File Offset: 0x00001AE8
		public override string CompInspectStringExtra()
		{
			int temp = (int)this.Fermenter.AmbientTemperature;
			Color white = Color.white;
			string temptype = "Fermenter.tempfine".Translate();
			if (temp < -1 || temp > 32)
			{
				Color red = Color.red;
				temptype = "Fermenter.tempbad".Translate();
			}
			else if (temp < 7)
			{
				Color yellow = Color.yellow;
				temptype = "Fermenter.temppoor".Translate();
			}
			string warning;
			if (this.Fermenter is Building_Fermenter)
			{
				if (this.isFunctional(this.Fermenter))
				{
					warning = "Fermenter.tempregulated".Translate();
				}
				else
				{
					warning = "Fermenter.tempwarning".Translate(temptype);
				}
			}
			else
			{
				warning = "Fermenter.tempwarning".Translate(temptype);
			}
			string tempscale = "Fermenter.TmpCelsius".Translate();
			if (Prefs.TemperatureMode != TemperatureDisplayMode.Celsius)
			{
				if (Prefs.TemperatureMode == TemperatureDisplayMode.Fahrenheit)
				{
					tempscale = "Fermenter.TmpFahrenheit".Translate();
					temp = (int)GenTemperature.CelsiusTo((float)temp, TemperatureDisplayMode.Fahrenheit);
				}
				else
				{
					tempscale = "Fermenter.TmpKelvin".Translate();
					temp = (int)GenTemperature.CelsiusTo((float)temp, TemperatureDisplayMode.Kelvin);
				}
			}
			return "Fermenter.tempmsg".Translate(temp.ToString(), warning, tempscale);
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00003A30 File Offset: 0x00001C30
		public override void CompTick()
		{
			base.CompTick();
			if (this.Fermenter.Spawned && this.Fermenter.IsHashIntervalTick(120))
			{
				if (this.isFunctional(this.Fermenter))
				{
					if (this.stopDueToStock && !this.stockLimit)
					{
						this.stopDueToStock = false;
						this.producing = true;
					}
					if (this.producing)
					{
						if (this.Idling)
						{
							if (this.outputProductDef != null && this.inputResourceDef != null && !this.stockLimit)
							{
								this.ChangeStatus(1);
								return;
							}
						}
						else if (this.Filling)
						{
							if (this.stockLimit)
							{
								this.stopDueToStock = true;
								this.DoCancel();
								return;
							}
							if (this.CheckIsFull())
							{
								this.ChangeStatus(2);
								if (this.Fermenter is Building_Fermenter)
								{
									this.fermentTicks = (int)((this.Fermenter as Building_Fermenter).FermentHours * 2500f);
									return;
								}
								this.fermentTicks = (int)((this.Fermenter as Building_FermenterVat).FermentHours * 2500f);
								return;
							}
						}
						else
						{
							if (this.Fermenting)
							{
								this.DoFermentation();
								return;
							}
							if (this.Removing)
							{
								if (!this.Fermented)
								{
									if (this.Fermenter is Building_Fermenter)
									{
										this.ChangeStatus(4);
										this.cleanTicks = (int)(this.GetCleaning() * 2500f);
										return;
									}
									this.DoReset();
									return;
								}
							}
							else if (this.Cleaning)
							{
								if (this.cleanTicks > 0)
								{
									this.cleanTicks -= 120;
									return;
								}
								this.DoReset();
								return;
							}
							else if (this.Spoiled)
							{
								if (!this.Fermented)
								{
									if (this.Fermenter is Building_Fermenter)
									{
										this.ChangeStatus(4);
										this.cleanTicks = (int)(this.GetCleaning() * 2500f);
									}
									else
									{
										this.DoReset();
									}
									this.producing = false;
									return;
								}
							}
							else if (this.Resetting)
							{
								if (this.resettingTicks > 0)
								{
									this.resettingTicks -= 120;
									return;
								}
								this.ChangeStatus(0);
								this.resettingTicks = 0;
								if (this.Fermenter is Building_Fermenter)
								{
									this.batchsize = (this.Fermenter as Building_Fermenter).BatchLimit;
								}
								else
								{
									this.batchsize = (this.Fermenter as Building_FermenterVat).BatchLimit;
								}
								this.spaceLeft = this.batchsize;
								this.products = 0;
								this.ingredients = 0;
								this.fermentTicks = 0;
								this.cleanTicks = 0;
								this.spoilTicks = 0;
								return;
							}
							else if (this.Cancelling && this.ingredients <= 0)
							{
								this.ingredients = 0;
								this.spaceLeft = this.batchsize;
								this.ChangeStatus(7);
								this.producing = false;
								return;
							}
						}
					}
					else if (this.Fermenting)
					{
						this.DoFermentation();
						return;
					}
				}
				else if (this.Fermenting)
				{
					float temp = this.Fermenter.AmbientTemperature;
					if (temp < -1f)
					{
						this.spoilTicks += 120;
					}
					else if (temp < 7f)
					{
						this.spoilTicks += 60;
					}
					else if (temp > 32f)
					{
						this.spoilTicks += 120;
					}
					int totSpoil = this.GetSpoiling();
					if (totSpoil > 0 && this.spoilTicks >= totSpoil)
					{
						this.DoSpoil();
					}
				}
			}
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00003D68 File Offset: 0x00001F68
		public override void CompTickRare()
		{
			base.CompTickRare();
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00003D70 File Offset: 0x00001F70
		public void DoFermentation()
		{
			if (this.fermentTicks > 0)
			{
				this.fermentTicks -= 120;
				return;
			}
			this.fermentTicks = 0;
			this.products = this.batchsize - this.spaceLeft;
			this.ingredients = 0;
			if (this.products > 0)
			{
				this.ChangeStatus(3);
				return;
			}
			if (this.Fermenter is Building_Fermenter)
			{
				this.ChangeStatus(4);
				this.cleanTicks = (int)(this.GetCleaning() * 2500f);
				return;
			}
			this.DoReset();
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00003DF6 File Offset: 0x00001FF6
		public void DoReset()
		{
			this.cleanTicks = 0;
			this.ChangeStatus(7);
			this.resettingTicks = 1250;
			this.ingredients = 0;
			this.products = 0;
			this.spaceLeft = this.batchsize;
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00003E2C File Offset: 0x0000202C
		public void DoSpoil()
		{
			this.fermentTicks = 0;
			this.ingredients = 0;
			if (this.spoilProductDef != null)
			{
				this.products = this.batchsize - this.spaceLeft;
				this.ChangeStatus(6);
			}
			else
			{
				this.products = 0;
				this.spaceLeft = this.batchsize;
				if (this.Fermenter is Building_Fermenter)
				{
					this.ChangeStatus(4);
					this.cleanTicks = (int)(this.GetCleaning() * 2500f);
				}
				else
				{
					this.DoReset();
				}
			}
			this.spoilTicks = 0;
			Messages.Message("Fermenter.SpoiledMsg".Translate(), this.Fermenter, MessageTypeDefOf.NeutralEvent, true);
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00003ED9 File Offset: 0x000020D9
		public void DoCancel()
		{
			this.ChangeStatus(8);
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00003EE2 File Offset: 0x000020E2
		public void ChangeStatus(int newStatus)
		{
			this.prevStatus = this.status;
			this.status = newStatus;
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00003EF8 File Offset: 0x000020F8
		public float GetCleaning()
		{
			float cleaning = this.Props.cleanHours;
			if (cleaning < 1f)
			{
				cleaning = 1f;
			}
			return Math.Min(cleaning, 5f);
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00003F2C File Offset: 0x0000212C
		public int GetSpoiling()
		{
			int totSpoil = this.Props.totalSpoilTicks;
			if (totSpoil < 2500)
			{
				totSpoil = 2500;
			}
			return Math.Min(totSpoil, 180000);
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00003F5E File Offset: 0x0000215E
		public bool CheckIsFull()
		{
			return this.spaceLeft <= 0;
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00003F6C File Offset: 0x0000216C
		public bool isFunctional(Thing fermenter)
		{
			if (fermenter != null)
			{
				if (fermenter is Building_Fermenter)
				{
					if (fermenter.IsBrokenDown())
					{
						return false;
					}
					CompPowerTrader compPow = fermenter.TryGetComp<CompPowerTrader>();
					if (compPow == null)
					{
						return false;
					}
					if (!compPow.PowerOn)
					{
						return false;
					}
				}
				else
				{
					float temp = (fermenter as Building).AmbientTemperature;
					if (temp < 7f || temp > 32f)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x0400000F RID: 15
		public int spaceLeft = 240;

		// Token: 0x04000010 RID: 16
		public int ingredients;

		// Token: 0x04000011 RID: 17
		public int products;

		// Token: 0x04000012 RID: 18
		public int status;

		// Token: 0x04000013 RID: 19
		public int prevStatus;

		// Token: 0x04000014 RID: 20
		public int fermentTicks;

		// Token: 0x04000015 RID: 21
		public int spoilTicks;

		// Token: 0x04000016 RID: 22
		public int cleanTicks;

		// Token: 0x04000017 RID: 23
		public int resettingTicks;

		// Token: 0x04000018 RID: 24
		public bool producing;

		// Token: 0x04000019 RID: 25
		public bool stockLimit;

		// Token: 0x0400001A RID: 26
		public int batchsize = 250;

		// Token: 0x0400001B RID: 27
		public bool stopDueToStock;
	}
}
