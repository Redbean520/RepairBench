<#
  Static self-check for the Repair Bench mod. Read-only: never starts the game, never writes.
  Contracts are appended task by task. Usage:
    powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify.ps1 -Strict
  Exit code 0 = all contracts passed, 1 = at least one FAIL (only enforced with -Strict).
#>
param(
    [string]$ModRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$CoreDir = "D:\SteamLibrary\steamapps\common\RimWorld\Data\Core",
    [switch]$Strict
)

$ErrorActionPreference = "Stop"
$script:failures = 0
$script:warnings = 0

function Pass([string]$id) {
    Write-Host "PASS $id" -ForegroundColor Green
}
function Fail([string]$id, [string]$msg) {
    $script:failures++
    Write-Host "FAIL $id - $msg" -ForegroundColor Red
}
function Warn([string]$id, [string]$msg) {
    $script:warnings++
    Write-Host "WARN $id - $msg" -ForegroundColor Yellow
}
function LoadXml([string]$path) {
    $doc = New-Object System.Xml.XmlDocument
    $doc.Load($path)
    return $doc
}
function Get-DocRoot([string]$path) {
    if (-not (Test-Path -LiteralPath $path)) { return $null }
    try { return LoadXml $path } catch { return $null }
}
function Select-Text([System.Xml.XmlNode]$node, [string]$tag) {
    if ($null -eq $node) { return $null }
    $n = $node.SelectSingleNode($tag)
    if ($null -eq $n) { return $null }
    return $n.InnerText.Trim()
}

$modRoot = (Resolve-Path -LiteralPath $ModRoot).Path

# ---------------------------------------------------------------- C1 About.xml
try {
    $aboutPath = Join-Path $modRoot "About\About.xml"
    if (-not (Test-Path -LiteralPath $aboutPath)) { Fail "C1" "missing About/About.xml" }
    else {
        $doc = LoadXml $aboutPath
        $md = $doc.DocumentElement
        # NOTE: PowerShell 5.1's XML adapter shadows .Name with child-element lookup, so use LocalName.
        if ($md.LocalName -ne "ModMetaData") { Fail "C1" "root is $($md.LocalName), expected ModMetaData" }
        else {
            $packageId = Select-Text $md "packageId"
            $name = Select-Text $md "name"
            $ver = Select-Text $md "modVersion"
            $versions = @($md.SelectNodes("supportedVersions/li") | ForEach-Object { $_.InnerText.Trim() })
            if ($packageId -ne "redbean.repairbench") { Fail "C1" "packageId is '$packageId'" }
            elseif ($name -ne "Repair Bench") { Fail "C1" "name is '$name', expected 'Repair Bench'" }
            elseif (-not $ver) { Fail "C1" "modVersion missing" }
            elseif ($versions -notcontains "1.7") { Fail "C1" "supportedVersions lacks 1.7" }
            else { Pass "C1" }
        }
    }
} catch { Fail "C1" $_.Exception.Message }

# ---------------------------------------------------------------- C2 LoadFolders.xml
try {
    $lfPath = Join-Path $modRoot "LoadFolders.xml"
    if (-not (Test-Path -LiteralPath $lfPath)) { Fail "C2" "missing LoadFolders.xml" }
    else {
        $doc = LoadXml $lfPath
        if ($null -eq $doc.SelectSingleNode("//v1.7")) { Fail "C2" "no <v1.7> node" }
        else { Pass "C2" }
    }
} catch { Fail "C2" $_.Exception.Message }

# ---------------------------------------------------------------- C3 XML well-formedness
try {
    $bad = @()
    $count = 0
    foreach ($dir in @("Defs", "Patches", "Languages")) {
        $full = Join-Path $modRoot $dir
        if (-not (Test-Path -LiteralPath $full)) { continue }
        foreach ($f in Get-ChildItem -LiteralPath $full -Recurse -File -Filter "*.xml") {
            $count++
            try { LoadXml $f.FullName | Out-Null } catch { $bad += "$($f.FullName): $($_.Exception.Message)" }
        }
    }
    if ($bad.Count -gt 0) { Fail "C3" ($bad -join " | ") }
    elseif ($count -eq 0) { Warn "C3" "no XML files under Defs/Patches/Languages yet" ; Pass "C3" }
    else { Pass "C3" }
} catch { Fail "C3" $_.Exception.Message }

