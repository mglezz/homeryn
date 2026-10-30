param([Parameter(Mandatory = $true)][string]$ArchivePath)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('Homeryn.Package.' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
Expand-Archive -LiteralPath $ArchivePath -DestinationPath $scratch
$package = @(Get-ChildItem -LiteralPath $scratch -Directory)
if ($package.Count -ne 1) { throw 'Expected a single top-level package folder.' }
foreach ($entry in Get-ChildItem -LiteralPath $package[0].FullName -Recurse -Force) {
    if ($entry.Name -match '(?i)^(\.git|\.vs|\.env(?:\..*)?)$|\.(db(?:-(?:wal|shm|journal))?|sqlite(?:3)?(?:-(?:wal|shm|journal))?|pfx|p12|pem|key|pdb|suo|user|log)$') {
        throw "Disallowed private, database, or debug file in package: $($entry.Name)"
    }
}
$desktop = Join-Path $package[0].FullName 'Homeryn.Desktop.exe'
$serverExe = Join-Path $package[0].FullName 'server\Homeryn.Server.exe'
foreach ($file in @($desktop, $serverExe, (Join-Path $package[0].FullName 'coreclr.dll'), (Join-Path $package[0].FullName 'server\coreclr.dll'))) {
    if (-not (Test-Path -LiteralPath $file)) { throw "Missing self-contained package file: $file" }
}
$releaseMetadata = Invoke-RestMethod 'https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json'
$minimumRuntime = [version]$releaseMetadata.'latest-runtime'
if ($minimumRuntime.Major -ne 10) { throw 'Unexpected .NET release metadata.' }
foreach ($runtimeFile in @('Homeryn.Desktop.runtimeconfig.json', 'server\Homeryn.Server.runtimeconfig.json')) {
    $config = Get-Content -LiteralPath (Join-Path $package[0].FullName $runtimeFile) -Raw | ConvertFrom-Json
    if ($config.runtimeOptions.PSObject.Properties.Name -contains 'frameworks' -or $config.runtimeOptions.PSObject.Properties.Name -contains 'framework') {
        throw 'The package unexpectedly depends on a separately installed runtime.'
    }
    if ($null -eq $config.runtimeOptions.PSObject.Properties['includedFrameworks'] -or @($config.runtimeOptions.includedFrameworks).Count -eq 0) {
        throw 'Bundled runtime versions are missing.'
    }
    foreach ($framework in $config.runtimeOptions.includedFrameworks) {
        if ([version]$framework.version -lt $minimumRuntime) {
            throw "Outdated bundled runtime $($framework.name) $($framework.version); update to $minimumRuntime or later."
        }
    }
}
$database = Join-Path $scratch 'original.db'
$backup = Join-Path $scratch 'backup.db'
$serverProcess = $null
$desktopProcess = $null

function Read-Exactly($Stream, [byte[]]$Buffer) {
    $offset = 0
    while ($offset -lt $Buffer.Length) {
        $count = $Stream.Read($Buffer, $offset, $Buffer.Length - $offset)
        if ($count -eq 0) { throw 'Unexpected end of response.' }
        $offset += $count
    }
}
function Send-Request($Request) {
    $client = New-Object Net.Sockets.TcpClient
    try {
        $connection = $client.ConnectAsync('127.0.0.1', 5000)
        if (-not $connection.Wait(1000)) { throw 'Connection timed out.' }
        $stream = $client.GetStream()
        $stream.ReadTimeout = 5000
        $stream.WriteTimeout = 5000
        $bytes = [Text.Encoding]::UTF8.GetBytes(($Request | ConvertTo-Json -Depth 8 -Compress))
        $header = [BitConverter]::GetBytes([Net.IPAddress]::HostToNetworkOrder([int]$bytes.Length))
        $stream.Write($header, 0, 4)
        $stream.Write($bytes, 0, $bytes.Length)
        Read-Exactly $stream $header
        $length = [Net.IPAddress]::NetworkToHostOrder([BitConverter]::ToInt32($header, 0))
        if ($length -le 0 -or $length -gt 4096) { throw 'Invalid response size.' }
        $payload = New-Object byte[] $length
        Read-Exactly $stream $payload
        return [Text.Encoding]::UTF8.GetString($payload) | ConvertFrom-Json
    }
    finally { $client.Dispose() }
}
function Start-TestServer([string]$Path, [string]$Label) {
    $process = Start-Process -FilePath $serverExe -ArgumentList @('--database', ('"' + $Path + '"')) -WorkingDirectory $scratch -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $scratch "$Label.out.log") -RedirectStandardError (Join-Path $scratch "$Label.err.log")
    try {
        $ready = $false
        for ($attempt = 0; $attempt -lt 30; $attempt++) {
            if ($process.HasExited) { throw "Packaged server exited: $(Get-Content (Join-Path $scratch "$Label.err.log") -Raw)" }
            try { $ready = (Send-Request @{ command = 'PING' }).status -eq 'PONG' } catch { $ready = $false }
            if ($ready) { return $process }
            Start-Sleep -Milliseconds 200
        }
        throw 'Packaged server did not become ready.'
    }
    catch { if (-not $process.HasExited) { $process.Kill(); $process.WaitForExit() }; $process.Dispose(); throw }
}
function Assert-Ok($Response) {
    if ($Response.status -ne 'OK') { throw "Unexpected response: $($Response | ConvertTo-Json -Compress -Depth 8)" }
}

