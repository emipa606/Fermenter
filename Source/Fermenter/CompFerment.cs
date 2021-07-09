using System;
using RimWorld;
using Verse;

namespace Fermenter
{
    // Token: 0x02000007 RID: 7
    public class CompFerment : ThingComp
    {
        // Token: 0x0400001A RID: 26
        public int batchsize = 250;

        // Token: 0x04000016 RID: 22
        public int cleanTicks;

        // Token: 0x04000014 RID: 20
        public int fermentTicks;

        // Token: 0x04000010 RID: 16
        public int ingredients;

        // Token: 0x04000013 RID: 19
        public int prevStatus;

        // Token: 0x04000018 RID: 24
        public bool producing;

        // Token: 0x04000011 RID: 17
        public int products;

        // Token: 0x04000017 RID: 23
        public int resettingTicks;

        // Token: 0x0400000F RID: 15
        public int spaceLeft = 240;

        // Token: 0x04000015 RID: 21
        public int spoilTicks;

        // Token: 0x04000012 RID: 18
        public int status;

        // Token: 0x04000019 RID: 25
        public bool stockLimit;

        // Token: 0x0400001B RID: 27
        public bool stopDueToStock;

        // Token: 0x17000007 RID: 7
        // (get) Token: 0x06000050 RID: 80 RVA: 0x00003781 File Offset: 0x00001981
        public CompProperties_Ferment Props => (CompProperties_Ferment) props;

        // Token: 0x17000008 RID: 8
        // (get) Token: 0x06000051 RID: 81 RVA: 0x0000378E File Offset: 0x0000198E
        public Thing Fermenter => parent;

        // Token: 0x17000009 RID: 9
        // (get) Token: 0x06000052 RID: 82 RVA: 0x00003796 File Offset: 0x00001996
        public bool Idling => status == 0;

        // Token: 0x1700000A RID: 10
        // (get) Token: 0x06000053 RID: 83 RVA: 0x000037A1 File Offset: 0x000019A1
        public bool Fermenting => status == 2;

        // Token: 0x1700000B RID: 11
        // (get) Token: 0x06000054 RID: 84 RVA: 0x000037AC File Offset: 0x000019AC
        public bool Fermented => products > 0;

        // Token: 0x1700000C RID: 12
        // (get) Token: 0x06000055 RID: 85 RVA: 0x000037B7 File Offset: 0x000019B7
        public bool Filling => status == 1;

        // Token: 0x1700000D RID: 13
        // (get) Token: 0x06000056 RID: 86 RVA: 0x000037C2 File Offset: 0x000019C2
        public bool Removing => status == 3;

        // Token: 0x1700000E RID: 14
        // (get) Token: 0x06000057 RID: 87 RVA: 0x000037CD File Offset: 0x000019CD
        public bool Cleaning => status == 4;

        // Token: 0x1700000F RID: 15
        // (get) Token: 0x06000058 RID: 88 RVA: 0x000037D8 File Offset: 0x000019D8
        public bool Resetting => status == 7;

        // Token: 0x17000010 RID: 16
        // (get) Token: 0x06000059 RID: 89 RVA: 0x000037E3 File Offset: 0x000019E3
        public bool Spoiled => status == 6;

        // Token: 0x17000011 RID: 17
        // (get) Token: 0x0600005A RID: 90 RVA: 0x000037EE File Offset: 0x000019EE
        public bool Stopped => status == 5;

        // Token: 0x17000012 RID: 18
        // (get) Token: 0x0600005B RID: 91 RVA: 0x000037F9 File Offset: 0x000019F9
        public bool Cancelling => status == 8;

        // Token: 0x17000013 RID: 19
        // (get) Token: 0x0600005C RID: 92 RVA: 0x00003804 File Offset: 0x00001A04
        public ThingDef spoilProductDef
        {
            get
            {
                var defname = Fermenter is Building_Fermenter
                    ? (Fermenter as Building_Fermenter)?.SpoilProduct
                    : (Fermenter as Building_FermenterVat)?.SpoilProduct;

                return DefDatabase<ThingDef>.GetNamed(defname, false);
            }
        }

        // Token: 0x17000014 RID: 20
        // (get) Token: 0x0600005D RID: 93 RVA: 0x00003850 File Offset: 0x00001A50
        public ThingDef outputProductDef
        {
            get
            {
                var defname = Fermenter is Building_Fermenter
                    ? (Fermenter as Building_Fermenter)?.FermentProduct
                    : (Fermenter as Building_FermenterVat)?.FermentProduct;

                return DefDatabase<ThingDef>.GetNamed(defname, false);
            }
        }

        // Token: 0x17000015 RID: 21
        // (get) Token: 0x0600005E RID: 94 RVA: 0x0000389C File Offset: 0x00001A9C
        public ThingDef inputResourceDef
        {
            get
            {
                var defname = Fermenter is Building_Fermenter
                    ? (Fermenter as Building_Fermenter)?.FermentResource
                    : (Fermenter as Building_FermenterVat)?.FermentResource;

                return DefDatabase<ThingDef>.GetNamed(defname, false);
            }
        }