# ---------------------------------------------------------------- helpers for C4+
function Get-ModXml([string]$relPath) {
    $p = Join-Path $modRoot $relPath
    if (-not (Test-Path -LiteralPath $p)) { return $null }
    try { return LoadXml $p } catch { return $null }
}
function Get-DefNode([string]$relPath, [string]$defName) {
    $doc = Get-ModXml $relPath
    if ($null -eq $doc) { return $null }
    $node = $doc.SelectSingleNode("//*[defName='" + $defName + "']")
    return $node
}
function Get-CoreDefNames() {
    $set = @{}
    if (-not (Test-Path -LiteralPath $CoreDir)) { return $null }
    $roots = @($CoreDir)
    $dataRoot = Split-Path -Parent $CoreDir
    if (Test-Path -LiteralPath $dataRoot) {
        foreach ($d in Get-ChildItem -LiteralPath $dataRoot -Directory) { $roots += $d.FullName }
    }
    foreach ($root in $roots) {
        foreach ($f in Get-ChildItem -LiteralPath $root -Recurse -File -Filter "*.xml" -ErrorAction SilentlyContinue) {
            try {
                $doc = LoadXml $f.FullName
                foreach ($n in $doc.SelectNodes("//defName")) {
                    $t = $n.InnerText.Trim()
                    if ($t) { $set[$t] = $true }
                }
            } catch { }
        }
    }
    return $set
}
function Get-ModDefNames() {
    $set = @{}
    $defsDir = Join-Path $modRoot "Defs"
    if (-not (Test-Path -LiteralPath $defsDir)) { return $set }
    foreach ($f in Get-ChildItem -LiteralPath $defsDir -Recurse -File -Filter "*.xml") {
        try {
            $doc = LoadXml $f.FullName
            foreach ($n in $doc.SelectNodes("//defName")) {
                $t = $n.InnerText.Trim()
                if ($t) { $set[$t] = $true }
            }
        } catch { }
    }
    return $set
}

# ---------------------------------------------------------------- C4 bench def
try {
    $node = Get-DefNode "Defs\ThingDefs_Buildings\RepairBench_Bench.xml" "RepairBench_Bench"
    if ($null -eq $node) { Fail "C4" "RepairBench_Bench not found" }
    else {
        $thingClass = Select-Text $node "thingClass"
        $size = Select-Text $node "size"
        $sizeMatch = [regex]::Match($size, "^\((\d+),(\d+)\)$")
        $sizeX = if ($sizeMatch.Success) { $sizeMatch.Groups[1].Value } else { "" }
        $sizeZ = if ($sizeMatch.Success) { $sizeMatch.Groups[2].Value } else { "" }
        $interaction = Select-Text $node "hasInteractionCell"
        $fuel = $node.SelectSingleNode("comps/li[@Class='CompProperties_Refuelable']")
        $cap = if ($fuel) { Select-Text $fuel "fuelCapacity" } else { $null }
        $fac = $node.SelectSingleNode("comps/li[@Class='CompProperties_AffectedByFacilities']")
        $ext = $node.SelectSingleNode("modExtensions/li[@Class='RepairBench.RepairBenchExtension']")
        $ticks = if ($ext) { Select-Text $ext "ticksForFullRepair" } else { $null }
        $kits = if ($ext) { Select-Text $ext "kitsForFullRepair" } else { $null }
        if ($thingClass -ne "Building") { Fail "C4" "thingClass is '$thingClass', expected Building (plain building: no bill tab)" }
        elseif ($sizeX -ne "3" -or $sizeZ -ne "1") { Fail "C4" ("size is " + $sizeX + "x" + $sizeZ + ", expected 3x1") }
        elseif ($interaction -ne "true") { Fail "C4" "hasInteractionCell is '$interaction'" }
        elseif ($cap -ne "20") { Fail "C4" "CompProperties_Refuelable fuelCapacity is '$cap', expected 20" }
        elseif ($null -eq $fac) { Fail "C4" "missing CompProperties_AffectedByFacilities" }
        elseif ($ticks -ne "3000") { Fail "C4" "modExtension ticksForFullRepair is '$ticks', expected 3000" }
        elseif ($kits -ne "10") { Fail "C4" "modExtension kitsForFullRepair is '$kits', expected 10" }
        else { Pass "C4" }
    }
} catch { Fail "C4" $_.Exception.Message }

