using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RepairBench
{
    public class JobDriver_RepairItem : JobDriver
    {
        private int cachedTicksTotal = -1;

        private Thing Item => job.targetA.Thing;

        private Thing Bench => job.targetB.Thing;

        private int Kits => Mathf.Max(1, job.count);

        private int TicksTotal
        {
            get
            {
                if (cachedTicksTotal < 0)
                {
                    Thing item = Item;
                    RepairBenchExtension ext = RepairBenchUtility.Extension;
                    cachedTicksTotal = item != null && ext != null
                        ? RepairBenchUtility.TicksFor(item, ext, RepairBenchUtility.WorkSpeedFor(pawn))
                        : 60;
                }
                return cachedTicksTotal;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed))
            {
                return false;
            }
            if (!pawn.Reserve(job.targetB, job, 1, -1, null, errorOnFailed))
            {
                return false;
            }
            Thing bench = Bench;
            if (bench != null && bench.def.hasInteractionCell &&
                !pawn.ReserveSittableOrSpot(bench.InteractionCell, job, errorOnFailed))
            {
                return false;
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoItem = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            gotoItem.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            gotoItem.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return gotoItem;

            yield return Toils_Haul.StartCarryThing(TargetIndex.A);

            Toil gotoBench = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
            gotoBench.FailOnDestroyedOrNull(TargetIndex.B);
            yield return gotoBench;

            Toil drop = ToilMaker.MakeToil("RepairBench_DropCarriedItem");
            drop.initAction = () =>
            {
                if (pawn.carryTracker.CarriedThing == null)
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }
                if (!pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _))
                {
                    EndJobWith(JobCondition.Incompletable);
                }
            };
            drop.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return drop;

            Thing item = Item;
            int total = TicksTotal;
            RepairBenchProgress progress = pawn.Map.GetComponent<RepairBenchProgress>();
            int done = progress != null ? progress.Get(item) : 0;
            int remaining = Mathf.Max(1, total - done);

            Toil wait = Toils_General.Wait(remaining, TargetIndex.B);
            wait.tickAction = () =>
            {
                if (progress != null)
                {
                    progress.Add(item, 1);
                }
                pawn.skills?.Learn(SkillDefOf.Crafting, 0.1f);
            };
            wait.WithProgressBar(TargetIndex.A, () =>
            {
                int current = progress != null ? progress.Get(item) : 0;
                return total > 0 ? Mathf.Clamp01((float)current / total) : 1f;
            });
            wait.PlaySustainerOrSound(() => SoundDef.Named("Recipe_Smith"));
            wait.FailOnDestroyedOrNull(TargetIndex.A);
            wait.FailOnDestroyedOrNull(TargetIndex.B);
            wait.AddFailCondition(() => !IsStillDesignated(item));
            yield return wait;

            yield return Toils_General.DoAtomic(FinishRepair);
        }

        private bool IsStillDesignated(Thing item)
        {
            return item != null && pawn.Map.designationManager.DesignationOn(item, RepairBenchDefOf.RepairBench_RepairDesignation) != null;
        }

        private void FinishRepair()
        {
            Thing item = Item;
            Thing bench = Bench;
            int kits = Kits;
            CompRefuelable comp = bench != null ? bench.TryGetComp<CompRefuelable>() : null;
            if (item == null || comp == null || comp.Fuel < kits)
            {
                EndJobWith(JobCondition.Incompletable);
                return;
            }
            comp.ConsumeFuel(kits);
            item.HitPoints = item.MaxHitPoints;
            RepairBenchProgress progress = pawn.Map.GetComponent<RepairBenchProgress>();
            if (progress != null)
            {
                progress.Clear(item);
            }
            pawn.Map.designationManager.TryRemoveDesignationOn(item, RepairBenchDefOf.RepairBench_RepairDesignation);
        }
    }
}
