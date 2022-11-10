using System.Reflection;
using HarmonyLib;
using Verse;

namespace CookOil;

[StaticConstructorOnStartup]
internal static class HarmonyPatching
{
    static HarmonyPatching()
    {
        new Harmony("com.Pelador.Rimworld.CookOil").PatchAll(Assembly.GetExecutingAssembly());
    }
}