# ---------------------------------------------------------------- C5 def reference integrity
try {
    if (-not (Test-Path -LiteralPath $CoreDir)) { Warn "C5" "CoreDir not found ($CoreDir), skipped def reference scan" }
    else {
        $known = Get-CoreDefNames
        $modDefs = Get-ModDefNames
        foreach ($k in $modDefs.Keys) { $known[$k] = $true }
        $missing = @()
        $scanXPaths = @(
            "//researchPrerequisites/li",
            "//researchPrerequisite",
            "//recipeUsers/li",
            "//thingDefs/li",
            "//thingCategories/li",
            "//fixedIngredientFilter/thingDefs/li",
            "//ingredients//filter/thingDefs/li",
            "//products/*",
            "//prerequisites/li"
        )
        $defsDir = Join-Path $modRoot "Defs"
        foreach ($f in Get-ChildItem -LiteralPath $defsDir -Recurse -File -Filter "*.xml") {
            $doc = LoadXml $f.FullName
            foreach ($xp in $scanXPaths) {
                foreach ($n in $doc.SelectNodes($xp)) {
                    $name = if ($n.LocalName -eq "li" -or $n.LocalName -eq "researchPrerequisite") { $n.InnerText.Trim() } else { $n.LocalName }
                    if (-not $name) { continue }
                    if (-not $known.ContainsKey($name)) { $missing += "$($f.Name): $name ($xp)" }
                }
            }
        }
        if ($missing.Count -gt 0) { Fail "C5" ("unknown defName refs: " + (($missing | Select-Object -Unique) -join " | ")) }
        else { Pass "C5" }
    }
} catch { Fail "C5" $_.Exception.Message }

