using Verse;

namespace Fermenter;

public class CompProperties_Ferment : CompProperties
{
    public readonly float cleanHours = 3f;

    public readonly int totalSpoilTicks = 60000;

    public CompProperties_Ferment()
    {
        compClass = typeof(CompFerment);
    }
}