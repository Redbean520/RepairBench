// Repair Bench - metadata checker.
// Reads the built assembly as metadata only (no runtime load), so it works even though
// the mod targets net10.0 and the game ships its own Assembly-CSharp.
// Used by tools/verify.ps1 contract C12.
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

string dllPath = args.Length > 0 ? args[0] : Path.Combine("1.7", "Assemblies", "RepairBench.dll");
if (!File.Exists(dllPath))
{
    Console.WriteLine("FAIL C12 - missing assembly " + dllPath);
    return 1;
}

using FileStream stream = File.OpenRead(dllPath);
using PEReader pe = new PEReader(stream);
MetadataReader md = pe.GetMetadataReader();

var expected = new (string Type, string Base)[]
{
    ("RepairBench.WorkGiver_RepairDesignated", "RimWorld.WorkGiver_Scanner"),
    ("RepairBench.JobDriver_RepairItem", "Verse.AI.JobDriver"),
    ("RepairBench.Designator_RepairItem", "Verse.Designator"),
    ("RepairBench.RepairBenchProgress", "Verse.MapComponent"),
    ("RepairBench.Alert_RepairBenchPending", "RimWorld.Alert"),
    ("RepairBench.RepairBenchExtension", "Verse.DefModExtension"),
};

var byFullName = new Dictionary<string, TypeDefinitionHandle>();
foreach (TypeDefinitionHandle handle in md.TypeDefinitions)
{
    TypeDefinition td = md.GetTypeDefinition(handle);
    string ns = md.GetString(td.Namespace);
    string name = md.GetString(td.Name);
    string full = string.IsNullOrEmpty(ns) ? name : ns + "." + name;
    byFullName[full] = handle;
}

static string? BaseName(MetadataReader md, EntityHandle handle)
{
    if (handle.IsNil)
    {
        return null;
    }
    switch (handle.Kind)
    {
        case HandleKind.TypeReference:
            TypeReference tr = md.GetTypeReference((TypeReferenceHandle)handle);
            string ns = md.GetString(tr.Namespace);
            string name = md.GetString(tr.Name);
            return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
        case HandleKind.TypeDefinition:
            TypeDefinition td = md.GetTypeDefinition((TypeDefinitionHandle)handle);
            return md.GetString(td.Name);
        default:
            return null;
    }
}

static int CtorCount(MetadataReader md, TypeDefinition td)
{
    int count = 0;
    foreach (MethodDefinitionHandle mh in td.GetMethods())
    {
        MethodDefinition method = md.GetMethodDefinition(mh);
        if (md.GetString(method.Name) == ".ctor")
        {
            count += method.GetParameters().Count;
        }
    }
    return count;
}

var failures = new List<string>();
foreach ((string typeName, string baseName) in expected)
{
    if (!byFullName.TryGetValue(typeName, out TypeDefinitionHandle handle))
    {
        failures.Add("missing type " + typeName);
        continue;
    }
    TypeDefinition td = md.GetTypeDefinition(handle);
    string? actualBase = BaseName(md, td.BaseType);
    if (actualBase != baseName)
    {
        failures.Add(typeName + " base is " + (actualBase ?? "<null>") + ", expected " + baseName);
    }
    if ((td.Attributes & TypeAttributes.Abstract) != 0)
    {
        failures.Add(typeName + " is abstract");
    }
}

// Alert and MapComponent are instantiated by the game through reflection: they need
// a public parameterless ctor (Alert) / a public ctor taking Map (MapComponent).
int alertCtorParams = byFullName.TryGetValue("RepairBench.Alert_RepairBenchPending", out TypeDefinitionHandle ah)
    ? CtorCount(md, md.GetTypeDefinition(ah)) : -1;
int progressCtorParams = byFullName.TryGetValue("RepairBench.RepairBenchProgress", out TypeDefinitionHandle ph)
    ? CtorCount(md, md.GetTypeDefinition(ph)) : -1;
if (alertCtorParams != 0)
{
    failures.Add("Alert_RepairBenchPending needs a public parameterless ctor (found " + alertCtorParams + " ctor params)");
}
if (progressCtorParams != 1)
{
    failures.Add("RepairBenchProgress needs a public ctor(Map) (found " + progressCtorParams + " ctor params)");
}

if (failures.Count > 0)
{
    Console.WriteLine("FAIL C12 - " + string.Join(" | ", failures));
    return 1;
}
Console.WriteLine("PASS C12");
return 0;