# ---------------------------------------------------------------- C6 repair kit def
try {
    $node = Get-DefNode "Defs\ThingDefs_Items\RepairBench_RepairKit.xml" "RepairBench_RepairKit"
    if ($null -eq $node) { Fail "C6" "RepairBench_RepairKit not found" }
    else {
        $stack = Select-Text $node "stackLimit"
        $healthPrice = Select-Text $node "healthAffectsPrice"
        $texPath = Select-Text $node "graphicData/texPath"
        if ($stack -ne "50") { Fail "C6" "stackLimit is '$stack', expected 50" }
        elseif ($healthPrice -ne "false") { Fail "C6" "healthAffectsPrice is '$healthPrice', expected false" }
        else {
            $gc = Select-Text $node "graphicData/graphicClass"
            $texBase = Join-Path $modRoot ("Textures\" + ($texPath -replace "/", "\"))
            if ($gc -match "Graphic_StackCount|Graphic_Collection") {
                # Graphic_Collection wants a folder of sprites, not a single PNG.
                if (-not (Test-Path -LiteralPath $texBase -PathType Container) -or
                    @(Get-ChildItem -LiteralPath $texBase -File -Filter "*.png").Count -eq 0) {
                    Warn "C6" "stack-count texture folder missing for $texPath"
                }
            } elseif (-not (Test-Path -LiteralPath ($texBase + "_south.png")) -and
                      -not (Test-Path -LiteralPath ($texBase + ".png"))) {
                Warn "C6" "texture missing for $texPath"
            }
            Pass "C6"
        }
    }
} catch { Fail "C6" $_.Exception.Message }

# ---------------------------------------------------------------- C7 research def
try {
    $node = Get-DefNode "Defs\ResearchProjectDefs\RepairBench_RepairTech.xml" "RepairBench_RepairTech"
    if ($null -eq $node) { Fail "C7" "RepairBench_RepairTech not found" }
    else {
        $cost = Select-Text $node "baseCost"
        $pre = @($node.SelectNodes("prerequisites/li") | ForEach-Object { $_.InnerText.Trim() })
        if ($cost -ne "600") { Fail "C7" "baseCost is '$cost', expected 600" }
        elseif ($pre -notcontains "Smithing") { Fail "C7" "prerequisites lacks Smithing" }
        else { Pass "C7" }
    }
} catch { Fail "C7" $_.Exception.Message }

# ---------------------------------------------------------------- C8 designation def
try {
    $node = Get-DefNode "Defs\DesignationDefs\RepairBench_Designations.xml" "RepairBench_RepairDesignation"
    if ($null -eq $node) { Fail "C8" "RepairBench_RepairDesignation not found" }
    else {
        $target = Select-Text $node "targetType"
        $tex = Select-Text $node "texturePath"
        if ($target -ne "Thing") { Fail "C8" "targetType is '$target', expected Thing" }
        elseif ($tex -ne "Designations/RepairBench_Repair") { Fail "C8" "texturePath is '$tex'" }
        else { Pass "C8" }
    }
} catch { Fail "C8" $_.Exception.Message }

# ---------------------------------------------------------------- C9 patch xpath resolves against Core
try {
    $patchDoc = Get-ModXml "Patches\RepairBench_DesignationCategories.xml"
    if ($null -eq $patchDoc) { Fail "C9" "missing Patches/RepairBench_DesignationCategories.xml" }
    elseif (-not (Test-Path -LiteralPath $CoreDir)) { Warn "C9" "CoreDir not found ($CoreDir), patch xpath not verified" }
    else {
        $catFile = $null
        foreach ($f in Get-ChildItem -LiteralPath $CoreDir -Recurse -File -Filter "*.xml" -ErrorAction SilentlyContinue) {
            $text = [System.IO.File]::ReadAllText($f.FullName)
            if ($text -match "<DesignationCategoryDef>" -and $text -match "<defName>Orders</defName>") { $catFile = $f.FullName; break }
        }
        if ($null -eq $catFile) { Warn "C9" "Core DesignationCategories.xml not found, patch xpath not verified" }
        else {
            $catDoc = LoadXml $catFile
            $bad = @()
            foreach ($op in $patchDoc.SelectNodes("//Operation")) {
                $xp = $op.GetAttribute("xpath")
                if (-not $xp) { continue }
                if ($op.LocalName -eq "PatchOperationAdd") {
                    $xpathForCheck = $xp -replace "/[^/]+$", ""
                    $hits = $catDoc.SelectNodes($xpathForCheck)
                } else {
                    $hits = $catDoc.SelectNodes($xp)
                }
                if ($null -eq $hits -or $hits.Count -eq 0) { $bad += $xp }
            }
            if ($bad.Count -gt 0) { Fail "C9" ("xpath matched nothing: " + ($bad -join " | ")) }
            else { Pass "C9" }
        }
    }
} catch { Fail "C9" $_.Exception.Message }

# ---------------------------------------------------------------- C10 designator patch content
try {
    $patchDoc = Get-ModXml "Patches\RepairBench_DesignationCategories.xml"
    if ($null -eq $patchDoc) { Fail "C10" "missing patch file" }
    else {
        $lis = @($patchDoc.SelectNodes("//value/li") | ForEach-Object { $_.InnerText.Trim() })
        if (-not ($lis | Where-Object { $_ -like "RepairBench.*" })) { Fail "C10" "patch value has no RepairBench.* designator class" }
        elseif (-not (Test-Path -LiteralPath $CoreDir)) { Warn "C10" "CoreDir not found, Orders category precondition not verified" }
        else {
            $found = $false
            foreach ($f in Get-ChildItem -LiteralPath $CoreDir -Recurse -File -Filter "*.xml" -ErrorAction SilentlyContinue) {
                $text = [System.IO.File]::ReadAllText($f.FullName)
                if ($text -match "<DesignationCategoryDef>" -and $text -match "<defName>Orders</defName>" -and $text -match "specialDesignatorClasses") { $found = $true; break }
            }
            if (-not $found) { Fail "C10" "Core Orders category has no specialDesignatorClasses" }
            else { Pass "C10" }
        }
    }
} catch { Fail "C10" $_.Exception.Message }

# ---------------------------------------------------------------- C11 work type / work giver / job defs
try {
    $wt = Get-DefNode "Defs\WorkTypeDefs\RepairBench_WorkTypes.xml" "RepairBench_Repairing"
    $wg = Get-DefNode "Defs\WorkGiverDefs\RepairBench_WorkGivers.xml" "RepairBench_RepairDesignated"
    $jd = Get-DefNode "Defs\JobDefs\RepairBench_Jobs.xml" "RepairBench_RepairItem"
    if ($null -eq $wt) { Fail "C11" "RepairBench_Repairing work type not found" }
    elseif ($null -eq $wg) { Fail "C11" "RepairBench_RepairDesignated work giver not found" }
    elseif ($null -eq $jd) { Fail "C11" "RepairBench_RepairItem job def not found" }
    else {
        $prio = Select-Text $wt "naturalPriority"
        $skills = @($wt.SelectNodes("relevantSkills/li") | ForEach-Object { $_.InnerText.Trim() })
        $giverClass = Select-Text $wg "giverClass"
        $wgWorkType = Select-Text $wg "workType"
        $wgPrio = Select-Text $wg "priorityInType"
        $driverClass = Select-Text $jd "driverClass"
        if ($prio -ne "460") { Fail "C11" "work type naturalPriority is '$prio'" }
        elseif ($skills -notcontains "Crafting") { Fail "C11" "work type relevantSkills lacks Crafting" }
        elseif ($giverClass -ne "RepairBench.WorkGiver_RepairDesignated") { Fail "C11" "giverClass is '$giverClass'" }
        elseif ($wgWorkType -ne "RepairBench_Repairing") { Fail "C11" "work giver workType is '$wgWorkType'" }
        elseif ($wgPrio -ne "40") { Fail "C11" "priorityInType is '$wgPrio'" }
        elseif ($driverClass -ne "RepairBench.JobDriver_RepairItem") { Fail "C11" "driverClass is '$driverClass'" }
        else { Pass "C11" }
    }
} catch { Fail "C11" $_.Exception.Message }

# ---------------------------------------------------------------- C12 metadata: classes derive from the right base types
# Windows PowerShell 5.1 runs on .NET Framework and cannot load a net10.0 assembly, so the
# type table is inspected by tools/typecheck.cs through System.Reflection.Metadata instead.
try {
    $dll = Join-Path $modRoot "1.7\Assemblies\RepairBench.dll"
    $checker = Join-Path $modRoot "tools\typecheck.cs"
    $dotnet = Join-Path $env:USERPROFILE ".dotnet\dotnet.exe"
    if (-not (Test-Path -LiteralPath $dll)) { Fail "C12" "missing 1.7/Assemblies/RepairBench.dll (build first)" }
    elseif (-not (Test-Path -LiteralPath $checker)) { Fail "C12" "missing tools/typecheck.cs" }
    elseif (-not (Test-Path -LiteralPath $dotnet)) { Warn "C12" "dotnet 10 not found at $dotnet, metadata check skipped" }
    else {
        $out = & $dotnet run $checker -- $dll 2>&1
        $code = $LASTEXITCODE
        $line = ($out | Where-Object { $_ -match "^(PASS|FAIL) C12" } | Select-Object -First 1)
        if ($code -eq 0 -and $line) { Pass "C12" }
        else { Fail "C12" (($out | Select-Object -Last 6) -join " / ") }
    }
} catch { Fail "C12" $_.Exception.Message }

# ---------------------------------------------------------------- C-LangParity: English and Chinese Keyed keys match
try {
    $enPath = Join-Path $modRoot "Languages\English\Keyed\RepairBench_Keys.xml"
    $zhPath = Join-Path $modRoot "Languages\ChineseSimplified\Keyed\RepairBench_Keys.xml"
    if (-not (Test-Path -LiteralPath $enPath)) { Fail "C-LangParity" "missing English Keyed file" }
    elseif (-not (Test-Path -LiteralPath $zhPath)) { Fail "C-LangParity" "missing Chinese Keyed file" }
    else {
        $en = LoadXml $enPath
        $zh = LoadXml $zhPath
        $enKeys = @($en.DocumentElement.ChildNodes | Where-Object { $_.NodeType -eq "Element" } | ForEach-Object { $_.LocalName })
        $zhKeys = @($zh.DocumentElement.ChildNodes | Where-Object { $_.NodeType -eq "Element" } | ForEach-Object { $_.LocalName })
        $onlyEn = @($enKeys | Where-Object { $zhKeys -notcontains $_ })
        $onlyZh = @($zhKeys | Where-Object { $enKeys -notcontains $_ })
        if ($enKeys.Count -eq 0) { Fail "C-LangParity" "English Keyed file has no keys" }
        elseif ($onlyEn.Count -gt 0 -or $onlyZh.Count -gt 0) {
            Fail "C-LangParity" ("key mismatch; only-en: " + ($onlyEn -join ", ") + " | only-zh: " + ($onlyZh -join ", "))
        } else { Pass "C-LangParity" }
    }
} catch { Fail "C-LangParity" $_.Exception.Message }

# ---------------------------------------------------------------- C-TranslateKeys: every key used in C# exists in both languages
try {
    $srcDir = Join-Path $modRoot "Source\RepairBench"
    $used = @{}
    foreach ($f in Get-ChildItem -LiteralPath $srcDir -File -Filter "*.cs") {
        $text = [System.IO.File]::ReadAllText($f.FullName)
        foreach ($m in [regex]::Matches($text, '"([A-Za-z][A-Za-z0-9_]+)"\s*\.\s*Translate\s*\(')) {
            $used[$m.Groups[1].Value] = $true
        }
    }
    $enKeys = @{}
    $enPath = Join-Path $modRoot "Languages\English\Keyed\RepairBench_Keys.xml"
    if (Test-Path -LiteralPath $enPath) {
        $en = LoadXml $enPath
        foreach ($n in $en.DocumentElement.ChildNodes) { if ($n.NodeType -eq "Element") { $enKeys[$n.LocalName] = $true } }
    }
    $missing = @($used.Keys | Where-Object { -not $enKeys.ContainsKey($_) } | Sort-Object)
    if ($missing.Count -gt 0) { Fail "C-TranslateKeys" ("keys used in C# but missing from Keyed: " + ($missing -join ", ")) }
    elseif ($used.Count -eq 0) { Warn "C-TranslateKeys" "no .Translate() keys found in C# (unexpected)" ; Pass "C-TranslateKeys" }
    else { Pass "C-TranslateKeys" }
} catch { Fail "C-TranslateKeys" $_.Exception.Message }

# ---------------------------------------------------------------- C-DefInjectedPaths: every DefInjected key is "defName.field" on a real Def
# 1.7 (Verse/DefInjectionPackage.cs:109-253) only accepts FLAT keys: the element name must be
# "<defName>.<field>". A nested <ThingDef><defName><field> tree is rejected with "Key lacks a dot"
# and silently translates nothing, so this contract now enforces the flat shape.
try {
    $injDir = Join-Path $modRoot "Languages\ChineseSimplified\DefInjected"
    $bad = @()
    $checked = 0
    if (Test-Path -LiteralPath $injDir) {
        $defsIndex = @{}
        $defsDir = Join-Path $modRoot "Defs"
        foreach ($f in Get-ChildItem -LiteralPath $defsDir -Recurse -File -Filter "*.xml") {
            $doc = LoadXml $f.FullName
            foreach ($d in $doc.DocumentElement.ChildNodes) {
                if ($d.NodeType -ne "Element") { continue }
                $defNameNode = $d.SelectSingleNode("defName")
                if ($null -eq $defNameNode) { continue }
                $defsIndex[$defNameNode.InnerText.Trim()] = @{ Type = $d.LocalName; Node = $d }
            }
        }
        foreach ($f in Get-ChildItem -LiteralPath $injDir -Recurse -File -Filter "*.xml") {
            $doc = LoadXml $f.FullName
            $folder = $f.Directory.Name
            foreach ($node in $doc.DocumentElement.ChildNodes) {
                if ($node.NodeType -ne "Element") { continue }
                $key = $node.LocalName
                $dot = $key.IndexOf(".")
                if ($dot -le 0) {
                    $bad += "$($f.Name): key '$key' lacks a dot (1.7 rejects nested DefInjected)"
                    continue
                }
                $checked++
                $defName = $key.Substring(0, $dot)
                $fieldPath = $key.Substring($dot + 1)
                $entry = $defsIndex[$defName]
                if ($null -eq $entry) { $bad += "$($f.Name): no Def named '$defName' in this mod"; continue }
                if ($entry.Type -ne $folder) { $bad += "$($f.Name): '$defName' is a $($entry.Type), but the file is under $folder/"; continue }
                $fieldNode = $entry.Node.SelectSingleNode($fieldPath)
                if ($null -eq $fieldNode) {
                    $bad += "$($f.Name): '$defName' has no XML path '$fieldPath' (fine only when the field is inherited/default - add it to Defs to be safe)"
                }
            }
        }
    }
    if ($bad.Count -gt 0) { Fail "C-DefInjectedPaths" (" | " + ($bad -join " | ")) }
    elseif ($checked -eq 0) { Warn "C-DefInjectedPaths" "no flat DefInjected keys found"; Pass "C-DefInjectedPaths" }
    else { Pass "C-DefInjectedPaths" }
} catch { Fail "C-DefInjectedPaths" $_.Exception.Message }

# ---------------------------------------------------------------- C-Tex: every referenced texture exists
try {
    $texRoot = Join-Path $modRoot "Textures"
    $texBad = New-Object System.Collections.Generic.List[string]
    $script:texChecked = 0
    $rel = { param($p) $p.Replace("/", "\") }
    $probe = {
        param([string]$path, [string]$who)
        $script:texChecked++
        $full = Join-Path $texRoot (& $rel $path)
        if (-not (Test-Path -LiteralPath ($full + ".png"))) {
            $texBad.Add("$who -> Textures/$path.png missing")
        }
    }
    # Def texture paths (graphicData/texPath + DesignationDef/texturePath)
    $defsDir = Join-Path $modRoot "Defs"
    if (Test-Path -LiteralPath $defsDir) {
        foreach ($f in Get-ChildItem -LiteralPath $defsDir -Recurse -File -Filter "*.xml") {
            $doc = LoadXml $f.FullName
            foreach ($g in $doc.SelectNodes("//*[texPath]")) {
                $path = $g.SelectSingleNode("texPath").InnerText.Trim()
                $gcNode = $g.SelectSingleNode("graphicClass")
                $gc = if ($null -ne $gcNode) { $gcNode.InnerText.Trim() } else { "" }
                if ($gc -match "Graphic_Multi") {
                    & $probe ($path + "_south") ($f.Name + " (south)")
                    & $probe ($path + "_east") ($f.Name + " (east)")
                } elseif ($gc -match "Graphic_StackCount|Graphic_Collection") {
                    # Graphic_Collection resolves texPath to a FOLDER of sprites and splits names on '_'
                    # (RepairKit_a/_b/_c/_d). A lone PNG here makes the game log
                    # "Collection cannot init: No textures found at path ...".
                    $folder = Join-Path $texRoot (& $rel $path)
                    $script:texChecked++
                    if (-not (Test-Path -LiteralPath $folder -PathType Container)) {
                        $texBad.Add("$($f.Name) texPath $path -> folder missing ($gc needs a folder of sprites)")
                    } elseif (@(Get-ChildItem -LiteralPath $folder -File -Filter "*.png").Count -eq 0) {
                        $texBad.Add("$($f.Name) texPath $path -> folder has no .png sprites")
                    }
                } else {
                    & $probe $path ($f.Name + " texPath")
                }
            }
            foreach ($d in $doc.SelectNodes("//DesignationDef/texturePath")) {
                $dp = $d.InnerText.Trim()
                & $probe $dp ($f.Name + " designation")
            }
        }
    }
    # ContentFinder paths used from C#
    $srcDir = Join-Path $modRoot "Source"
    if (Test-Path -LiteralPath $srcDir) {
        foreach ($f in Get-ChildItem -LiteralPath $srcDir -Recurse -File -Filter "*.cs") {
            $text = [System.IO.File]::ReadAllText($f.FullName)
            foreach ($m in [regex]::Matches($text, 'ContentFinder<Texture2D>\s*\.\s*Get\s*\(\s*"([^"]+)"')) {
                & $probe $m.Groups[1].Value ($f.Name + " ContentFinder")
            }
        }
    }
    # About images (ModIcon 128, Preview 1280x720)
    Add-Type -AssemblyName System.Drawing
    $aboutChecks = @(
        @{ Name = "ModIcon.png"; W = 128; H = 128 },
        @{ Name = "Preview.png"; W = 1280; H = 720 }
    )
    foreach ($a in $aboutChecks) {
        $p = Join-Path $modRoot ("About\" + $a.Name)
        if (-not (Test-Path -LiteralPath $p)) { $texBad.Add("About/$($a.Name) missing") }
        else {
            $img = [System.Drawing.Image]::FromFile($p)
            try {
                if ($img.Width -ne $a.W -or $img.Height -ne $a.H) {
                    $texBad.Add("About/$($a.Name) is $($img.Width)x$($img.Height), expected $($a.W)x$($a.H)")
                }
            } finally { $img.Dispose() }
        }
    }
    if ($texBad.Count -gt 0) { Fail "C-Tex" (" | " + ($texBad -join " | ")) }
    elseif ($script:texChecked -eq 0) { Warn "C-Tex" "no texture references found"; Pass "C-Tex" }
    else { Pass "C-Tex" }
} catch { Fail "C-Tex" $_.Exception.Message }

# ---------------------------------------------------------------- C-JobFx: item goes onto the bench, work effect + sound, bench cleared afterwards
try {
    $jdPath = Join-Path $modRoot "Source\RepairBench\JobDriver_RepairItem.cs"
    if (-not (Test-Path -LiteralPath $jdPath)) { Fail "C-JobFx" "JobDriver_RepairItem.cs missing" }
    else {
        $jd = [System.IO.File]::ReadAllText($jdPath)
        $fxBad = New-Object System.Collections.Generic.List[string]
        # Work animation + sound come from the vanilla Repair effecter (sparks + Interact_Repair),
        # inherited by the bench from BuildingBase.repairEffect.
        if ($jd -notmatch 'WithEffect\s*\(') { $fxBad.Add("no WithEffect(...): no work-animation particles/sound") }
        if ($jd -notmatch 'repairEffect') { $fxBad.Add("no repairEffect: animation must use the bench's vanilla repair effecter") }
        # The repaired item must be laid on the bench top, and cleared off it when done.
        if ($jd -notmatch 'ThingPlaceMode\.Direct') { $fxBad.Add("no ThingPlaceMode.Direct: item is not placed on the bench top") }
        if ($jd -notmatch 'GenPlace\.TryPlaceThing') { $fxBad.Add("no GenPlace.TryPlaceThing: item never leaves the bench after repair") }
        if ($jd -notmatch 'SkillDefOf\.Crafting') { $fxBad.Add("no SkillDefOf.Crafting: no skill while repairing") }
        if ($fxBad.Count -gt 0) { Fail "C-JobFx" (" | " + ($fxBad -join " | ")) } else { Pass "C-JobFx" }
    }
} catch { Fail "C-JobFx" $_.Exception.Message }

# ---------------------------------------------------------------- C-KitTex: repair kit keeps one fixed sprite for every stack count
try {
    $node = Get-DefNode "Defs\ThingDefs_Items\RepairBench_RepairKit.xml" "RepairBench_RepairKit"
    if ($null -eq $node) { Fail "C-KitTex" "RepairBench_RepairKit not found" }
    else {
        $gc = Select-Text $node "graphicData/graphicClass"
        $texPath = Select-Text $node "graphicData/texPath"
        if ($gc -ne "Graphic_Single") {
            Fail "C-KitTex" "graphicClass is '$gc', expected Graphic_Single so 1/10/50 kits all show the same sprite"
        } else {
            $png = Join-Path $modRoot ("Textures\" + ($texPath -replace "/", "\") + ".png")
            if (-not (Test-Path -LiteralPath $png)) { Fail "C-KitTex" "single sprite missing: $texPath.png" } else { Pass "C-KitTex" }
        }
    }
} catch { Fail "C-KitTex" $_.Exception.Message }

# ---------------------------------------------------------------- summary
Write-Host ""
if ($script:failures -gt 0) {
    Write-Host ("RESULT: {0} FAIL, {1} WARN" -f $script:failures, $script:warnings) -ForegroundColor Red
    if ($Strict) { exit 1 }
} else {
    Write-Host ("RESULT: all contracts passed ({0} WARN)" -f $script:warnings) -ForegroundColor Green
}
exit 0