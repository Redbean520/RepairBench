param(
    [string]$RimWorldDir = "D:\SteamLibrary\steamapps\common\RimWorld",
    [switch]$DryRun,
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
$modRoot = Split-Path -Parent $PSScriptRoot
$modName = Split-Path -Leaf $modRoot
$sourceDir = Join-Path $modRoot "Source\RepairBench"
$targetDir = Join-Path $RimWorldDir ("Mods\" + $modName)

# Whitelist of things the game actually reads. Defs/Patches/Textures/Languages live at the mod root,
# so they MUST be listed here (the WorldTechLevelPatch template did not need them).
$include = @("About", "Defs", "Patches", "Textures", "Languages", "1.7", "LoadFolders.xml", "README.md")

if ($DryRun) {
    Write-Host "[dry-run] mod root : $modRoot"
    Write-Host "[dry-run] target   : $targetDir"
    Write-Host "[dry-run] build    : $(if ($NoBuild) { 'skipped' } else { 'would run dotnet build' })"
    foreach ($item in $include) {
        $src = Join-Path $modRoot $item
        if (Test-Path $src) { Write-Host "[dry-run] copy     : $item" } else { Write-Host "[dry-run] missing  : $item" }
    }
    $published = Join-Path $targetDir "About\PublishedFileId.txt"
    if (Test-Path $published) { Write-Host "[dry-run] publishedFileId: $(([System.IO.File]::ReadAllText($published)).Trim())" }
    else { Write-Host "[dry-run] publishedFileId: missing" }
    if (Get-Process -Name "RimWorld", "RimWorldWin64" -ErrorAction SilentlyContinue) { Write-Host "[dry-run] RimWorld is running (would be stopped)" }
    exit 0
}

# The maintainer accepts losing unsaved progress: kill the game before touching Assemblies.
$gameProc = Get-Process -Name "RimWorld", "RimWorldWin64" -ErrorAction SilentlyContinue
if ($gameProc) {
    $gamePids = ($gameProc | ForEach-Object { $_.Id }) -join ", "
    Write-Host "RimWorld is running (pid $gamePids), stopping it as agreed before deploy..." -ForegroundColor Yellow
    $gameProc | Stop-Process -Force -ErrorAction SilentlyContinue
    $deadline = (Get-Date).AddSeconds(20)
    while ((Get-Process -Name "RimWorld", "RimWorldWin64" -ErrorAction SilentlyContinue) -and (Get-Date) -lt $deadline) {
        Start-Sleep -Milliseconds 250
    }
    $stillAlive = Get-Process -Name "RimWorld", "RimWorldWin64" -ErrorAction SilentlyContinue
    if ($stillAlive) {
        throw "Failed to stop RimWorld (pid " + (($stillAlive | ForEach-Object { $_.Id }) -join ", ") + ")"
    }
    Write-Host "Game closed, continuing deploy."
}

if (-not $NoBuild) {
    Write-Host "Building $modName ..."
    & "$env:USERPROFILE\.dotnet\dotnet.exe" build $sourceDir -c Release
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
}

$dll = Join-Path $modRoot "1.7\Assemblies\RepairBench.dll"
if (-not (Test-Path $dll)) { throw "Missing build output $dll" }

# Workshop id must survive the wipe of the deployed folder, otherwise the next upload
# creates a duplicate workshop item.
$aboutPublished = Join-Path $targetDir "About\PublishedFileId.txt"
$repoPublished = Join-Path $modRoot "About\PublishedFileId.txt"
$publishedFileId = $null

if (Test-Path $aboutPublished) { $publishedFileId = ([System.IO.File]::ReadAllText($aboutPublished)).Trim() }
if (-not $publishedFileId -and (Test-Path $repoPublished)) { $publishedFileId = ([System.IO.File]::ReadAllText($repoPublished)).Trim() }
if ($publishedFileId -and -not (Test-Path $repoPublished)) {
    [System.IO.File]::WriteAllText($repoPublished, $publishedFileId, (New-Object System.Text.UTF8Encoding($false)))
}
if ($publishedFileId) { Write-Host "publishedFileId (before): $publishedFileId" }
else { Write-Host "publishedFileId (before): none - fine for a first release" -ForegroundColor Yellow }

if (Test-Path $targetDir) {
    Write-Host "Clearing $targetDir ..."
    Remove-Item -Recurse -Force $targetDir
}
New-Item -ItemType Directory -Path $targetDir -Force | Out-Null

Write-Host "Copying to $targetDir ..."
foreach ($item in $include) {
    $src = Join-Path $modRoot $item
    if (-not (Test-Path $src)) { continue }
    Copy-Item -Path $src -Destination (Join-Path $targetDir $item) -Recurse -Force
}

Get-ChildItem $targetDir -Recurse -Include "*.pdb", "*.user" -File -ErrorAction SilentlyContinue |
    Remove-Item -Force -ErrorAction SilentlyContinue

$aboutPublished = Join-Path $targetDir "About\PublishedFileId.txt"
if ($publishedFileId -and -not (Test-Path $aboutPublished)) {
    [System.IO.File]::WriteAllText($aboutPublished, $publishedFileId, (New-Object System.Text.UTF8Encoding($false)))
}
if (Test-Path $aboutPublished) { Write-Host "publishedFileId (after): $(([System.IO.File]::ReadAllText($aboutPublished)).Trim())" }
else { Write-Host "publishedFileId (after): none - fine for a first release" -ForegroundColor Yellow }

Write-Host "Done: $dll -> $targetDir"
