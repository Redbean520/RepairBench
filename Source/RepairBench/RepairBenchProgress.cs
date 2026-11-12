using System.Collections.Generic;
using Verse;

namespace RepairBench
{
    /// <summary>
    /// Per-map record of repair work already done on a given piece of gear, so an
    /// interrupted job can resume where it stopped instead of restarting from zero.
    /// Auto-instanced by Map.FillComponents (no Def required).
    /// </summary>
    public class RepairBenchProgress : MapComponent
    {
        public class Entry : IExposable
        {
            public Thing thing;
            public int ticksDone;

            public void ExposeData()
            {
                Scribe_References.Look(ref thing, "thing");
                Scribe_Values.Look(ref ticksDone, "ticksDone", 0);
            }
        }

        private List<Entry> entries = new List<Entry>();

        public RepairBenchProgress(Map map) : base(map)
        {
        }

        public int Get(Thing thing)
        {
            if (thing == null || entries == null)
            {
                return 0;
            }
            Entry entry = entries.Find(e => e != null && e.thing == thing);
            return entry?.ticksDone ?? 0;
        }

        public void Add(Thing thing, int ticks)
        {
            if (thing == null || ticks <= 0 || entries == null)
            {
                return;
            }
            Entry entry = entries.Find(e => e != null && e.thing == thing);
            if (entry == null)
            {
                entry = new Entry { thing = thing };
                entries.Add(entry);
            }
            entry.ticksDone += ticks;
        }

        public void Clear(Thing thing)
        {
            if (thing == null || entries == null)
            {
                return;
            }
            entries.RemoveAll(e => e == null || e.thing == thing);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                Prune();
            }
            Scribe_Collections.Look(ref entries, "entries", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Prune();
            }
        }

        public override void MapComponentTick()
        {
            if (entries == null || entries.Count == 0)
            {
                return;
            }
            if (Find.TickManager == null || Find.TickManager.TicksGame % 120 != 0)
            {
                return;
            }
            Prune();
        }

        private void Prune()
        {
            if (entries == null)
            {
                entries = new List<Entry>();
                return;
            }
            if (map == null)
            {
                return;
            }
            // Only drop entries whose item is gone. Worn-gear repairs have no ground designation,
            // so keying on the designation would throw their progress away between work sessions.
            entries.RemoveAll(e => e == null || e.thing == null || e.thing.Destroyed);
        }
    }
}
