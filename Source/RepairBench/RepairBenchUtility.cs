using System.Collections.Generic;
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

        /// <summary>
        /// Work time is continuous in missing hit points: half-damaged gear takes half the time of
        /// wrecked gear. Repair kits still come in integer steps (see KitsFor), but the work no
        /// longer snaps to kit steps, so a scratch is a short job and a wreck is a long one.
        /// </summary>
        public static int TicksFor(Thing item, RepairBenchExtension ext, float speed)
        {
            if (ext == null)
            {
                ext = Extension;
            }
            if (item == null || ext == null || item.MaxHitPoints <= 0)
            {
                return 60;
            }
            float missing = Mathf.Clamp01(1f - (float)item.HitPoints / item.MaxHitPoints);
            float ticks = ext.ticksForFullRepair * missing / Mathf.Max(0.1f, speed);
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

        /// <summary>
        /// Builds the haul-to-bench-and-repair job for one item, or returns null and reports why.
        /// Shared by the work giver (automatic scanning) and by the right-click "force repair"
        /// options, so both use exactly the same rules for designation, reservations, repair kits
        /// and bench choice. preferredBench is the bench the player right-clicked, if any.
        /// </summary>
        public static Job TryMakeRepairJob(Pawn pawn, Thing item, Thing preferredBench, bool requireDesignation, bool forced, out string failReason)
        {
            failReason = null;
            if (pawn == null || pawn.Map == null || !IsValidItem(item) || item.Map != pawn.Map)
            {
                return null;
            }
            if (requireDesignation &&
                pawn.Map.designationManager.DesignationOn(item, RepairBenchDefOf.RepairBench_RepairDesignation) == null)
            {
                return null;
            }
            if (!pawn.CanReserve(item, 1, -1, null, forced))
            {
                return null;
            }
            RepairBenchExtension ext = Extension;
            if (ext == null)
            {
                return null;
            }
            int kits = KitsFor(item, ext);
            Thing bench;
            if (preferredBench != null)
            {
                if (!preferredBench.Spawned || !CanUseBench(preferredBench, kits) ||
                    preferredBench.IsForbidden(pawn) ||
                    !pawn.CanReach(preferredBench, PathEndMode.InteractionCell, Danger.Deadly))
                {
                    failReason = "RepairBench_NoUsableBench".Translate();
                    return null;
                }
                bench = preferredBench;
            }
            else
            {
                bench = FindUsableBench(pawn, kits);
                if (bench == null)
                {
                    failReason = "RepairBench_NoUsableBench".Translate();
                    return null;
                }
            }
            if (!pawn.CanReserve(bench, 1, -1, null, forced))
            {
                failReason = "RepairBench_NoUsableBench".Translate();
                return null;
            }
            Job job = JobMaker.MakeJob(RepairBenchDefOf.RepairBench_RepairItem, item, bench);
            job.count = kits;
            return job;
        }

        /// <summary>True when this pawn is currently wearing / holding the given gear.</summary>
        public static bool IsWornOrEquippedBy(Pawn pawn, Thing gear)
        {
            if (pawn == null || gear == null)
            {
                return false;
            }
            if (gear is Apparel apparel)
            {
                return pawn.apparel != null && pawn.apparel.WornApparel.Contains(apparel);
            }
            if (gear is ThingWithComps equipment)
            {
                return pawn.equipment != null && pawn.equipment.Contains(equipment);
            }
            return false;
        }

        /// <summary>
        /// Damaged weapons / apparel the pawn itself is wearing or holding and is allowed to take off.
        /// Locked apparel (quest locked, styling station) is skipped, same as vanilla does.
        /// </summary>
        public static List<Thing> GetDamagedWornGear(Pawn pawn)
        {
            List<Thing> result = new List<Thing>();
            if (pawn == null)
            {
                return result;
            }
            if (pawn.apparel != null)
            {
                foreach (Apparel apparel in pawn.apparel.WornApparel)
                {
                    if (IsGear(apparel) && apparel.HitPoints < apparel.MaxHitPoints &&
                        !pawn.apparel.IsLocked(apparel))
                    {
                        result.Add(apparel);
                    }
                }
            }
            if (pawn.equipment != null)
            {
                foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading)
                {
                    if (IsGear(equipment) && equipment.HitPoints < equipment.MaxHitPoints)
                    {
                        result.Add(equipment);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Builds the "pawn takes its own gear off, repairs it on this bench, puts it back on" job,
        /// or returns null and reports why (shown as the disabled float-menu reason).
        /// </summary>
        public static Job TryMakeWornRepairJob(Pawn pawn, Thing gear, Thing bench, out string failReason)
        {
            failReason = null;
            if (pawn == null || pawn.Map == null || gear == null || gear.Destroyed)
            {
                return null;
            }
            if (!IsWornOrEquippedBy(pawn, gear))
            {
                failReason = "FloatMenuRepairOwnNotWorn".Translate();
                return null;
            }
            if (!IsGear(gear) || gear.HitPoints >= gear.MaxHitPoints)
            {
                failReason = "FloatMenuRepairOwnCannot".Translate();
                return null;
            }
            if (gear is Apparel lockedApparel && pawn.apparel != null && pawn.apparel.IsLocked(lockedApparel))
            {
                failReason = "FloatMenuRepairOwnCannot".Translate();
                return null;
            }
            if (bench == null || !bench.Spawned || bench.def != RepairBenchDefOf.RepairBench_Bench)
            {
                failReason = "RepairBench_NoUsableBench".Translate();
                return null;
            }
            RepairBenchExtension ext = Extension;
            if (ext == null)
            {
                return null;
            }
            int kits = KitsFor(gear, ext);
            if (!CanUseBench(bench, kits) || bench.IsForbidden(pawn) ||
                !pawn.CanReach(bench, PathEndMode.InteractionCell, Danger.Deadly))
            {
                failReason = "RepairBench_NoUsableBench".Translate();
                return null;
            }
            if (!pawn.CanReserve(gear, 1, -1, null, true))
            {
                failReason = "FloatMenuRepairOwnCannot".Translate();
                return null;
            }
            if (!pawn.CanReserve(bench, 1, -1, null, true))
            {
                failReason = "RepairBench_NoUsableBench".Translate();
                return null;
            }
            Job job = JobMaker.MakeJob(RepairBenchDefOf.RepairBench_RepairWornItem, gear, bench);
            job.count = kits;
            return job;
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
