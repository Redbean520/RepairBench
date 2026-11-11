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
            Job job = RepairBenchUtility.TryMakeRepairJob(pawn, t, null, true, forced, out string failReason);
            if (job == null && forced && failReason != null)
            {
                JobFailReason.Is(failReason);
            }
            return job;
        }
    }
}
