using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;

namespace RepairBench
{
    /// <summary>
    /// Right-click command: marks a damaged weapon or apparel lying on the ground for repair.
    /// FloatMenuMakerMap.Init() instantiates every non-abstract FloatMenuOptionProvider subclass
    /// by reflection, so this needs no Def and no Harmony patch.
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
            if (map.designationManager.DesignationOn(clickedThing, designationDef) != null)
            {
                yield return Disabled(
                    "FloatMenuRepairItemMarked".Translate(),
                    "FloatMenuRepairItemMarkedDesc".Translate());
                yield break;
            }

            int kits = RepairBenchUtility.KitsFor(clickedThing, null);
            Thing item = clickedThing;
            FloatMenuOption option = new FloatMenuOption(
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
            yield return option;
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
