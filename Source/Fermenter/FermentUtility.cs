using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Fermenter
{
    // Token: 0x0200000B RID: 11
    public class FermentUtility
    {
        // Token: 0x04000026 RID: 38
        [NoTranslate] public static readonly string ProdTexPath = "Special/Product_Icon";

        // Token: 0x04000027 RID: 39
        [NoTranslate] public static readonly string produceTexPath = "Special/Produce_Icon";

        // Token: 0x04000028 RID: 40
        [NoTranslate] public static readonly string FrontLimitPath = "Special/StockLimits/FermenterStock";

        // Token: 0x04000029 RID: 41
        [NoTranslate] public static readonly string FrontBatchPath = "Special/BatchLimits/FermenterBatch";

        // Token: 0x0400002A RID: 42
        [NoTranslate] public static readonly string EndLimitPath = "Limit_icon";

        // Token: 0x0400002B RID: 43
        [NoTranslate] public static readonly string CancelTexPath = "Special/Cancel_Icon";

        // Token: 0x0600006F RID: 111 RVA: 0x00004058 File Offset: 0x00002258
        public static string statusDesc(int status)
        {
            string desc;
            switch (status)
            {
                case 1:
                    desc = "Fermenter.WaitFill".Translate();
                    break;
                case 2:
                    desc = "Fermenter.Fermenting".Translate();
                    break;
                case 3:
                    desc = "Fermenter.WaitEmpty".Translate();
                    break;
                case 4:
                    desc = "Fermenter.Cleaning".Translate();
                    break;
                case 5:
                    desc = "Fermenter.Stopped".Translate();
                    break;
                case 6:
                    desc = "Fermenter.Spoiled".Translate();
                    break;
                case 7:
                    desc = "Fermenter.Resetting".Translate();
                    break;
                case 8:
                    desc = "Fermenter.Cancelling".Translate();
                    break;
                default:
                    desc = "Fermenter.Idle".Translate();
                    break;
            }

            return desc;
        }

        // Token: 0x06000070 RID: 112 RVA: 0x00004148 File Offset: 0x00002348
        public static string GetStatusDesc(Building b)
        {
            var desc = " ";
            var comp = b.TryGetComp<CompFerment>();
            if (comp == null)
            {
                return desc;
            }

            desc += statusDesc(comp.status);
            switch (comp.status)
            {
                case 1:
                    desc = string.Concat(desc, " (", comp.ingredients.ToString(), "/", comp.batchsize.ToString(),
                        ")");
                    break;
                case 3:
                    desc = desc + " (" + comp.products + ")";
                    break;
                case 4:
                    desc = desc + " (" + Math.Min(1f,
                            ((comp.GetCleaning() * 2500f) - comp.cleanTicks) / (comp.GetCleaning() * 2500f))
                        .ToStringPercent() + ")";
                    break;
                case 6:
                    desc = desc + " (" + comp.products + ")";
                    break;
                case 7:
                    desc = desc + " (" + Math.Min(1f, (1250 - comp.resettingTicks) / 1250f).ToStringPercent() + ")";
                    break;
                case 8:
                    desc = desc + " (" + comp.ingredients + ")";
                    break;
            }

            return desc;
        }

        // Token: 0x06000071 RID: 113 RVA: 0x000042D0 File Offset: 0x000024D0
        public static Texture2D GetProductIcon(string defName, out ThingDef def)
        {
            def = null;
            if (defName == "")
            {
                return ContentFinder<Texture2D>.Get(ProdTexPath);
            }

            def = ThingDef.Named(defName);
            if (def == null)
            {
                return ContentFinder<Texture2D>.Get(ProdTexPath);
            }

            var thingDef = def;
            if (thingDef?.uiIcon != null)
            {
                return def.uiIcon;
            }

            var thingDef2 = def;
            bool b;
            if (thingDef2 == null)
            {
                b = false;
            }
            else
            {
                var graphicData = thingDef2.graphicData;
                b = graphicData?.texPath != null;
            }

            if (!b)
            {
                return ContentFinder<Texture2D>.Get(ProdTexPath);
            }

            var texturePath = def.graphicData.texPath;
            if (def.graphicData.graphicClass.Name == "Graphic_StackCount")
            {
                texturePath = texturePath + "/" + def.defName + "_a";
            }

            return ContentFinder<Texture2D>.Get(texturePath);
        }

        // Token: 0x06000072 RID: 114 RVA: 0x000043A6 File Offset: 0x000025A6
        internal static bool IsResearched(string resProj)
        {
            return resProj == "" || ResearchProjectDef.Named(resProj).IsFinished;
        }

        // Token: 0x06000073 RID: 115 RVA: 0x000043C8 File Offset: 0x000025C8
        internal static void ResNotFoundOverlay(Building b, ThingDef def)
        {
            if (Find.CurrentMap == null || Find.CurrentMap != b.Map)
            {
                return;
            }

            var OutOfFuelMat = MaterialPool.MatFrom("UI/Overlays/OutOfFuel", ShaderDatabase.MetaOverlay);
            var mat = MaterialPool.MatFrom(def.uiIcon, ShaderDatabase.MetaOverlay, Color.white);
            var BaseAlt = AltitudeLayer.WorldClipper.AltitudeFor();
            if (mat == null)
            {
                return;
            }

            var altInd = 21;
            var plane = MeshPool.plane08;
            var drawPos = b.TrueCenter();
            drawPos.y = BaseAlt + (0.046875f * altInd);
            var num2 = ((float) Math.Sin((Time.realtimeSinceStartup + (397f * (b.thingIDNumber % 571))) * 4f) +
                        1f) * 0.5f;
            num2 = 0.3f + (num2 * 0.7f);
            for (var i = 0; i < 2; i++)
            {
                var material = FadedMaterialPool.FadedVersionOf(i < 1 ? mat : OutOfFuelMat, num2);

                if (material != null)
                {
                    Graphics.DrawMesh(plane, drawPos, Quaternion.identity, material, 0);
                }
            }
        }
    }
}