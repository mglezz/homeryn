$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repoRoot = Split-Path $PSScriptRoot -Parent
Push-Location $repoRoot
try {
    # Pin both the scanner version and archive digest; never execute an unchecked download.
    $toolDirectory = Join-Path $repoRoot 'artifacts/security-tools/gitleaks-8.30.1'
    New-Item -ItemType Directory -Path $toolDirectory -Force | Out-Null
    $archive = Join-Path $toolDirectory 'scanner.zip'
    Invoke-WebRequest -UseBasicParsing 'https://github.com/gitleaks/gitleaks/releases/download/v8.30.1/gitleaks_8.30.1_windows_x64.zip' -OutFile $archive
    $expected = 'd29144deff3a68aa93ced33dddf84b7fdc26070add4aa0f4513094c8332afc4e'
    if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expected) {
        throw 'Secret scanner checksum mismatch.'
    }
    Expand-Archive -LiteralPath $archive -DestinationPath $toolDirectory -Force
    & (Join-Path $toolDirectory 'gitleaks.exe') git . --redact --no-banner --log-opts=--all
    if ($LASTEXITCODE -ne 0) { throw 'Secret scan failed. Inspect redacted diagnostics privately.' }

    # This command returns success even when it finds advisories; inspect its JSON as well.
    $auditText = & dotnet list Homeryn.slnx package --vulnerable --include-transitive --format json --output-version 1
    if ($LASTEXITCODE -ne 0) { throw 'Dependency audit could not complete.' }
    $audit = ($auditText -join "`n") | ConvertFrom-Json
    if ($null -ne $audit.PSObject.Properties['problems'] -and @($audit.problems).Count -gt 0) {
        throw 'Dependency audit reported problems.'
    }
    if (@($audit.projects).Count -ne 4) { throw 'Dependency audit did not cover all four projects.' }
    foreach ($project in $audit.projects) {
        if ($null -eq $project.PSObject.Properties['frameworks']) { continue }
        foreach ($framework in $project.frameworks) {
            foreach ($group in @('topLevelPackages', 'transitivePackages')) {
                if ($null -eq $framework.PSObject.Properties[$group]) { continue }
                foreach ($package in $framework.$group) {
                    if ($null -ne $package.PSObject.Properties['vulnerabilities'] -and @($package.vulnerabilities).Count -gt 0) {
                        throw "Known vulnerability found in dependency $($package.id)."
                    }
                }
            }
        }
    }
    Write-Output 'PASS: Git history secret scan and direct/transitive NuGet vulnerability audit.'
}
finally {
    Pop-Location
}
