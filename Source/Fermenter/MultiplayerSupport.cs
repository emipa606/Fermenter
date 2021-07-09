using System.Reflection;
using HarmonyLib;
using Multiplayer.API;
using Verse;

namespace Fermenter
{
    // Token: 0x02000010 RID: 16
    [StaticConstructorOnStartup]
    internal static class MultiplayerSupport
    {
        // Token: 0x04000033 RID: 51
        private static readonly Harmony harmony = new Harmony("rimworld.Fermenter.multiplayersupport");

        // Token: 0x06000088 RID: 136 RVA: 0x00004998 File Offset: 0x00002B98
        static MultiplayerSupport()
        {
            if (!MP.enabled)
            {
                return;
            }

            MP.RegisterSyncMethod(typeof(Building_Fermenter), "FermenterSelectProd");
            MP.RegisterSyncMethod(typeof(Building_Fermenter), "SetProdValues");
            MP.RegisterSyncMethod(typeof(Building_FermenterVat), "FermenterSelectProd");
            MP.RegisterSyncMethod(typeof(Building_FermenterVat), "SetProdValues");
            MP.RegisterSyncMethod(typeof(Building_Fermenter), "ToggleProducing");
            MP.RegisterSyncMethod(typeof(Building_FermenterVat), "ToggleProducing");
            MP.RegisterSyncMethod(typeof(Building_Fermenter), "FermenterSelectLimit");
            MP.RegisterSyncMethod(typeof(Building_Fermenter), "SetStockLimits");
            MP.RegisterSyncMethod(typeof(Building_FermenterVat), "FermenterSelectLimit");
            MP.RegisterSyncMethod(typeof(Building_FermenterVat), "SetStockLimits");
            MP.RegisterSyncMethod(typeof(Building_Fermenter), "FermenterSelectBatch");
            MP.RegisterSyncMethod(typeof(Building_Fermenter), "SetBatchLimits");
            MP.RegisterSyncMethod(typeof(Building_FermenterVat), "FermenterSelectBatch");
            MP.RegisterSyncMethod(typeof(Building_FermenterVat), "SetBatchLimits");
            MP.RegisterSyncMethod(typeof(Building_Fermenter), "FermenterCancelBatch");
            MP.RegisterSyncMethod(typeof(Building_Fermenter), "SetCancelBatch");
            MP.RegisterSyncMethod(typeof(Building_FermenterVat), "FermenterCancelBatch");
            MP.RegisterSyncMethod(typeof(Building_FermenterVat), "SetCancelBatch");
        }

        // Token: 0x06000089 RID: 137 RVA: 0x00004B48 File Offset: 0x00002D48
        private static void FixRNG(MethodInfo method)
        {
            harmony.Patch(method, new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPre"),
                new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPos"));
        }

        // Token: 0x0600008A RID: 138 RVA: 0x00004B82 File Offset: 0x00002D82
        private static void FixRNGPre()
        {
            Rand.PushState(Find.TickManager.TicksAbs);
        }

        // Token: 0x0600008B RID: 139 RVA: 0x00004B93 File Offset: 0x00002D93
        private static void FixRNGPos()
        {
            Rand.PopState();
        }
    }
}