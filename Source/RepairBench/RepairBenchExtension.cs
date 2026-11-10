using Verse;

namespace RepairBench
{
    /// <summary>
    /// Tuning data for the repair bench. Attached to the bench ThingDef via modExtensions.
    /// </summary>
    public class RepairBenchExtension : DefModExtension
    {
        /// <summary>Ticks of work needed to repair an item from 0% to 100% condition.</summary>
        public int ticksForFullRepair = 3000;

        /// <summary>Repair kits consumed to repair an item from 0% to 100% condition.</summary>
        public int kitsForFullRepair = 10;
    }
}
