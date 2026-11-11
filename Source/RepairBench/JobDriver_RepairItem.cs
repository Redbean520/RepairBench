using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RepairBench
{
    /**
     * 帮小人修补一件地上装备的 Job。
     *
     * 流程：走到装备 -> 拾起 -> 走到修补台互动格 -> 把装备摆到台面上
     *      -> 面向台面工作（自制修补特效：金属屑 + 锉磨循环音 + 进度条）
     *      -> 原子完成（扣修补剂、耐久回满、清进度与标记）-> 把修好的装备放回台旁地面。
     */
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
            // 兜底：Job 以任何方式结束（完成 / 失败 / 被打断 / 精神崩溃）都清掉工作动画，
            // 否则小人会一直带着 RepairBench_WorkAnimation 晃下去。
            AddFinishAction(condition => StopWorkAnimation());

            Toil gotoItem = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            gotoItem.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            gotoItem.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return gotoItem;

            yield return Toils_Haul.StartCarryThing(TargetIndex.A);

            Toil gotoBench = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
            gotoBench.FailOnDestroyedOrNull(TargetIndex.B);
            yield return gotoBench;

            // 把装备摆到修补台台面（原版工作台手感）；台面没有空格时退回台旁地面，流程仍可完成。
            Toil placeOnBench = ToilMaker.MakeToil("RepairBench_PlaceItemOnBench");
            placeOnBench.initAction = () =>
            {
                if (pawn.carryTracker.CarriedThing == null)
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }
                IntVec3 benchCell = FindFreeBenchCell();
                if (benchCell.IsValid && pawn.carryTracker.TryDropCarriedThing(benchCell, ThingPlaceMode.Direct, out _))
                {
                    return;
                }
                if (!pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _))
                {
                    EndJobWith(JobCondition.Incompletable);
                }
            };
            placeOnBench.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return placeOnBench;

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
            // 自己的修补特效：自定义锉磨音（sustainer）+ 崩出的金属屑，见 Defs/Effects/RepairBench_Effecters.xml。
            // 不用从 BuildingBase 继承的原版 repairEffect（那是焊花 + 原版修理音，语义不对）。
            wait.WithEffect(() => RepairBenchDefOf.RepairBench_RepairEffect, TargetIndex.A);
            wait.activeSkill = () => SkillDefOf.Crafting;
            wait.FailOnDestroyedOrNull(TargetIndex.A);
            wait.FailOnDestroyedOrNull(TargetIndex.B);
            wait.AddFailCondition(() => !IsStillDesignated(item));
            // 干活期间让小人轻微晃动（1.7 原版工作台本身没有小人动画）。
            wait.AddPreInitAction(StartWorkAnimation);
            wait.AddFinishAction(StopWorkAnimation);
            yield return wait;

            yield return Toils_General.DoAtomic(FinishRepair);
        }

        /** 开始干活时给小人挂上晃动动画（Defs/AnimationDefs/RepairBench_Animation.xml）。 */
        private void StartWorkAnimation()
        {
            AnimationDef animation = RepairBenchDefOf.RepairBench_WorkAnimation;
            if (animation != null && pawn.Spawned)
            {
                pawn.Drawer.renderer.SetAnimation(animation);
            }
        }

        /**
         * 收工时清掉动画。只清“自己的”那个动画：万一这段时间里游戏或其它 mod 换了别的动画
         * （比如 Anomaly 的变异体动画），不要顺手把它也清掉。
         */
        private void StopWorkAnimation()
        {
            if (pawn.Drawer == null || pawn.Drawer.renderer == null)
            {
                return;
            }
            AnimationDef animation = RepairBenchDefOf.RepairBench_WorkAnimation;
            if (animation != null && pawn.Drawer.renderer.CurAnimation == animation)
            {
                pawn.Drawer.renderer.SetAnimation(null);
            }
        }

        /**
         * 台面上离小人最近的格子；都占着就返回 Invalid（调用方退回落地面）。
         * 工作台每格只允许 1 个物品（BuildingProperties.maxItemsInCell 默认 1），所以 3x1 的台最多摆 3 件。
         */
        private IntVec3 FindFreeBenchCell()
        {
            Thing bench = Bench;
            Map map = pawn.Map;
            if (bench == null || map == null)
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
                    bestDist = dist;
                    best = cell;
                }
            }
            return best;
        }

        /** 修好后把装备从台面挪到台旁地面，避免占着工作台格子挡住下一件。 */
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
            IntVec3 dropCell = bench.InteractionCell.IsValid ? bench.InteractionCell : pawn.Position;
            item.DeSpawn();
            if (!GenPlace.TryPlaceThing(item, dropCell, map, ThingPlaceMode.Near, out _))
            {
                GenSpawn.Spawn(item, dropCell, map);
            }
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
            MoveItemOffBench(item, bench);
            // 收尾音：一声金属扣合，提示「这件修好了」。
            if (RepairBenchDefOf.RepairBench_RepairDone != null)
            {
                RepairBenchDefOf.RepairBench_RepairDone.PlayOneShot(new TargetInfo(bench.Position, pawn.Map));
            }
        }
    }
}
