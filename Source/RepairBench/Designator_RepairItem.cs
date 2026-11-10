using RimWorld;
using UnityEngine;
using Verse;

namespace RepairBench
{
    public class Designator_RepairItem : Designator
    {
        protected override DesignationDef Designation => RepairBenchDefOf.RepairBench_RepairDesignation;

        // Without a draw style DesignatorManager treats the designator as single-cell-only and never
        // starts a drag, so you cannot sweep a rectangle like vanilla's Harvest/Haul. Same line as
        // Designator_Haul.
        public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.FilledRectangle;

        public override bool Visible => RepairBenchDefOf.RepairBench_RepairTech.IsFinished || DebugSettings.godMode;

        public Designator_RepairItem()
        {
            defaultLabel = "DesignatorRepairItems".Translate();
            defaultDesc = "DesignatorRepairItemsDesc".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Designators/RepairBench_Repair");
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_Haul;
            useMouseIcon = true;
            hasDesignateAllFloatMenuOption = true;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(Map) || c.Fogged(Map))
            {
                return false;
            }
            Thing first = c.GetFirstItem(Map);
            if (first == null)
            {
                return false;
            }
            return CanDesignateThing(first);
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            if (t == null || t.def == null)
            {
                return false;
            }
            if (!t.Spawned)
            {
                return "MessageGearNotOnGround".Translate();
            }
            if (t.def.category != ThingCategory.Item)
            {
                return false;
            }
            if (!t.def.IsWeapon && !t.def.IsApparel)
            {
                return false;
            }
            if (!t.def.useHitPoints || t.MaxHitPoints <= 0)
            {
                return false;
            }
            if (t.stackCount != 1)
            {
                return false;
            }
            if (t.HitPoints >= t.MaxHitPoints)
            {
                return "MessageAlreadyRepaired".Translate();
            }
            if (Map.designationManager.DesignationOn(t, Designation) != null)
            {
                return false;
            }
            return true;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            Thing first = c.GetFirstItem(Map);
            if (first != null)
            {
                DesignateThing(first);
            }
        }

        public override void DesignateThing(Thing t)
        {
            Map.designationManager.AddDesignation(new Designation(t, Designation));
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }
    }
}
