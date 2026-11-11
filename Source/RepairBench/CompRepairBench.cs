using RimWorld;
using Verse;

namespace RepairBench
{
    /**
     * 修补台的「燃料」= 修补剂。继承原版 CompRefuelable 保留装料/倒料/缺料图标，
     * 只在检视面板补一句中文可见的缺料提示：
     * 原版缺料时只有一个小图标 + fuelLabel 行，颜色和中文化都不明显，
     * 玩家容易以为「没提示」。
     */
    public class CompRepairBench : CompRefuelable
    {
        public override string CompInspectStringExtra()
        {
            string text = base.CompInspectStringExtra();
            if (HasFuel)
            {
                return text;
            }
            string warning = "<color=#cf5b4a>" + (string)"RepairBench_NoKitsInBench".Translate() + "</color>";
            return text.NullOrEmpty() ? warning : text + "\n" + warning;
        }
    }
}
