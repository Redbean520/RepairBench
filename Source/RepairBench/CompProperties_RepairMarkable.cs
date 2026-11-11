using Verse;

namespace RepairBench
{
    /// <summary>
    /// 标记用 comp 的属性。没有额外字段，只负责把 compClass 指向 CompRepairMarkable，
    /// 这样 Patches/RepairBench_ItemGizmo.xml 可以用一行 &lt;li Class="RepairBench.CompProperties_RepairMarkable"/&gt; 挂到原版基类上。
    /// </summary>
    public class CompProperties_RepairMarkable : CompProperties
    {
        public CompProperties_RepairMarkable()
        {
            compClass = typeof(CompRepairMarkable);
        }
    }
}