        // Token: 0x0600004F RID: 79 RVA: 0x0000367C File Offset: 0x0000187C
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref spaceLeft, "spaceLeft", 240);
            Scribe_Values.Look(ref ingredients, "ingredients");
            Scribe_Values.Look(ref products, "products");
            Scribe_Values.Look(ref status, "status");
            Scribe_Values.Look(ref producing, "producing");
            Scribe_Values.Look(ref prevStatus, "prevStatus");
            Scribe_Values.Look(ref fermentTicks, "fermentTicks");
            Scribe_Values.Look(ref spoilTicks, "spoilTicks");
            Scribe_Values.Look(ref resettingTicks, "resettingTicks");
            Scribe_Values.Look(ref cleanTicks, "cleanTicks");
            Scribe_Values.Look(ref stockLimit, "stockLimit");
            Scribe_Values.Look(ref batchsize, "batchsize", 250);
            Scribe_Values.Look(ref stopDueToStock, "stopDueToStock");
        }

        // Token: 0x0600005F RID: 95 RVA: 0x000038E8 File Offset: 0x00001AE8
        public override string CompInspectStringExtra()
        {
            var temp = (int) Fermenter.AmbientTemperature;
            string temptype = "Fermenter.tempfine".Translate();
            if (temp < -1 || temp > 32)
            {
                temptype = "Fermenter.tempbad".Translate();
            }
            else if (temp < 7)
            {
                temptype = "Fermenter.temppoor".Translate();
            }

            string warning;
            if (Fermenter is Building_Fermenter)
            {
                warning = isFunctional(Fermenter)
                    ? "Fermenter.tempregulated".Translate()
                    : "Fermenter.tempwarning".Translate(temptype);
            }
            else
            {
                warning = "Fermenter.tempwarning".Translate(temptype);
            }

            string tempscale = "Fermenter.TmpCelsius".Translate();
            if (Prefs.TemperatureMode == TemperatureDisplayMode.Celsius)
            {
                return "Fermenter.tempmsg".Translate(temp.ToString(), warning, tempscale);
            }

            if (Prefs.TemperatureMode == TemperatureDisplayMode.Fahrenheit)
            {
                tempscale = "Fermenter.TmpFahrenheit".Translate();
                temp = (int) GenTemperature.CelsiusTo(temp, TemperatureDisplayMode.Fahrenheit);
            }
            else
            {
                tempscale = "Fermenter.TmpKelvin".Translate();
                temp = (int) GenTemperature.CelsiusTo(temp, TemperatureDisplayMode.Kelvin);
            }

            return "Fermenter.tempmsg".Translate(temp.ToString(), warning, tempscale);
        }

        // Token: 0x06000060 RID: 96 RVA: 0x00003A30 File Offset: 0x00001C30
        public override void CompTick()
        {
            base.CompTick();
            if (!Fermenter.Spawned || !Fermenter.IsHashIntervalTick(120))
            {
                return;
            }

            if (isFunctional(Fermenter))
            {
                if (stopDueToStock && !stockLimit)
                {
                    stopDueToStock = false;
                    producing = true;
                }

                if (producing)
                {
                    if (Idling)
                    {
                        if (outputProductDef != null && inputResourceDef != null && !stockLimit)
                        {
                            ChangeStatus(1);
                        }
                    }
                    else if (Filling)
                    {
                        if (stockLimit)
                        {
                            stopDueToStock = true;
                            DoCancel();
                            return;
                        }

                        if (!CheckIsFull())
                        {
                            return;
                        }

                        ChangeStatus(2);
                        if (Fermenter is Building_Fermenter)
                        {
                            fermentTicks = (int) (((Building_Fermenter) Fermenter).FermentHours * 2500f);
                            return;
                        }

                        fermentTicks = (int) (((Building_FermenterVat) Fermenter).FermentHours * 2500f);
                    }
                    else
                    {
                        if (Fermenting)
                        {
                            DoFermentation();
                            return;
                        }

                        if (Removing)
                        {
                            if (Fermented)
                            {
                                return;
                            }

                            if (Fermenter is Building_Fermenter)
                            {
                                ChangeStatus(4);
                                cleanTicks = (int) (GetCleaning() * 2500f);
                                return;
                            }

                            DoReset();
                        }
                        else if (Cleaning)
                        {
                            if (cleanTicks > 0)
                            {
                                cleanTicks -= 120;
                                return;
                            }

                            DoReset();
                        }
                        else if (Spoiled)
                        {
                            if (Fermented)
                            {
                                return;
                            }

                            if (Fermenter is Building_Fermenter)
                            {
                                ChangeStatus(4);
                                cleanTicks = (int) (GetCleaning() * 2500f);
                            }
                            else
                            {
                                DoReset();
                            }

                            producing = false;
                        }
                        else if (Resetting)
                        {
                            if (resettingTicks > 0)
                            {
                                resettingTicks -= 120;
                                return;
                            }

                            ChangeStatus(0);
                            resettingTicks = 0;
                            batchsize = (Fermenter as Building_Fermenter)?.BatchLimit ??
                                        ((Building_FermenterVat) Fermenter).BatchLimit;

                            spaceLeft = batchsize;
                            products = 0;
                            ingredients = 0;
                            fermentTicks = 0;
                            cleanTicks = 0;
                            spoilTicks = 0;
                        }
                        else if (Cancelling && ingredients <= 0)
                        {
                            ingredients = 0;
                            spaceLeft = batchsize;
                            ChangeStatus(7);
                            producing = false;
                        }
                    }
                }
                else if (Fermenting)
                {
                    DoFermentation();
                }
            }
            else if (Fermenting)
            {
                var temp = Fermenter.AmbientTemperature;
                if (temp < -1f)
                {
                    spoilTicks += 120;
                }
                else if (temp < 7f)
                {
                    spoilTicks += 60;
                }
                else if (temp > 32f)
                {
                    spoilTicks += 120;
                }

                var totSpoil = GetSpoiling();
                if (totSpoil > 0 && spoilTicks >= totSpoil)
                {
                    DoSpoil();
                }
            }
        }

        // Token: 0x06000061 RID: 97 RVA: 0x00003D68 File Offset: 0x00001F68

        // Token: 0x06000062 RID: 98 RVA: 0x00003D70 File Offset: 0x00001F70
        public void DoFermentation()
        {
            if (fermentTicks > 0)
            {
                fermentTicks -= 120;
                return;
            }

            fermentTicks = 0;
            products = batchsize - spaceLeft;
            ingredients = 0;
            if (products > 0)
            {
                ChangeStatus(3);
                return;
            }

            if (Fermenter is Building_Fermenter)
            {
                ChangeStatus(4);
                cleanTicks = (int) (GetCleaning() * 2500f);
                return;
            }

            DoReset();
        }

        // Token: 0x06000063 RID: 99 RVA: 0x00003DF6 File Offset: 0x00001FF6
        public void DoReset()
        {
            cleanTicks = 0;
            ChangeStatus(7);
            resettingTicks = 1250;
            ingredients = 0;
            products = 0;
            spaceLeft = batchsize;
        }

        // Token: 0x06000064 RID: 100 RVA: 0x00003E2C File Offset: 0x0000202C
        public void DoSpoil()
        {
            fermentTicks = 0;
            ingredients = 0;
            if (spoilProductDef != null)
            {
                products = batchsize - spaceLeft;
                ChangeStatus(6);
            }
            else
            {
                products = 0;
                spaceLeft = batchsize;
                if (Fermenter is Building_Fermenter)
                {
                    ChangeStatus(4);
                    cleanTicks = (int) (GetCleaning() * 2500f);
                }
                else
                {
                    DoReset();
                }
            }

            spoilTicks = 0;
            Messages.Message("Fermenter.SpoiledMsg".Translate(), Fermenter, MessageTypeDefOf.NeutralEvent);
        }

        // Token: 0x06000065 RID: 101 RVA: 0x00003ED9 File Offset: 0x000020D9
        public void DoCancel()
        {
            ChangeStatus(8);
        }

        // Token: 0x06000066 RID: 102 RVA: 0x00003EE2 File Offset: 0x000020E2
        public void ChangeStatus(int newStatus)
        {
            prevStatus = status;
            status = newStatus;
        }

        // Token: 0x06000067 RID: 103 RVA: 0x00003EF8 File Offset: 0x000020F8
        public float GetCleaning()
        {
            var cleaning = Props.cleanHours;
            if (cleaning < 1f)
            {
                cleaning = 1f;
            }

            return Math.Min(cleaning, 5f);
        }

        // Token: 0x06000068 RID: 104 RVA: 0x00003F2C File Offset: 0x0000212C
        public int GetSpoiling()
        {
            var totSpoil = Props.totalSpoilTicks;
            if (totSpoil < 2500)
            {
                totSpoil = 2500;
            }

            return Math.Min(totSpoil, 180000);
        }

        // Token: 0x06000069 RID: 105 RVA: 0x00003F5E File Offset: 0x0000215E
        public bool CheckIsFull()
        {
            return spaceLeft <= 0;
        }

        // Token: 0x0600006A RID: 106 RVA: 0x00003F6C File Offset: 0x0000216C
        public bool isFunctional(Thing fermenter)
        {
            if (fermenter == null)
            {
                return true;
            }

            if (fermenter is Building_Fermenter)
            {
                if (fermenter.IsBrokenDown())
                {
                    return false;
                }

                var compPow = fermenter.TryGetComp<CompPowerTrader>();
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
                var temp = ((Building) fermenter).AmbientTemperature;
                if (temp < 7f || temp > 32f)
                {
                    return false;
                }
            }

            return true;
        }
    }
}