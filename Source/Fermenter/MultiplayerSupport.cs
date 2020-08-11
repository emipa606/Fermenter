using System;
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
		// Token: 0x06000088 RID: 136 RVA: 0x00004998 File Offset: 0x00002B98
		static MultiplayerSupport()
		{
			if (!MP.enabled)
			{
				return;
			}
			MP.RegisterSyncMethod(typeof(Building_Fermenter), "FermenterSelectProd", null);
			MP.RegisterSyncMethod(typeof(Building_Fermenter), "SetProdValues", null);
			MP.RegisterSyncMethod(typeof(Building_FermenterVat), "FermenterSelectProd", null);
			MP.RegisterSyncMethod(typeof(Building_FermenterVat), "SetProdValues", null);
			MP.RegisterSyncMethod(typeof(Building_Fermenter), "ToggleProducing", null);
			MP.RegisterSyncMethod(typeof(Building_FermenterVat), "ToggleProducing", null);
			MP.RegisterSyncMethod(typeof(Building_Fermenter), "FermenterSelectLimit", null);
			MP.RegisterSyncMethod(typeof(Building_Fermenter), "SetStockLimits", null);
			MP.RegisterSyncMethod(typeof(Building_FermenterVat), "FermenterSelectLimit", null);
			MP.RegisterSyncMethod(typeof(Building_FermenterVat), "SetStockLimits", null);
			MP.RegisterSyncMethod(typeof(Building_Fermenter), "FermenterSelectBatch", null);
			MP.RegisterSyncMethod(typeof(Building_Fermenter), "SetBatchLimits", null);
			MP.RegisterSyncMethod(typeof(Building_FermenterVat), "FermenterSelectBatch", null);
			MP.RegisterSyncMethod(typeof(Building_FermenterVat), "SetBatchLimits", null);
			MP.RegisterSyncMethod(typeof(Building_Fermenter), "FermenterCancelBatch", null);
			MP.RegisterSyncMethod(typeof(Building_Fermenter), "SetCancelBatch", null);
			MP.RegisterSyncMethod(typeof(Building_FermenterVat), "FermenterCancelBatch", null);
			MP.RegisterSyncMethod(typeof(Building_FermenterVat), "SetCancelBatch", null);
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00004B48 File Offset: 0x00002D48
		private static void FixRNG(MethodInfo method)
		{
			MultiplayerSupport.harmony.Patch(method, new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPre", null), new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPos", null), null, null);
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

		// Token: 0x04000033 RID: 51
		private static Harmony harmony = new Harmony("rimworld.Fermenter.multiplayersupport");
	}
}
