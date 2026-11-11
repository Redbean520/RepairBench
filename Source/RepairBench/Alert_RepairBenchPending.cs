using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RepairBench
{
    /// <summary>
    /// Shows up when marked gear cannot be repaired because no repair bench has
    /// enough repair kits loaded. Registered by AlertsReadout through reflection,
    /// so this class needs no Def - only a public parameterless constructor.
    /// </summary>
    public class Alert_RepairBenchPending : Alert
    {
        public Alert_RepairBenchPending()
        {
            // High 而不是 Medium：Medium 会排在右侧警报列表下方、不响铃，玩家很容易完全没注意到。
            // 这条警报代表「已经标了要修、但修不了」，属于需要玩家马上处理的阻塞状态。
            defaultPriority = AlertPriority.High;
            defaultLabel = "AlertRepairBenchPending".Translate();
            defaultExplanation = "AlertRepairBenchPendingDesc".Translate();
        }

        public override AlertReport GetReport()
        {
            if (RepairBenchDefOf.RepairBench_RepairTech == null || !RepairBenchDefOf.RepairBench_RepairTech.IsFinished)
            {
                return AlertReport.Inactive;
            }
            RepairBenchExtension ext = RepairBenchUtility.Extension;
            if (ext == null)
            {
                return AlertReport.Inactive;
            }
            List<Map> maps = Find.Maps;
            if (maps == null)
            {
                return AlertReport.Inactive;
            }
            List<Thing> culprits = new List<Thing>();
            for (int i = 0; i < maps.Count; i++)
            {
                Map map = maps[i];
                if (map == null || map.designationManager == null)
                {
                    continue;
                }
                foreach (Designation designation in map.designationManager.SpawnedDesignationsOfDef(RepairBenchDefOf.RepairBench_RepairDesignation))
                {
                    Thing item = designation.target.Thing;
                    if (item == null)
                    {
                        continue;
                    }
                    int kits = RepairBenchUtility.KitsFor(item, ext);
                    if (RepairBenchUtility.FindUsableBench(map, kits, requirePawnChecks: false) == null)
                    {
                        culprits.Add(item);
                    }
                }
            }
            return culprits.Count > 0 ? AlertReport.CulpritsAre(culprits) : AlertReport.Inactive;
        }
    }
}
