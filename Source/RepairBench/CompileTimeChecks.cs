using RimWorld;
using Verse;
using Verse.AI;

namespace RepairBench
{
    /// <summary>
    /// Compile-time contracts. Nothing here is ever called: the only job of this file is
    /// to stop the build if a class loses the base type or constructor that the game's
    /// reflection based registration depends on (WorkGiver XML / JobDef driverClass /
    /// Alert discovery / MapComponent discovery).
    /// </summary>
    internal static class CompileTimeChecks
    {
        internal static void Assert()
        {
            Use(new WorkGiver_RepairDesignated());
            Use(new JobDriver_RepairItem());
            Use(new Designator_RepairItem());
            Use(new RepairBenchExtension());
            Use(new RepairBenchProgress(null));
            Use(new Alert_RepairBenchPending());
            Use(new FloatMenuOptionProvider_RepairBench());
            Use(new CompRepairBench());
        }

        private static void Use(WorkGiver_Scanner value) { }
        private static void Use(JobDriver value) { }
        private static void Use(Designator value) { }
        private static void Use(DefModExtension value) { }
        private static void Use(MapComponent value) { }
        private static void Use(Alert value) { }
        private static void Use(FloatMenuOptionProvider value) { }
        private static void Use(ThingComp value) { }
    }
}
