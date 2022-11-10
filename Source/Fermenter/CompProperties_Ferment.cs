using Verse;

namespace Fermenter;

public class CompProperties_Ferment : CompProperties
{
    public float cleanHours = 3f;

    public int totalSpoilTicks = 60000;

    public CompProperties_Ferment()
    {
        compClass = typeof(CompFerment);
    }
}