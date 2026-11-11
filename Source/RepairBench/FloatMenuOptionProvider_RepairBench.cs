using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RepairBench
{
    /// <summary>
    /// Right-click options:
    /// - on a damaged weapon / apparel lying on the ground: mark it for repair, or order a colonist
    ///   to go and repair it right now ("force repair", the same idea as vanilla prioritize-work);
    /// - on a repair bench: force repair for every marked item on that map, using that bench.
    /// FloatMenuMakerMap.Init() instantiates every non-abstract FloatMenuOptionProvider subclass by
    /// reflection, so this needs no Def and no Harmony patch.
    /// v1 only repairs items on the ground, so worn/held gear gets a disabled option with a reason
    /// instead of a silently missing command.
    /// </summary>
    public class FloatMenuOptionProvider_RepairBench : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;

        protected override bool Undrafted => true;

        protected override bool Multiselect => false;

        protected override bool RequiresManipulation => true;

        protected override bool CanSelfTarget => true;

        // Gear that is worn or equipped has Spawned == false; let those through so we can show
        // the disabled "put it on the ground first" hint instead of hiding the command.
        public override bool CanTargetDespawned => true;

        protected override bool AppliesInt(FloatMenuContext context)
        {
            return RepairBenchDefOf.RepairBench_RepairTech.IsFinished || DebugSettings.godMode;
        }

        public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
        {
            if (clickedThing == null)
            {
                yield break;
            }
            if (clickedThing.def == RepairBenchDefOf.RepairBench_Bench)
            {
                foreach (FloatMenuOption benchOption in ForceRepairOptionsForBench(clickedThing, context))
                {
                    yield return benchOption;
                }
                yield break;
            }
            if (!RepairBenchUtility.IsGear(clickedThing))
            {
                yield break;
            }
            if (clickedThing.HitPoints >= clickedThing.MaxHitPoints)
            {
                yield break;
            }
            DesignationDef designationDef = RepairBenchDefOf.RepairBench_RepairDesignation;

            if (!clickedThing.Spawned)
            {
                yield return Disabled(
                    "FloatMenuRepairItemWorn".Translate(),
                    "FloatMenuRepairItemWornDesc".Translate());
                yield break;
            }
            Map map = clickedThing.Map;
            if (map == null)
            {
                yield break;
            }
            Thing item = clickedThing;
            Pawn pawn = context.FirstSelectedPawn;
            bool marked = map.designationManager.DesignationOn(item, designationDef) != null;

            // Force repair: skip the work giver scan and hand the job straight to this colonist.
            FloatMenuOption force = ForceRepairOption(item, pawn, null, "FloatMenuForceRepair".Translate(), null);
            if (force != null)
            {
                yield return force;
            }

            if (marked)
            {
                yield return new FloatMenuOption(
                    "GizmoCancelRepairItem".Translate(),
                    delegate
                    {
                        if (item.Spawned && item.Map != null)
                        {
                            item.Map.designationManager.TryRemoveDesignationOn(item, designationDef);
                        }
                    })
                {
                    tooltip = new TipSignal("GizmoCancelRepairItemDesc".Translate())
                };
                yield break;
            }

            int kits = RepairBenchUtility.KitsFor(item, null);
            yield return new FloatMenuOption(
                "FloatMenuRepairItem".Translate(),
                delegate
                {
                    if (!item.Spawned || item.Map == null)
                    {
                        return;
                    }
                    item.Map.designationManager.AddDesignation(new Designation(item, designationDef));
                    SoundDefOf.Designate_Haul.PlayOneShotOnCamera(item.Map);
                })
            {
                tooltip = new TipSignal("FloatMenuRepairItemDesc".Translate(kits))
            };
        }

        private IEnumerable<FloatMenuOption> ForceRepairOptionsForBench(Thing bench, FloatMenuContext context)
        {
            Map map = bench?.Map;
            Pawn pawn = context?.FirstSelectedPawn;
            if (map == null || pawn == null)
            {
                yield break;
            }
            DesignationDef designationDef = RepairBenchDefOf.RepairBench_RepairDesignation;
            bool any = false;
            foreach (Designation designation in map.designationManager.SpawnedDesignationsOfDef(designationDef))
            {
                Thing item = designation.target.Thing;
                if (item == null || item == bench || !RepairBenchUtility.IsValidItem(item))
                {
                    continue;
                }
                any = true;
                string label = "FloatMenuForceRepairItem".Translate(item.LabelShort);
                FloatMenuOption option = ForceRepairOption(item, pawn, bench, label, bench);
                if (option != null)
                {
                    yield return option;
                }
            }
            if (!any)
            {
                yield break;
            }
        }

        /// <summary>
        /// One "force repair" entry. preferredBench is the bench the player clicked, if any; when it
        /// is null the colonist picks the nearest usable bench. Returns a disabled entry with the
        /// reason when the job cannot be made right now.
        /// </summary>
        private static FloatMenuOption ForceRepairOption(Thing item, Pawn pawn, Thing preferredBench, string label, Thing revalidateClickTarget)
        {
            if (pawn == null)
            {
                return null;
            }
            if (!CanPawnDoRepairWork(pawn))
            {
                return Disabled(label, "FloatMenuForceRepairCannot".Translate());
            }
            DesignationDef designationDef = RepairBenchDefOf.RepairBench_RepairDesignation;
            bool marked = item.Spawned && item.Map != null &&
                          item.Map.designationManager.DesignationOn(item, designationDef) != null;
            Job job = RepairBenchUtility.TryMakeRepairJob(pawn, item, preferredBench, marked, true, out string failReason);
            if (job == null)
            {
                string reason = failReason ?? (string)"FloatMenuForceRepairCannot".Translate();
                return Disabled(label, reason);
            }
            int kits = job.count;
            return new FloatMenuOption(
                label,
                delegate
                {
                    if (!item.Spawned || item.Map == null)
                    {
                        return;
                    }
                    if (item.Map.designationManager.DesignationOn(item, designationDef) == null)
                    {
                        item.Map.designationManager.AddDesignation(new Designation(item, designationDef));
                    }
                    // Rebuild at click time: reservations may have changed while the menu was open.
                    Job ordered = RepairBenchUtility.TryMakeRepairJob(pawn, item, preferredBench, true, true, out string _);
                    if (ordered != null)
                    {
                        pawn.jobs.TryTakeOrderedJobPrioritizedWork(
                            ordered,
                            RepairBenchDefOf.RepairBench_RepairDesignated.Worker,
                            item.Position);
                    }
                },
                MenuOptionPriority.Default,
                null,
                revalidateClickTarget)
            {
                tooltip = new TipSignal("FloatMenuForceRepairDesc".Translate(kits))
            };
        }

        private static bool CanPawnDoRepairWork(Pawn pawn)
        {
            if (pawn == null || pawn.Drafted || pawn.Map == null)
            {
                return false;
            }
            WorkTypeDef workType = RepairBenchDefOf.RepairBench_Repairing;
            if (workType == null || pawn.WorkTypeIsDisabled(workType))
            {
                return false;
            }
            return pawn.workSettings != null && pawn.workSettings.GetPriority(workType) > 0;
        }

        private static FloatMenuOption Disabled(string label, string tooltip)
        {
            return new FloatMenuOption(label, null)
            {
                tooltip = new TipSignal(tooltip)
            };
        }
    }
}
