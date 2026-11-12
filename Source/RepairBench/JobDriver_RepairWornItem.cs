using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RepairBench
{
    /// <summary>
    /// Repairs one piece of gear the same pawn is wearing / holding: the pawn walks to the repair bench,
    /// takes the gear off itself, puts it on the bench, repairs it with the normal progress bar / effect /
    /// sound, then puts it back on. No hauling target and no ground designation: this job is only ever
    /// started from the right-click menu on a bench.
    ///
    /// targetA = the gear (initially worn / equipped, so not spawned), targetB = the bench, job.count = kits.
    /// </summary>
    public class JobDriver_RepairWornItem : JobDriver
    {
        private int cachedTicksTotal = -1;
        private bool ownsWorkAnimation;
        private bool gearTakenOff;

        private Thing Gear => job.targetA.Thing;
        private Thing Bench => job.targetB.Thing;
        private int Kits => Mathf.Max(1, job.count);

        private int TicksTotal
        {
            get
            {
                if (cachedTicksTotal < 0)
                {
                    Thing gear = Gear;
                    RepairBenchExtension ext = RepairBenchUtility.Extension;
                    cachedTicksTotal = (gear != null && ext != null)
                        ? RepairBenchUtility.TicksFor(gear, ext, RepairBenchUtility.WorkSpeedFor(pawn))
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
            AddFinishAction(condition =>
            {
                StopWorkAnimation();
                RestoreGearAfterJob(condition);
            });

            Toil gotoBench = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
            gotoBench.FailOnDestroyedOrNull(TargetIndex.B);
            yield return gotoBench;

            Toil takeOff = ToilMaker.MakeToil("RepairBench_TakeOffGear");
            takeOff.defaultCompleteMode = ToilCompleteMode.Instant;
            takeOff.initAction = TakeOffAndPlaceOnBench;
            yield return takeOff;

            Thing gear = Gear;
            int total = TicksTotal;
            RepairBenchProgress progress = pawn.Map != null ? pawn.Map.GetComponent<RepairBenchProgress>() : null;
            int done = progress != null ? progress.Get(gear) : 0;
            int remaining = Mathf.Max(1, total - done);

            Toil wait = Toils_General.Wait(remaining, TargetIndex.B);
            wait.tickAction = delegate
            {
                progress?.Add(gear, 1);
                pawn.skills?.Learn(SkillDefOf.Crafting, 0.1f);
            };
            wait.WithProgressBar(TargetIndex.A, delegate
            {
                int current = progress != null ? progress.Get(gear) : 0;
                return total > 0 ? Mathf.Clamp01((float)current / total) : 1f;
            });
            wait.WithEffect(() => RepairBenchDefOf.RepairBench_RepairEffect, TargetIndex.A);
            wait.activeSkill = () => SkillDefOf.Crafting;
            wait.FailOnDestroyedOrNull(TargetIndex.A);
            wait.FailOnDestroyedOrNull(TargetIndex.B);
            wait.AddPreInitAction(StartWorkAnimation);
            wait.AddFinishAction(StopWorkAnimation);
            yield return wait;

            yield return Toils_General.DoAtomic(FinishRepairWorn);
        }

        private void TakeOffAndPlaceOnBench()
        {
            Thing gear = Gear;
            Thing bench = Bench;
            if (gear == null || bench == null || pawn.Map == null)
            {
                EndJobWith(JobCondition.Incompletable);
                return;
            }
            if (!TakeOff(gear))
            {
                // 玩家在菜单打开后自己脱/换掉了这件装备，或它被锁住：不修，安静收尾。
                EndJobWith(JobCondition.Incompletable);
                return;
            }
            gearTakenOff = true;
            if (gear.Spawned && !pawn.carryTracker.TryStartCarry(gear))
            {
                EndJobWith(JobCondition.Incompletable);
                return;
            }
            IntVec3 benchCell = FindFreeBenchCell(bench);
            if (benchCell.IsValid && pawn.carryTracker.TryDropCarriedThing(benchCell, ThingPlaceMode.Direct, out _))
            {
                return;
            }
            if (!pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _))
            {
                EndJobWith(JobCondition.Incompletable);
            }
        }

        private bool TakeOff(Thing gear)
        {
            if (gear is Apparel apparel)
            {
                if (pawn.apparel == null || !pawn.apparel.WornApparel.Contains(apparel))
                {
                    return false;
                }
                return pawn.apparel.TryDrop(apparel, out _, pawn.Position, forbid: false);
            }
            if (gear is ThingWithComps equipment)
            {
                if (pawn.equipment == null || !pawn.equipment.Contains(equipment))
                {
                    return false;
                }
                return pawn.equipment.TryDropEquipment(equipment, out _, pawn.Position, forbid: false);
            }
            return false;
        }

        private IntVec3 FindFreeBenchCell(Thing bench)
        {
            Map map = bench.Map;
            if (map == null)
            {
                return IntVec3.Invalid;
            }
            IntVec3 best = IntVec3.Invalid;
            int bestDist = int.MaxValue;
            foreach (IntVec3 cell in bench.OccupiedRect().Cells)
            {
                if (!cell.InBounds(map) || cell.GetItemCount(map) > 0)
                {
                    continue;
                }
                int dist = (cell - pawn.Position).LengthHorizontalSquared;
                if (dist < bestDist)
                {
                    best = cell;
                    bestDist = dist;
                }
            }
            return best;
        }

        private void FinishRepairWorn()
        {
            Thing gear = Gear;
            Thing bench = Bench;
            if (gear == null || bench == null)
            {
                EndJobWith(JobCondition.Incompletable);
                return;
            }
            CompRefuelable comp = bench.TryGetComp<CompRefuelable>();
            if (comp == null || comp.Fuel < Kits)
            {
                EndJobWith(JobCondition.Incompletable);
                return;
            }
            comp.ConsumeFuel(Kits);
            gear.HitPoints = gear.MaxHitPoints;
            pawn.Map?.GetComponent<RepairBenchProgress>()?.Clear(gear);
            if (!PutGearBackOn(gear))
            {
                Log.Warning("[Repair Bench] Could not re-equip " + gear.LabelShort + " after repairing; leaving it next to the bench.");
                MoveItemOffBench(gear, bench);
            }
            if (RepairBenchDefOf.RepairBench_RepairDone != null)
            {
                RepairBenchDefOf.RepairBench_RepairDone.PlayOneShot(new TargetInfo(bench.Position, pawn.Map));
            }
        }

        /// <summary>
        /// Finish-action fallback: if the job was interrupted (mental break, drafted, slept, bench
        /// destroyed...) while the gear was off, put it back on instead of leaving the colonist
        /// unarmed / unarmoured. Does nothing when the pawn no longer holds the item or already wears it.
        /// </summary>
        private void RestoreGearAfterJob(JobCondition condition)
        {
            Thing gear = Gear;
            if (!gearTakenOff || gear == null || gear.Destroyed || pawn.Map == null)
            {
                return;
            }
            if (RepairBenchUtility.IsWornOrEquippedBy(pawn, gear))
            {
                return;
            }
            if (!gear.Spawned || gear.Map != pawn.Map)
            {
                return;
            }
            if (!PutGearBackOn(gear))
            {
                return;
            }
            if (condition != JobCondition.Succeeded)
            {
                Messages.Message("MessageRepairBenchReequipped".Translate(), MessageTypeDefOf.NeutralEvent, false);
            }
        }

        private bool PutGearBackOn(Thing gear)
        {
            if (gear == null || gear.Destroyed || pawn == null || pawn.Dead || !pawn.Spawned)
            {
                return false;
            }
            if (gear is Apparel apparel)
            {
                if (pawn.apparel == null)
                {
                    return false;
                }
                pawn.apparel.Wear(apparel);
                return pawn.apparel.WornApparel.Contains(apparel);
            }
            if (gear is ThingWithComps equipment)
            {
                if (pawn.equipment == null)
                {
                    return false;
                }
                // 原版 JobDriver_Equip 的重新装备顺序：DeSpawn -> MakeRoomFor -> AddEquipment。
                if (equipment.Spawned)
                {
                    equipment.DeSpawn();
                }
                pawn.equipment.MakeRoomFor(equipment);
                pawn.equipment.AddEquipment(equipment);
                equipment.def.soundInteract?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                return pawn.equipment.Contains(equipment);
            }
            return false;
        }

        private void MoveItemOffBench(Thing item, Thing bench)
        {
            if (item == null || bench == null || !item.Spawned)
            {
                return;
            }
            if (!bench.OccupiedRect().Contains(item.Position))
            {
                return;
            }
            Map map = pawn.Map;
            IntVec3 cell = bench.InteractionCell.IsValid ? bench.InteractionCell : pawn.Position;
            item.DeSpawn();
            if (!GenPlace.TryPlaceThing(item, cell, map, ThingPlaceMode.Near, out _))
            {
                GenSpawn.Spawn(item, cell, map);
            }
        }

        private void StartWorkAnimation()
        {
            ownsWorkAnimation = false;
            // 装了 Yayo's Animation 时交给它来动（桥把我们的 JobDef 映射成它的 Repair 动作）。
            if (!RepairBenchYayoCompat.UseOwnAnimation)
            {
                return;
            }
            if (pawn == null || !pawn.Spawned || pawn.Drawer == null || pawn.Drawer.renderer == null)
            {
                return;
            }
            PawnRenderer renderer = pawn.Drawer.renderer;
            if (renderer.HasAnimation)
            {
                return;
            }
            AnimationDef animation = RepairBenchDefOf.RepairBench_WorkAnimation;
            if (animation == null)
            {
                return;
            }
            renderer.SetAnimation(animation);
            ownsWorkAnimation = true;
        }

        private void StopWorkAnimation()
        {
            try
            {
                if (!ownsWorkAnimation || pawn == null || pawn.Drawer == null || pawn.Drawer.renderer == null)
                {
                    return;
                }
                PawnRenderer renderer = pawn.Drawer.renderer;
                AnimationDef animation = RepairBenchDefOf.RepairBench_WorkAnimation;
                if (animation != null && renderer.CurAnimation == animation)
                {
                    renderer.SetAnimation(null);
                }
            }
            finally
            {
                ownsWorkAnimation = false;
            }
        }
    }
}
