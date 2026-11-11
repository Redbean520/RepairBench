using RimWorld;
using Verse;

namespace RepairBench
{
    [DefOf]
    public static class RepairBenchDefOf
    {
        public static DesignationDef RepairBench_RepairDesignation;
        public static ResearchProjectDef RepairBench_RepairTech;
        public static ThingDef RepairBench_Bench;
        public static ThingDef RepairBench_RepairKit;
        public static RecipeDef RepairBench_MakeRepairKit;
        public static WorkTypeDef RepairBench_Repairing;
        public static WorkGiverDef RepairBench_RepairDesignated;
        public static JobDef RepairBench_RepairItem;

        // 自定义修补特效与收尾音（Defs/Effects/RepairBench_Effecters.xml、Defs/SoundDefs/RepairBench_Sounds.xml）
        public static EffecterDef RepairBench_RepairEffect;
        public static SoundDef RepairBench_RepairDone;

        // 干活时小人的轻微晃动（Defs/AnimationDefs/RepairBench_Animation.xml）
        public static AnimationDef RepairBench_WorkAnimation;
    }
}
