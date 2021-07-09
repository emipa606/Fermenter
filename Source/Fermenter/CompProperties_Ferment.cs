using Verse;

namespace Fermenter
{
    // Token: 0x02000008 RID: 8
    public class CompProperties_Ferment : CompProperties
    {
        // Token: 0x0400001D RID: 29
        public float cleanHours = 3f;

        // Token: 0x0400001C RID: 28
        public int totalSpoilTicks = 60000;

        // Token: 0x0600006C RID: 108 RVA: 0x00003FE1 File Offset: 0x000021E1
        public CompProperties_Ferment()
        {
            compClass = typeof(CompFerment);
        }
    }
}