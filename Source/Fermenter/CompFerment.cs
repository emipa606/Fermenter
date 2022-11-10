using System;
using RimWorld;
using Verse;

namespace Fermenter;

public class CompFerment : ThingComp
{
    public int batchsize = 250;

    public int cleanTicks;

    public int fermentTicks;

    public int ingredients;

    public int prevStatus;

    public bool producing;

    public int products;

    public int resettingTicks;

    public int spaceLeft = 240;

    public int spoilTicks;

    public int status;

    public bool stockLimit;

    public bool stopDueToStock;

    public CompProperties_Ferment Props => (CompProperties_Ferment)props;

    public Thing Fermenter => parent;

    public bool Idling => status == 0;

    public bool Fermenting => status == 2;

    public bool Fermented => products > 0;

    public bool Filling => status == 1;

    public bool Removing => status == 3;

    public bool Cleaning => status == 4;

    public bool Resetting => status == 7;

    public bool Spoiled => status == 6;

    public bool Stopped => status == 5;

    public bool Cancelling => status == 8;

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

    public override string CompInspectStringExtra()
    {
        var temp = (int)Fermenter.AmbientTemperature;
        string temptype = "Fermenter.tempfine".Translate();
        switch (temp)
        {
            case < -1:
            case > 32:
                temptype = "Fermenter.tempbad".Translate();
                break;
            case < 7:
                temptype = "Fermenter.temppoor".Translate();
                break;
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
        switch (Prefs.TemperatureMode)
        {
            case TemperatureDisplayMode.Celsius:
                return "Fermenter.tempmsg".Translate(temp.ToString(), warning, tempscale);
            case TemperatureDisplayMode.Fahrenheit:
                tempscale = "Fermenter.TmpFahrenheit".Translate();
                temp = (int)GenTemperature.CelsiusTo(temp, TemperatureDisplayMode.Fahrenheit);
                break;
            default:
                tempscale = "Fermenter.TmpKelvin".Translate();
                temp = (int)GenTemperature.CelsiusTo(temp, TemperatureDisplayMode.Kelvin);
                break;
        }

        return "Fermenter.tempmsg".Translate(temp.ToString(), warning, tempscale);
    }

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
                    if (Fermenter is Building_Fermenter fermenter)
                    {
                        fermentTicks = (int)(fermenter.FermentHours * 2500f);
                        return;
                    }

                    fermentTicks = (int)(((Building_FermenterVat)Fermenter).FermentHours * 2500f);
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
                            cleanTicks = (int)(GetCleaning() * 2500f);
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
                            cleanTicks = (int)(GetCleaning() * 2500f);
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
                                    ((Building_FermenterVat)Fermenter).BatchLimit;

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
            switch (temp)
            {
                case < -1f:
                    spoilTicks += 120;
                    break;
                case < 7f:
                    spoilTicks += 60;
                    break;
                case > 32f:
                    spoilTicks += 120;
                    break;
            }

            var totSpoil = GetSpoiling();
            if (totSpoil > 0 && spoilTicks >= totSpoil)
            {
                DoSpoil();
            }
        }
    }


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
            cleanTicks = (int)(GetCleaning() * 2500f);
            return;
        }

        DoReset();
    }

    public void DoReset()
    {
        cleanTicks = 0;
        ChangeStatus(7);
        resettingTicks = 1250;
        ingredients = 0;
        products = 0;
        spaceLeft = batchsize;
    }

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
                cleanTicks = (int)(GetCleaning() * 2500f);
            }
            else
            {
                DoReset();
            }
        }

        spoilTicks = 0;
        Messages.Message("Fermenter.SpoiledMsg".Translate(), Fermenter, MessageTypeDefOf.NeutralEvent);
    }

    public void DoCancel()
    {
        ChangeStatus(8);
    }

    public void ChangeStatus(int newStatus)
    {
        prevStatus = status;
        status = newStatus;
    }

    public float GetCleaning()
    {
        var cleaning = Props.cleanHours;
        if (cleaning < 1f)
        {
            cleaning = 1f;
        }

        return Math.Min(cleaning, 5f);
    }

    public int GetSpoiling()
    {
        var totSpoil = Props.totalSpoilTicks;
        if (totSpoil < 2500)
        {
            totSpoil = 2500;
        }

        return Math.Min(totSpoil, 180000);
    }

    public bool CheckIsFull()
    {
        return spaceLeft <= 0;
    }

    public bool isFunctional(Thing fermenter)
    {
        switch (fermenter)
        {
            case null:
                return true;
            case Building_Fermenter when fermenter.IsBrokenDown():
                return false;
            case Building_Fermenter:
            {
                var compPow = fermenter.TryGetComp<CompPowerTrader>();
                if (compPow == null)
                {
                    return false;
                }

                if (!compPow.PowerOn)
                {
                    return false;
                }

                break;
            }
            default:
            {
                var temp = ((Building)fermenter).AmbientTemperature;
                if (temp is < 7f or > 32f)
                {
                    return false;
                }

                break;
            }
        }

        return true;
    }
}