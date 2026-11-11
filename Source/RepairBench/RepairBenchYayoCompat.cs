using System;
using System.Reflection;
using Verse;

namespace RepairBench
{
    /**
     * 与 Yayo's Animation (Continued) 的运行时桥。
     *
     * 期望行为：装了 Yayo 时修补动作走 Yayo 的动画；没装 Yayo 时才用本 mod 自带的
     * AnimationDef（Defs/AnimationDefs/RepairBench_Animation.xml）。
     *
     * 难点：Yayo 只对它自己硬编码的 JobDef 名单做动画（AnimationCore.cs 里 switch(defName)），
     * 名单外的 JobDef 走 default 分支 -> 不产生任何动作。所以「不挂本 mod 动画」并不等于
     * 「使用 Yayo 动画」，而是会让小人站着一动不动。这里通过 Harmony 在运行时给
     * YayoAnimation.AnimationCore.AniStanding 挂一个 prefix：当 defName 是本 mod 的
     * RepairBench_RepairItem 时，把它改写成 Yayo 名单里已有的 "Repair"，
     * 于是 Yayo 会用它对原版修理/制作的那套工作动画来动小人。
     *
     * 设计取舍：
     * - 全部走反射：本 mod 不引用 Yayo / Harmony 程序集，不写 modDependencies；没装 Yayo 时
     *   连 Harmony 类型都不会去碰。
     * - 只改写参数，不改 Yayo 的方法体，也不接管它的其它 JobDef。
     * - 任何一步失败（Yayo 改签名、Harmony 版本变化等）都只打一条 Warning 并回退到本 mod
     *   自己的 AnimationDef，不会让修补流程崩掉。
     */
    [StaticConstructorOnStartup]
    public static class RepairBenchYayoCompat
    {
        private const string JobDefName = "RepairBench_RepairItem";

        private const string YayoStandingMethodName = "AniStanding";

        /** Yayo 名单里与本工作语义最接近的 JobDef（原版修理）。 */
        private const string YayoRepairJobName = "Repair";

        private static bool bridgeInstalled;

        /** true = 没有可用的 Yayo 桥，由本 mod 自己挂 AnimationDef。 */
        public static bool UseOwnAnimation => !bridgeInstalled;

        static RepairBenchYayoCompat()
        {
            try
            {
                bridgeInstalled = TryInstallBridge();
                if (bridgeInstalled)
                {
                    Log.Message("[Repair Bench] Yayo's Animation detected - repair jobs will use its work animation.");
                }
            }
            catch (Exception e)
            {
                bridgeInstalled = false;
                Log.Warning("[Repair Bench] Could not hook Yayo's Animation; the built-in work animation will be used instead. " + e);
            }
        }

        private static bool TryInstallBridge()
        {
            Type yayoCore = GenTypes.GetTypeInAnyAssembly("YayoAnimation.AnimationCore");
            if (yayoCore == null)
            {
                // 没装 Yayo：没有需要桥接的东西。
                return false;
            }
            Type harmonyType = GenTypes.GetTypeInAnyAssembly("HarmonyLib.Harmony");
            Type harmonyMethodType = GenTypes.GetTypeInAnyAssembly("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
            {
                Log.Warning("[Repair Bench] Yayo's Animation is active but Harmony is missing; the built-in work animation will be used.");
                return false;
            }
            MethodInfo target = FindAniStanding(yayoCore);
            if (target == null)
            {
                Log.Warning("[Repair Bench] Yayo's Animation was found but AniStanding(Pawn, ref Rot4, PawnDrawData, string) no longer matches; the built-in work animation will be used.");
                return false;
            }
            MethodInfo prefix = typeof(RepairBenchYayoCompat).GetMethod(nameof(YayoAniStandingPrefix), BindingFlags.Static | BindingFlags.NonPublic);
            if (prefix == null)
            {
                return false;
            }
            MethodInfo patch = FindPatchMethod(harmonyType);
            if (patch == null)
            {
                Log.Warning("[Repair Bench] Harmony has no Patch(MethodBase, HarmonyMethod, HarmonyMethod, HarmonyMethod, HarmonyMethod); the built-in work animation will be used.");
                return false;
            }
            object harmony = Activator.CreateInstance(harmonyType, "redbean.repairbench.yayo");
            object harmonyMethod = Activator.CreateInstance(harmonyMethodType, new object[] { prefix });
            patch.Invoke(harmony, new object[] { target, harmonyMethod, null, null, null });
            return true;
        }

        private static MethodInfo FindAniStanding(Type yayoCore)
        {
            foreach (MethodInfo method in yayoCore.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (method.Name != YayoStandingMethodName)
                {
                    continue;
                }
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 4 && parameters[3].ParameterType == typeof(string) && parameters[3].Name == "defName")
                {
                    return method;
                }
            }
            return null;
        }

        private static MethodInfo FindPatchMethod(Type harmonyType)
        {
            foreach (MethodInfo method in harmonyType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.Name != "Patch")
                {
                    continue;
                }
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 5 && parameters[0].ParameterType == typeof(MethodBase))
                {
                    return method;
                }
            }
            return null;
        }

        /**
         * Harmony prefix：把本 mod 的 JobDef 名改写成 Yayo 认识的 "Repair"。
         * 原方法签名里 defName 是按值传入的 string，Harmony 允许 prefix 用 ref 写回原参数。
         */
        private static void YayoAniStandingPrefix(ref string defName)
        {
            if (defName == JobDefName)
            {
                defName = YayoRepairJobName;
            }
        }
    }
}
