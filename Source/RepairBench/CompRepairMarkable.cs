using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RepairBench
{
    /// <summary>
    /// 给“能被修补的装备”（武器 / 衣物 / 护甲）在命令栏加一个「维修」按钮：
    /// 选中地上带磨损的装备 -> 直接点按钮就等于打上修补标记（和 Orders 菜单里的修补标记同一件事）。
    /// 已带标记时按钮变成「取消维修」。
    ///
    /// 这个 comp 不是写在某个具体 ThingDef 上的，而是由 Patches/RepairBench_ItemGizmo.xml
    /// 追加到原版抽象基类 BaseWeapon / ApparelNoQualityBase 的 comps，于是所有枪械、近战武器、
    /// 手雷、衣物、护甲都会自动带上（补丁在原版继承解析之前生效）。
    /// 不显示的情况：没研究完修补技术、不是装备、平地上已满耐久、装备不在地上（穿在身上 / 拿在手里）。
    /// </summary>
    public class CompRepairMarkable : ThingComp
    {
        private static DesignationDef MarkDef => RepairBenchDefOf.RepairBench_RepairDesignation;

        private static bool ResearchDone =>
            RepairBenchDefOf.RepairBench_RepairTech == null || RepairBenchDefOf.RepairBench_RepairTech.IsFinished;

        private Designation CurrentMark
        {
            get
            {
                Map map = parent.Map;
                if (map == null || MarkDef == null)
                {
                    return null;
                }
                return map.designationManager.DesignationOn(parent, MarkDef);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!parent.Spawned || parent.Map == null || MarkDef == null)
            {
                yield break;
            }
            if (!ResearchDone && !DebugSettings.godMode)
            {
                yield break;
            }
            if (!RepairBenchUtility.IsGear(parent))
            {
                yield break;
            }

            Designation existing = CurrentMark;
            if (existing != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "GizmoCancelRepairItem".Translate(),
                    defaultDesc = "GizmoCancelRepairItemDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/RepairBench_Repair"),
                    action = delegate
                    {
                        Map map = parent.Map;
                        if (map != null)
                        {
                            map.designationManager.RemoveDesignation(existing);
                        }
                    }
                };
                yield break;
            }

            if (parent.HitPoints >= parent.MaxHitPoints)
            {
                yield break;
            }

            yield return new Command_Action
            {
                defaultLabel = "GizmoRepairItem".Translate(),
                defaultDesc = "GizmoRepairItemDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Designators/RepairBench_Repair"),
                action = delegate
                {
                    Map map = parent.Map;
                    if (map == null)
                    {
                        return;
                    }
                    if (map.designationManager.DesignationOn(parent, MarkDef) == null)
                    {
                        map.designationManager.AddDesignation(new Designation(parent, MarkDef));
                    }
                    SoundDefOf.Designate_Haul.PlayOneShotOnCamera(map);
                }
            };
        }
    }
}
