using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RepairBench
{
    /// <summary>
    /// Right-click options:
    /// - on a damaged weapon / apparel lying on the ground: mark it for repair, or order a colonist
    ///   to go and repair it right now ("force repair", the same idea as vanilla prioritize-work);
    /// - on a repair bench: repair the selected pawn's own worn / equipped gear right there (the pawn
    ///   takes it off, repairs it on this bench and puts it back on), and force repair for every marked
    ///   item on that map using that bench.
    /// FloatMenuMakerMap.Init() instantiates every non-abstract FloatMenuOptionProvider subclass by
    /// reflection, so this needs no Def and no Harmony patch.
    /// Ground items are marked / hauled as before; worn gear is only repairable from the bench menu,
    /// because a worn item cannot be designated.
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
            // A clicked pawn shows up in both ClickedThings and ClickedPawns; pawn menus are not our
            // business here (own-gear repair is offered on the bench instead).
            if (clickedThing is Pawn)
            {
                yield break;
            }
            if (clickedThing.def == RepairBenchDefOf.RepairBench_Bench)
            {
                foreach (FloatMenuOption ownGearOption in OwnGearOptionsForBench(clickedThing, context))
                {
                    yield return ownGearOption;
                }
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

        /// <summary>
        /// "Repair &lt;item&gt;" entries for the selected pawn's own damaged gear, shown when the player
        /// right-clicks a repair bench: the pawn walks there, takes the gear off itself, repairs it on
        /// the bench, then puts it back on. Worn gear has no ground designation, so this menu is the
        /// only entry point for it.
        /// </summary>
        private static IEnumerable<FloatMenuOption> OwnGearOptionsForBench(Thing bench, FloatMenuContext context)
        {
            Pawn pawn = context?.FirstSelectedPawn;
            if (pawn == null)
            {
                yield break;
            }
            foreach (Thing gear in RepairBenchUtility.GetDamagedWornGear(pawn))
            {
                int kits = RepairBenchUtility.KitsFor(gear, null);
                int hp = gear.HitPoints;
                int maxHp = Mathf.Max(1, gear.MaxHitPoints);
                int seconds = Mathf.Max(1, Mathf.RoundToInt(
                    RepairBenchUtility.TicksFor(gear, null, RepairBenchUtility.WorkSpeedFor(pawn)) / 60f));
                string label = "FloatMenuRepairOwnItem".Translate(gear.LabelShort);
                string tooltip = "FloatMenuRepairOwnItemDesc".Translate(hp, maxHp, Mathf.RoundToInt(100f * hp / maxHp), kits, seconds);
                if (!CanPawnDoRepairWork(pawn))
                {
                    yield return Disabled(label, "FloatMenuForceRepairCannot".Translate());
                    continue;
                }
                Job job = RepairBenchUtility.TryMakeWornRepairJob(pawn, gear, bench, out string failReason);
                if (job == null)
                {
                    yield return Disabled(label, failReason ?? (string)"FloatMenuRepairOwnCannot".Translate());
                    continue;
                }
                Thing gearLocal = gear;
                yield return new FloatMenuOption(
                    label,
                    delegate
                    {
                        // Rebuild at click time: the player may have changed gear while the menu was open.
                        Job ordered = RepairBenchUtility.TryMakeWornRepairJob(pawn, gearLocal, bench, out string reason);
                        if (ordered == null)
                        {
                            Messages.Message(reason ?? (string)"FloatMenuRepairOwnCannot".Translate(), MessageTypeDefOf.RejectInput, false);
                            return;
                        }
                        pawn.jobs.TryTakeOrderedJobPrioritizedWork(
                            ordered,
                            RepairBenchDefOf.RepairBench_RepairDesignated.Worker,
                            bench.Position);
                    },
                    MenuOptionPriority.Default,
                    null,
                    bench)
                {
                    tooltip = new TipSignal(tooltip)
                };
            }
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
