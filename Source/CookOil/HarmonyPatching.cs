using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace CookOil
{
	// Token: 0x02000003 RID: 3
	[StaticConstructorOnStartup]
	internal static class HarmonyPatching
	{
		// Token: 0x06000007 RID: 7 RVA: 0x000024A0 File Offset: 0x000006A0
		static HarmonyPatching()
		{
			new Harmony("com.Pelador.Rimworld.CookOil").PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}
