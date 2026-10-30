param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\artifacts\release')
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repoRoot = Split-Path $PSScriptRoot -Parent
[xml]$properties = Get-Content (Join-Path $repoRoot 'Directory.Build.props')
$version = [string]$properties.Project.PropertyGroup.Version
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw 'Expected a stable three-part version.' }
$outputRoot = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$name = "Homeryn-$version-win-x64"
$zipPath = Join-Path $outputRoot "$name.zip"
if (Test-Path -LiteralPath $zipPath) { throw "Package already exists: $zipPath" }
$staging = Join-Path $outputRoot ('staging-' + [guid]::NewGuid().ToString('N'))
$package = Join-Path $staging $name
New-Item -ItemType Directory -Path $package -Force | Out-Null
foreach ($project in @('Desktop', 'Server')) {
    $destination = $package
    if ($project -eq 'Server') { $destination = Join-Path $package 'server' }
    & dotnet publish (Join-Path $repoRoot "src\Homeryn.$project\Homeryn.$project.csproj") --configuration Release --runtime win-x64 --self-contained true --output $destination --warnaserror -p:DebugType=None -p:DebugSymbols=false
    if ($LASTEXITCODE -ne 0) { throw "Publishing $project failed." }
}
Copy-Item -LiteralPath (Join-Path $repoRoot 'LICENSE') -Destination $package
Copy-Item -LiteralPath (Join-Path $repoRoot 'SECURITY.md') -Destination $package
Copy-Item -LiteralPath (Join-Path $repoRoot 'docs\user-guide.md') -Destination (Join-Path $package 'START-HERE.md')
Copy-Item -LiteralPath (Join-Path $repoRoot "docs\releases\v$version.md") -Destination (Join-Path $package 'RELEASE-NOTES.md')
Copy-Item -LiteralPath (Join-Path $repoRoot 'docs\THIRD-PARTY-NOTICES.md') -Destination $package
Copy-Item -LiteralPath (Join-Path $repoRoot 'docs\licenses') -Destination $package -Recurse
$dependencies = @()
$copiedRuntimePacks = @{}
foreach ($project in @('Desktop', 'Server')) {
    $assets = Get-Content (Join-Path $repoRoot "src\Homeryn.$project\obj\project.assets.json") -Raw | ConvertFrom-Json
    $packageRoots = @($assets.packageFolders.PSObject.Properties.Name)
    $dependencies += @($assets.libraries.PSObject.Properties | Where-Object { $_.Value.type -eq 'package' } | ForEach-Object { $_.Name })
    $runtimePath = Join-Path $package 'Homeryn.Desktop.runtimeconfig.json'
    if ($project -eq 'Server') { $runtimePath = Join-Path $package 'server\Homeryn.Server.runtimeconfig.json' }
    $runtime = Get-Content $runtimePath -Raw | ConvertFrom-Json
    foreach ($framework in $runtime.runtimeOptions.includedFrameworks) {
        $pack = ($framework.name + '.Runtime.win-x64').ToLowerInvariant()
        $key = "$pack/$($framework.version)"
        if ($copiedRuntimePacks.ContainsKey($key)) { continue }
        $dependencies += $key
        $source = $null
        foreach ($root in $packageRoots) {
            $candidate = Join-Path $root $key
            if (Test-Path -LiteralPath $candidate) { $source = $candidate; break }
        }
        if ($null -eq $source) { throw "Cannot locate notices for runtime pack $key" }
        $noticeDirectory = Join-Path $package "licenses\$pack-$($framework.version)"
        New-Item -ItemType Directory -Path $noticeDirectory | Out-Null
        $notices = @(Get-ChildItem -LiteralPath $source -File | Where-Object { $_.Name -match 'LICENSE|NOTICE' })
        if ($notices.Count -eq 0) { throw "No license found for $key" }
        $notices | Copy-Item -Destination $noticeDirectory
        $copiedRuntimePacks[$key] = $true
    }
}
$dependencies | Sort-Object -Unique | Set-Content -LiteralPath (Join-Path $package 'DEPENDENCIES.txt') -Encoding UTF8
Set-Content -LiteralPath (Join-Path $package 'VERSION.txt') -Value $version -Encoding ASCII
# Keep the uncompressed package for smoke testing and diagnostics; never erase prior output.
Compress-Archive -LiteralPath $package -DestinationPath $zipPath -CompressionLevel Optimal
$hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
Set-Content -LiteralPath (Join-Path $outputRoot 'SHA256SUMS.txt') -Value "$hash  $name.zip" -Encoding ASCII
Write-Output "Package: $zipPath"