try {
    # Never interrupt an existing user's server to run a smoke test.
    $probe = New-Object Net.Sockets.TcpListener([Net.IPAddress]::Loopback, 5000)
    try { $probe.Start() } finally { $probe.Stop() }
    $serverProcess = Start-TestServer $database 'original'
    $added = Send-Request @{ command = 'ADD_ITEM'; item = @{ name = 'Portable milk'; quantity = 2; isPurchased = $false } }
    Assert-Ok $added
    $chore = Send-Request @{ command = 'ADD_CHORE'; chore = @{ name = 'Portable kitchen'; assignee = 'Alex'; dueDate = '2026-10-31'; isCompleted = $false } }
    Assert-Ok $chore
    $updated = Send-Request @{ command = 'UPDATE_ITEM'; itemId = $added.item.id; expectedVersion = 1; item = @{ name = 'Portable milk'; quantity = 4; isPurchased = $true } }
    Assert-Ok $updated
    $stale = Send-Request @{ command = 'DELETE_ITEM'; itemId = $added.item.id; expectedVersion = 1 }
    if ($stale.error -ne 'ITEM_CONFLICT') { throw 'The packaged server accepted a stale deletion.' }
    & $serverExe --database $database --backup $backup
    if ($LASTEXITCODE -ne 0) { throw 'Packaged backup failed.' }
    $refusal = Start-Process -FilePath $serverExe -ArgumentList @('--database', ('"' + $database + '"'), '--backup', ('"' + $backup + '"')) -WindowStyle Hidden -Wait -PassThru -RedirectStandardOutput (Join-Path $scratch 'refusal.out.log') -RedirectStandardError (Join-Path $scratch 'refusal.err.log')
    try { if ($refusal.ExitCode -eq 0) { throw 'An existing backup was overwritten.' } }
    finally { $refusal.Dispose() }
    $serverProcess.Kill(); $serverProcess.WaitForExit(); $serverProcess.Dispose(); $serverProcess = $null
    $serverProcess = Start-TestServer $backup 'restored'
    $list = Send-Request @{ command = 'LIST_ITEMS' }
    Assert-Ok $list
    if ($list.page.items.Count -ne 1 -or $list.page.items[0].quantity -ne 4 -or $list.page.items[0].version -ne 2 -or -not $list.page.items[0].isPurchased) { throw 'Restored shopping fields differ.' }
    $list = Send-Request @{ command = 'LIST_CHORES' }
    Assert-Ok $list
    if ($list.chorePage.items.Count -ne 1 -or $list.chorePage.items[0].assignee -ne 'Alex') { throw 'Restored chores differ.' }
    # The GUI starts from an extracted folder outside the repository.
    $desktopProcess = Start-Process -FilePath $desktop -WorkingDirectory $scratch -WindowStyle Hidden -PassThru
    if (-not $desktopProcess.WaitForInputIdle(10000) -or $desktopProcess.HasExited) { throw 'Packaged desktop did not initialize.' }
    Write-Output 'PASS: extracted self-contained desktop, TCP CRUD, stale deletion, online backup and restore.'
}
finally {
    foreach ($process in @($desktopProcess, $serverProcess)) {
        if ($null -ne $process) {
            if (-not $process.HasExited) { $process.Kill(); $process.WaitForExit() }
            $process.Dispose()
        }
    }
    Write-Output "Smoke-test data and logs: $scratch"
}
