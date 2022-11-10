using System.Reflection;
using HarmonyLib;
using Multiplayer.API;
using Verse;

namespace Fermenter;

[StaticConstructorOnStartup]
internal static class MultiplayerSupport
{
    private static readonly Harmony harmony = new Harmony("rimworld.Fermenter.multiplayersupport");

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

    private static void FixRNG(MethodInfo method)
    {
        harmony.Patch(method, new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPre"),
            new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPos"));
    }

    private static void FixRNGPre()
    {
        Rand.PushState(Find.TickManager.TicksAbs);
    }

    private static void FixRNGPos()
    {
        Rand.PopState();
    }
}