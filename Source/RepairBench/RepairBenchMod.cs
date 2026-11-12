using Verse;

namespace RepairBench
{
    /// <summary>
    /// Mod entry point. Holds the version constant so the version stays synced in three places
    /// (this file, the csproj <Version>, About.xml <modVersion>).
    /// </summary>
    public class RepairBenchMod : Mod
    {
        public const string Version = "1.0.1";

        public RepairBenchMod(ModContentPack content) : base(content)
        {
        }
    }
}
