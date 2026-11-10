using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RepairBench
{
    public class WorkGiver_RepairDesignated : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.HaulableEver);

        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.designationManager
                .SpawnedDesignationsOfDef(RepairBenchDefOf.RepairBench_RepairDesignation)
                .Select(d => d.target.Thing)
                .Where(t => t != null)
                .Distinct();
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !pawn.Map.designationManager
                .SpawnedDesignationsOfDef(RepairBenchDefOf.RepairBench_RepairDesignation)
                .Any();
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobOnThing(pawn, t, forced) != null;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!RepairBenchUtility.IsValidItem(t))
            {
                return null;
            }
            if (pawn.Map.designationManager.DesignationOn(t, RepairBenchDefOf.RepairBench_RepairDesignation) == null)
            {
                return null;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return null;
            }
            RepairBenchExtension ext = RepairBenchUtility.Extension;
            if (ext == null)
            {
                return null;
            }
            int kits = RepairBenchUtility.KitsFor(t, ext);
            Thing bench = RepairBenchUtility.FindUsableBench(pawn, kits);
            if (bench == null)
            {
                if (forced)
                {
                    JobFailReason.Is("RepairBench_NoUsableBench".Translate());
                }
                return null;
            }
            if (!pawn.CanReserve(bench, 1, -1, null, forced))
            {
                return null;
            }
            Job job = JobMaker.MakeJob(RepairBenchDefOf.RepairBench_RepairItem, t, bench);
            job.count = kits;
            return job;
        }
    }
}
