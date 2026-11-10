using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RepairBench
{
    /// <summary>
    /// Single source of truth for the repair numbers and for finding a usable bench.
    /// </summary>
    public static class RepairBenchUtility
    {
        public static RepairBenchExtension Extension
        {
            get
            {
                ThingDef benchDef = RepairBenchDefOf.RepairBench_Bench;
                return benchDef?.GetModExtension<RepairBenchExtension>();
            }
        }

        /// <summary>
        /// Weapon or apparel that can hold hit points and is a single item, regardless of where it
        /// is (ground, held, worn) or how damaged it is. Shared by the designated-item checks and
        /// by the right-click command, which also wants to react to worn gear.
        /// </summary>
        public static bool IsGear(Thing t)
        {
            if (t == null || t.def == null)
            {
                return false;
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
            return t.stackCount == 1;
        }

        public static bool IsValidItem(Thing t)
        {
            if (!IsGear(t))
            {
                return false;
            }
            if (!t.Spawned)
            {
                return false;
            }
            return t.HitPoints < t.MaxHitPoints;
        }

        public static int KitsFor(Thing item, RepairBenchExtension ext)
        {
            if (item == null || item.MaxHitPoints <= 0)
            {
                return 1;
            }
            if (ext == null)
            {
                ext = Extension;
            }
            if (ext == null)
            {
                return 1;
            }
            float fraction = 1f - (float)item.HitPoints / item.MaxHitPoints;
            if (fraction <= 0f)
            {
                return 1;
            }
            return Mathf.Max(1, Mathf.CeilToInt(fraction * ext.kitsForFullRepair));
        }

        public static int TicksFor(Thing item, RepairBenchExtension ext, float speed)
        {
            if (ext == null)
            {
                ext = Extension;
            }
            if (item == null || ext == null)
            {
                return 60;
            }
            int kits = KitsFor(item, ext);
            float ratio = (float)kits / Mathf.Max(1, ext.kitsForFullRepair);
            float ticks = ext.ticksForFullRepair * ratio / Mathf.Max(0.1f, speed);
            return Mathf.Max(60, Mathf.RoundToInt(ticks));
        }

        public static float WorkSpeedFor(Pawn pawn)
        {
            if (pawn == null)
            {
                return 1f;
            }
            float speed = pawn.GetStatValue(StatDefOf.GeneralLaborSpeed);
            return Mathf.Max(0.1f, speed);
        }

        public static bool CanUseBench(Thing bench, int kits)
        {
            if (bench == null || !bench.Spawned || bench.IsBurning())
            {
                return false;
            }
            CompRefuelable comp = bench.TryGetComp<CompRefuelable>();
            return comp != null && comp.Fuel >= kits;
        }

        public static Thing FindUsableBench(Pawn pawn, int kits)
        {
            if (pawn == null || pawn.Map == null)
            {
                return null;
            }
            return GenClosest.ClosestThingReachable(
                pawn.Position, pawn.Map,
                ThingRequest.ForDef(RepairBenchDefOf.RepairBench_Bench),
                PathEndMode.InteractionCell,
                TraverseParms.For(pawn),
                9999f,
                b => CanUseBench(b, kits) && !b.IsForbidden(pawn) && pawn.CanReach(b, PathEndMode.InteractionCell, Danger.Deadly));
        }

        public static Thing FindUsableBench(Map map, int kits, bool requirePawnChecks, Pawn pawn = null)
        {
            if (map == null)
            {
                return null;
            }
            if (requirePawnChecks && pawn != null)
            {
                return FindUsableBench(pawn, kits);
            }
            foreach (Thing bench in map.listerThings.ThingsOfDef(RepairBenchDefOf.RepairBench_Bench))
            {
                if (CanUseBench(bench, kits))
                {
                    return bench;
                }
            }
            return null;
        }
    }
}
