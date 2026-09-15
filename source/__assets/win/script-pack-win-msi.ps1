#Requires -Version 7.0
<#
.SYNOPSIS
    Build (and optionally sign) the Windows MSI installer of ImageGlass.Win32 (x64).

.DESCRIPTION
    Produces the setup program published on GitHub Releases for users who do not want the
    portable ZIP: the same self-contained AOT build plus the shared app assets, harvested
    into a Windows Installer package by WiX 5.

    The package is DUAL SCOPE. The wizard asks whether to install for the current user
    (%LocalAppData%\Programs\ImageGlass, no elevation) or for every user
    (%ProgramFiles%\ImageGlass, elevated), and both work unattended through msiexec:

        msiexec /i <package>.msi /qn ALLUSERS=2 MSIINSTALLPERUSER=1        # per-user
        msiexec /i <package>.msi /qn ALLUSERS=2 MSIINSTALLPERUSER=""       # per-machine

    Installing removes ImageGlass 9 if it is present. Uninstalling runs
    "ImageGlass.exe --ig-remove-default-viewer" so the classic file-type registration does
    not outlive the app.

    An upgrade must run in the scope of the install it replaces, because Windows Installer
    cannot move a product between the two. The package detects that scope and locks the
    wizard to it; a silent install that disagrees is refused, not silently corrected. See
    "Upgrading an existing install" in __assets/win/README.md.

    The portable marker is NEVER shipped here: an installed copy carrying .igportable cannot
    start at all, because Program Files is not writable and the app refuses to fall back to
    %LocalAppData% behind the user's back.

    Output: __artifacts/bundle/ImageGlass_<label>_win-x64.msi

.PARAMETER Platform
    Target architecture. Only x64 is supported today; the parameter exists so the publish
    block stays identical to the MSIX/ZIP packers and arm64 is a small diff later.

.PARAMETER Sign
    Authenticode-sign every payload .exe / .dll before harvesting AND the finished .msi.
    Payload signing must happen first: wix build records each file size and MsiFileHash row
    and compresses the bytes into the embedded cabinet, so signing afterwards is impossible.
    If no certificate is found the installer is still built, unsigned, with a warning.

.PARAMETER CertSubject
    Substring of the code-signing certificate Subject to select it from the Current User /
    Local Machine "My" store (passed to signtool /n). Ignored when -CertFile is supplied.

.PARAMETER CertFile
    Path to a PFX certificate to sign with instead of a store certificate.

.PARAMETER CertPassword
    Password for -CertFile (if any).

.PARAMETER TimestampUrl
    RFC-3161 timestamp server. Default: http://timestamp.sectigo.com

.PARAMETER ProductVersion
    Override the MSI ProductVersion. Defaults to <IgVersion> from Directory.Build.props.

.PARAMETER CompressionLevel
    Cabinet compression. Default "high" for a release; "mszip" is much faster for local runs.

.PARAMETER SkipPublish
    Reuse the existing __artifacts/publish/win-x64 output instead of re-publishing
    (faster iteration; the installer may not reflect uncommitted source changes).

.PARAMETER SkipValidation
    Skip the ICE pass. Local iteration only: a release build must validate.

.PARAMETER BootstrapWix
    Install the pinned WiX tool and UI extension if they are missing, instead of failing with
    the commands to run. Opt-in, because a pack script must not change the global tool set.

.EXAMPLE
    pwsh __assets/win/script-pack-win-msi.ps1 -Sign
    # Signed x64 installer for GitHub Releases.

.EXAMPLE
    pwsh __assets/win/script-pack-win-msi.ps1 -SkipPublish -SkipValidation -CompressionLevel mszip
    # Fast local rebuild of just the installer around an existing publish dir.
#>

[CmdletBinding()]
param(
    [ValidateSet('x64')]
    [string]$Platform = 'x64',

    [switch]$Sign,

    [string]$CertSubject = 'Duong Dieu Phap',
    [string]$CertFile = '',
    [string]$CertPassword = '',
    [string]$TimestampUrl = 'http://timestamp.sectigo.com',

    [string]$ProductVersion = '',

    [ValidateSet('none', 'low', 'medium', 'high', 'mszip')]
    [string]$CompressionLevel = 'high',

    [switch]$SkipPublish,
    [switch]$SkipValidation,
    [switch]$BootstrapWix
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# --- Paths ---------------------------------------------------------------------
$WorkspaceDir = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$ProjectFile  = Join-Path $WorkspaceDir 'ImageGlass.Win32\ImageGlass.Win32.csproj'
$BuildProps   = Join-Path $WorkspaceDir 'Directory.Build.props'
$AppExtras    = Join-Path $WorkspaceDir '__assets\__app'
$DistDir      = Join-Path $WorkspaceDir '__artifacts\bundle'
$MsiDir       = Join-Path $PSScriptRoot 'msi'
$WxiFile      = Join-Path $MsiDir 'Variables.wxi'
$WxsFiles     = @((Join-Path $MsiDir 'Package.wxs'), (Join-Path $MsiDir 'UI.wxs'))
$WxlFile      = Join-Path $MsiDir 'en-us.wxl'

# Marker file that turns on portable mode; must match Const.PORTABLE_MARKER_FILE.
# An installed copy carrying it cannot start, so this packer refuses to ship one.
$PortableMarker = '.igportable'

# WiX is PINNED. WiX 6 and 7 are the same tool relicensed under a FireGiant maintenance fee,
# and a bare "dotnet tool install --global wix" installs the newest one, so never relax this.
$WixVersion = '5.0.2'
$WixUiExt   = 'WixToolset.UI.wixext'

# Unavoidable for a dual-scope package: ICE57 (HKMU shortcut keypaths), ICE61
# (AllowSameVersionUpgrades), ICE105 (the deferred no-impersonate uninstall action).
$SuppressIce = @('ICE57', 'ICE61', 'ICE105')

# --- Helpers -------------------------------------------------------------------

# Read a single <Tag>value</Tag> from Directory.Build.props.
function Get-BuildProp([string]$Tag) {
    $m = Select-String -Path $BuildProps -Pattern "<$Tag>(.*?)</$Tag>" | Select-Object -First 1
    if ($m) { return $m.Matches[0].Groups[1].Value.Trim() }
    return ''
}

# Read a single <?define Name = "value" ?> from the WiX include, so the GUIDs live in one place.
function Get-WxiDefine([string]$Name) {
    $m = Select-String -Path $WxiFile -Pattern "\?define\s+$Name\s*=\s*`"(.*?)`"" | Select-Object -First 1
    if ($m) { return $m.Matches[0].Groups[1].Value.Trim() }
    return ''
}

# Locate signtool.exe, preferring the newest Windows SDK and an x64 host build.
function Find-SdkTool([string]$Name) {
    $roots = @(
        "${env:ProgramFiles(x86)}\Windows Kits\10\bin",
        "${env:ProgramFiles}\Windows Kits\10\bin"
    ) | Where-Object { $_ -and (Test-Path $_) }

    foreach ($root in $roots) {
        $hit = Get-ChildItem -Path $root -Directory -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -match '^10\.' } |
            Sort-Object { [version]$_.Name } -Descending |
            ForEach-Object { Join-Path $_.FullName "x64\$Name" } |
            Where-Object { Test-Path $_ } |
            Select-Object -First 1
        if ($hit) { return $hit }
    }

    $onPath = Get-Command $Name -ErrorAction SilentlyContinue
    if ($onPath) { return $onPath.Source }

    throw "Could not find $Name. Install the Windows 10/11 SDK (includes signtool)."
}

# Locate the wix dotnet global tool. PATH first, then the default global-tool folder, which a
# same-session "dotnet tool install" does not add to the current process PATH.
function Find-WixTool {
    $onPath = Get-Command 'wix' -CommandType Application -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if ($onPath) { return $onPath.Source }

    $globalTool = Join-Path $env:USERPROFILE '.dotnet\tools\wix.exe'
    if (Test-Path $globalTool) { return $globalTool }

    if ($BootstrapWix) {
        Write-Host "    installing the wix dotnet tool $WixVersion"
        & dotnet tool install --global wix --version $WixVersion
        if ($LASTEXITCODE -ne 0) { throw "dotnet tool install wix failed (exit $LASTEXITCODE)." }
        if (Test-Path $globalTool) { return $globalTool }
    }

    throw @"
Could not find 'wix'. Install the pinned WiX Toolset (or re-run with -BootstrapWix):
  dotnet tool install --global wix --version $WixVersion
  wix extension add -g $WixUiExt/$WixVersion
"@
}

# Refuse any WiX but the pinned one; "wix --version" prints "<major>.<minor>.<patch>+<commit>".
function Assert-WixVersion([string]$Wix) {
    $raw = & $Wix --version
    if ($LASTEXITCODE -ne 0) { throw "wix --version failed (exit $LASTEXITCODE)." }

    $text  = ($raw | Out-String).Trim()
    $match = [regex]::Match($text, '(\d+\.\d+\.\d+)')
    $found = if ($match.Success) { $match.Groups[1].Value } else { $text }

    if ($found -ne $WixVersion) {
        throw @"
WiX $found found at '$Wix', but this installer is authored against $WixVersion. Re-pin it:
  dotnet tool uninstall --global wix
  dotnet tool install --global wix --version $WixVersion
"@
    }
    return $found
}

# The UI extension lives in the wix cache, not the dotnet tool, so it is pinned separately.
# "wix extension list" exits 2 when nothing matched, so parse the output, not $LASTEXITCODE.
function Assert-WixUiExtension([string]$Wix) {
    $pattern = "^\s*$([regex]::Escape($WixUiExt))\s+$([regex]::Escape($WixVersion))\s*$"
    $found = @(& $Wix extension list -g | Where-Object { "$_" -match $pattern })

    if ($found.Count -eq 0 -and $BootstrapWix) {
        Write-Host "    adding the wix UI extension $WixUiExt/$WixVersion"
        & $Wix extension add -g "$WixUiExt/$WixVersion"
        if ($LASTEXITCODE -ne 0) { throw "wix extension add failed (exit $LASTEXITCODE)." }
        $found = @(& $Wix extension list -g | Where-Object { "$_" -match $pattern })
    }

    if ($found.Count -eq 0) {
        throw @"
WiX extension $WixUiExt/$WixVersion is not in the cache (or re-run with -BootstrapWix):
  wix extension add -g $WixUiExt/$WixVersion
"@
    }
}

# Whether a usable code-signing certificate is available; also records which store it lives in so
# signtool searches the same one. Returns $false rather than throwing: the caller then packs UNSIGNED.
$script:UseMachineStore = $false
function Test-SigningCert {
    if ($CertFile) {
        if (Test-Path $CertFile) { return $true }
        Write-Warning "Certificate file not found: $CertFile"
        return $false
    }
    foreach ($store in @(
            @{ Path = 'Cert:\CurrentUser\My';  Machine = $false },
            @{ Path = 'Cert:\LocalMachine\My'; Machine = $true })) {
        $cert = Get-ChildItem $store.Path -CodeSigningCert -ErrorAction SilentlyContinue |
            Where-Object { $_.Subject -like "*$CertSubject*" -and $_.HasPrivateKey } |
            Select-Object -First 1
        if ($cert) {
            $script:UseMachineStore = $store.Machine
            return $true
        }
    }
    return $false
}

# Sign one or more files with signtool (Authenticode, SHA-256, timestamped).
# Returns $true on success, $false on failure (never throws).
function Invoke-SignTool([string]$SignTool, [string[]]$Files) {
    if ($Files.Count -eq 0) { return $true }
    $common = @('sign', '/fd', 'SHA256', '/tr', $TimestampUrl, '/td', 'SHA256')
    if ($CertFile) {
        $common += @('/f', $CertFile)
        if ($CertPassword) { $common += @('/p', $CertPassword) }
    }
    else {
        $common += @('/n', $CertSubject, '/a')
        if ($script:UseMachineStore) { $common += '/sm' }
    }
    # Out-Host, not bare: native stdout would otherwise join this function's return value and
    # the caller would see a non-empty array instead of the bool.
    & $SignTool @common @Files | Out-Host
    return ($LASTEXITCODE -eq 0)
}

# RFC 4122 v5 name-based GUID: a given ProductVersion always yields the same ProductCode, so
# "msiexec /x {GUID}" scripts keep working for a released build.
function New-DeterministicGuid([guid]$Namespace, [string]$Name) {
    $ns = $Namespace.ToByteArray()
    # .NET stores the first three fields little-endian; RFC 4122 hashes them big-endian.
    [Array]::Reverse($ns, 0, 4)
    [Array]::Reverse($ns, 4, 2)
    [Array]::Reverse($ns, 6, 2)

    $sha1 = [System.Security.Cryptography.SHA1]::Create()
    try { $hash = $sha1.ComputeHash($ns + [System.Text.Encoding]::UTF8.GetBytes($Name)) }
    finally { $sha1.Dispose() }

    $g = [byte[]]$hash[0..15]
    $g[6] = ($g[6] -band 0x0F) -bor 0x50   # version 5
    $g[8] = ($g[8] -band 0x3F) -bor 0x80   # RFC 4122 variant
    [Array]::Reverse($g, 0, 4)
    [Array]::Reverse($g, 4, 2)
    [Array]::Reverse($g, 6, 2)
    return [guid]::new($g)
}

# Open an MSI table and return each row as a string array. Missing tables come back empty.
function Get-MsiRows([object]$Database, [string]$Sql, [int]$FieldCount) {
    $rows = @()
    # A missing table throws here; that just means zero rows, which is what we want to report.
    try { $view = $Database.OpenView($Sql) } catch { return , $rows }
    # Out-Null on every void COM call: they emit a bare $null that would land in the row list.
    $view.Execute() | Out-Null
    while ($true) {
        $record = $view.Fetch()
        if ($null -eq $record) { break }
        $row = @()
        for ($i = 1; $i -le $FieldCount; $i++) {
            $row += [string]$record.GetType().InvokeMember(
                'StringData', 'GetProperty', $null, $record, @($i))
        }
        $rows += , $row
    }
    $view.Close() | Out-Null
    # Comma operator: without it an empty result returns $null and one row unrolls to its fields.
    return , $rows
}

# Word Count bit 3 declares the package per-user ONLY: MSI then ignores MSIINSTALLPERUSER, drops
# ALLUSERS, and the per-machine choice silently stops working. WiX correctly leaves it clear.
function Assert-DualScopeSummary([string]$MsiPath) {
    $installer = New-Object -ComObject WindowsInstaller.Installer
    $summary   = $installer.SummaryInformation($MsiPath, 0)
    $wordCount = [int]$summary.Property(15)
    [void][System.Runtime.InteropServices.Marshal]::FinalReleaseComObject($summary)
    [void][System.Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer)
    [GC]::Collect()

    if (($wordCount -band 8) -ne 0) {
        throw ("Word Count is ${wordCount}: bit 3 marks this a per-user-only package, which " +
               "silently disables the per-machine install option.")
    }
    return $wordCount
}

# ICE validation leaves msiexec holding the MSI, so clearing the staging dir can race the
# previous run. Not worth failing a build over; wait the lock out.
function Invoke-FileOpWithRetry([scriptblock]$Action, [string]$What) {
    $attempts = 15
    for ($attempt = 1; $attempt -le $attempts; $attempt++) {
        try {
            & $Action
            return
        }
        catch {
            if ($attempt -eq $attempts) {
                throw "$What failed after $attempts attempts: $($_.Exception.Message)"
            }
            if ($attempt -eq 1) { Write-Host "    waiting for a file lock to clear ($What)" }
            Start-Sleep -Seconds 2
        }
    }
}

# ICE105 is suppressed for the uninstall action, so re-assert by hand everything else it checks;
# version-controlled and specific beats the blanket validator it replaces.
function Test-Ice105Invariants([string]$MsiPath) {
    $bannedDirs = @(
        'AdminToolsFolder', 'CommonAppDataFolder', 'FontsFolder', 'System16Folder',
        'System64Folder', 'SystemFolder', 'TempFolder', 'WindowsFolder', 'WindowsVolume')

    $installer = New-Object -ComObject WindowsInstaller.Installer
    $database  = $installer.OpenDatabase($MsiPath, 0)
    try {
        $problems = @()

        # Identifiers are backtick-quoted ("Key" is reserved), and results are assigned before
        # enumeration, or an empty result enumerates as a single empty row.

        # Root 2 is HKLM; a per-user install cannot write there.
        $registryRows = Get-MsiRows $database 'SELECT `Registry`, `Root`, `Key` FROM `Registry`' 3
        foreach ($row in $registryRows) {
            if ($row[1] -eq '2') { $problems += "HKLM registry row '$($row[0])' ($($row[2]))" }
        }

        foreach ($table in @('ServiceInstall', 'ServiceControl', 'ODBCDataSource', 'MsiAssembly')) {
            $rows = Get-MsiRows $database "SELECT * FROM ``$table``" 1
            if ($rows.Count -gt 0) { $problems += "$table has $($rows.Count) row(s)" }
        }

        $directoryRows = Get-MsiRows $database 'SELECT `Directory` FROM `Directory`' 1
        foreach ($row in $directoryRows) {
            if ($bannedDirs -contains $row[0]) { $problems += "system directory '$($row[0])'" }
        }

        if ($problems.Count -gt 0) {
            throw ("ICE105 invariants broken, so the package is no longer a valid dual-scope " +
                   "installer: " + ($problems -join '; '))
        }
    }
    finally {
        [void][System.Runtime.InteropServices.Marshal]::FinalReleaseComObject($database)
        [void][System.Runtime.InteropServices.Marshal]::FinalReleaseComObject($installer)
    }
}

# --- Version -------------------------------------------------------------------
$igVersion = Get-BuildProp 'IgVersion'
if (-not $igVersion) { throw "Could not read <IgVersion> from $BuildProps" }
$igReleaseType = Get-BuildProp 'IgReleaseType'

# Mirrors the GitHub release tag/asset naming: <version>-<releasetype>, no "v" prefix.
$relLabel = if ($igReleaseType) { "$igVersion-$igReleaseType" } else { $igVersion }

$msiVersion = if ($ProductVersion) { $ProductVersion } else { $igVersion }

# MSI compares only the first three fields and ignores the fourth, which is why the authoring
# sets MajorUpgrade/AllowSameVersionUpgrades. The three that do count still have hard limits.
$versionFields = $msiVersion.Split('.')
if ($versionFields.Count -lt 3) {
    throw "ProductVersion '$msiVersion' needs at least major.minor.build."
}
$fieldLimits = @(255, 255, 65535)
for ($i = 0; $i -lt 3; $i++) {
    $parsed = 0
    if (-not [int]::TryParse($versionFields[$i], [ref]$parsed)) {
        throw "ProductVersion '$msiVersion' field $($i + 1) is not a number."
    }
    if ($parsed -lt 0 -or $parsed -gt $fieldLimits[$i]) {
        throw "ProductVersion '$msiVersion' field $($i + 1) must be 0..$($fieldLimits[$i])."
    }
}

# The fourth field is the only thing separating two builds of one release, so the authoring
# records it and compares it itself; MSI never sees it.
$buildNumber = if ($versionFields.Count -ge 4) { [int]$versionFields[3] } else { 0 }

$upgradeCode = Get-WxiDefine 'IgUpgradeCode'
if (-not $upgradeCode) { throw "Could not read IgUpgradeCode from $WxiFile" }
$productCode = New-DeterministicGuid ([guid]$upgradeCode) "ImageGlass-msi-$Platform-$msiVersion"

# --- Plan ----------------------------------------------------------------------
$rid         = "win-$Platform"
$msbuildPlat = 'x64'
$publishDir  = Join-Path $WorkspaceDir "__artifacts\publish\$rid"
$stagingDir  = Join-Path $DistDir "$rid-msi"
$payloadDir  = Join-Path $stagingDir 'ImageGlass'
$objDir      = Join-Path $stagingDir 'obj'
$wixPdb      = Join-Path $stagingDir 'ImageGlass.wixpdb'
$stagedMsi   = Join-Path $stagingDir "ImageGlass_${relLabel}_${rid}.msi"
# Inspected/validated in place of the real thing; see step 5. Lives outside the staging dir so a
# lingering msiexec handle cannot block the cleanup.
$checkMsi    = Join-Path ([System.IO.Path]::GetTempPath()) "ig-msi-check-$PID.msi"
$outMsi      = Join-Path $DistDir "ImageGlass_${relLabel}_${rid}.msi"

$script:doSign = [bool]$Sign
if ($script:doSign -and -not (Test-SigningCert)) {
    Write-Warning "No signing certificate found: building an UNSIGNED installer."
    $script:doSign = $false
}
$signtool = if ($script:doSign) { Find-SdkTool 'signtool.exe' } else { '' }

# Fail on a missing/wrong toolchain before the multi-minute publish, not after it.
$wix = Find-WixTool
$wixFound = Assert-WixVersion $wix
Assert-WixUiExtension $wix

Write-Host "==> Packing ImageGlass $igVersion as MSI ($Platform)"
Write-Host "    Version     : $msiVersion (ProductVersion)"
Write-Host "    ProductCode : {$($productCode.ToString().ToUpperInvariant())}"
Write-Host "    UpgradeCode : {$upgradeCode}"
Write-Host "    Signed      : $(if ($script:doSign) { 'yes (payload binaries + the .msi)' } else { 'no' })"
Write-Host "    Compression : $CompressionLevel"
Write-Host "    Validation  : $(if ($SkipValidation) { 'skipped' } else { "on (suppressing $($SuppressIce -join ', '))" })"
Write-Host "    Output      : $outMsi"
Write-Host "    wix         : $wix ($wixFound)"
if ($script:doSign) { Write-Host "    signtool    : $signtool" }

# --- 1. Publish a fresh self-contained AOT build -------------------------------
if ($SkipPublish -and (Test-Path (Join-Path $publishDir 'ImageGlass.exe'))) {
    Write-Host "    reusing publish output: $publishDir"
}
else {
    Write-Host "    publishing $rid (Release, AOT, self-contained)"
    if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
    & dotnet publish $ProjectFile -c Release -r $rid -p:Platform=$msbuildPlat -o $publishDir
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $rid (exit $LASTEXITCODE)." }
    # Bundle the shared app assets (themes, credits, etc.), mirroring the publish-win tasks.
    Copy-Item -Path (Join-Path $AppExtras '*') -Destination $publishDir -Recurse -Force
}
if (-not (Test-Path (Join-Path $publishDir 'ImageGlass.exe'))) {
    throw "Publish did not produce ImageGlass.exe in $publishDir"
}

# Sweep any check copy a previous run was still locked out of.
Get-ChildItem ([System.IO.Path]::GetTempPath()) -Filter 'ig-msi-check-*.msi' -File -ErrorAction SilentlyContinue |
    Remove-Item -Force -ErrorAction SilentlyContinue

# --- 2. Stage the payload ------------------------------------------------------
# The payload sits one level down so the obj/ and .wixpdb siblings are never harvested.
if (Test-Path $stagingDir) {
    Invoke-FileOpWithRetry { Remove-Item $stagingDir -Recurse -Force } "Clearing $stagingDir"
}
New-Item -ItemType Directory -Path $payloadDir -Force | Out-Null
Copy-Item -Path (Join-Path $publishDir '*') -Destination $payloadDir -Recurse -Force

# Drop debug symbols: they are ~290 MB of the publish dir and are not part of the product.
Get-ChildItem -Path $payloadDir -Recurse -Include '*.pdb' -File -ErrorAction SilentlyContinue |
    Remove-Item -Force

# The exportable Store license ships in the msstore MSIX only. Refuse to build rather than
# quietly strip it: the failure mode is a leaked Pro license, not a cosmetic one.
$storeLicenseDir = Join-Path $payloadDir '_store'
if (Test-Path $storeLicenseDir) {
    throw "Refusing to build: '$storeLicenseDir' exists. The store license ships only in the msstore MSIX."
}

# An installed copy carrying the portable marker cannot start: Program Files is not writable and
# ConfigMode reports the error and quits instead of falling back to %LocalAppData%.
$portablePath = Join-Path $payloadDir $PortableMarker
if (Test-Path $portablePath) {
    throw "Refusing to build: '$portablePath' exists. An installed copy with $PortableMarker cannot start."
}

# --- 3. Sign the payload binaries ----------------------------------------------
# Must precede the build: wix bakes file sizes and MsiFileHash rows in and cabs the bytes.
if ($script:doSign) {
    $binaries = Get-ChildItem -Path $payloadDir -Recurse -Include '*.exe', '*.dll' -File |
        Select-Object -ExpandProperty FullName
    Write-Host "    signing $($binaries.Count) payload binary file(s)"
    if (-not (Invoke-SignTool -SignTool $signtool -Files $binaries)) {
        Write-Warning "Could not sign payload binaries: the installer will be left UNSIGNED."
        $script:doSign = $false
    }
}

# --- 4. Build the MSI ----------------------------------------------------------
Write-Host ''
Write-Host '==> Building the installer'
New-Item -ItemType Directory -Path $DistDir -Force | Out-Null

$wixArgs = @(
    'build'
) + $WxsFiles + @(
    '-arch', $Platform,
    '-ext', "$WixUiExt/$WixVersion",       # always versioned; a bare -ext takes the newest cached
    '-culture', 'en-US',
    '-loc', $WxlFile,
    '-d', "ProductVersion=$msiVersion",
    '-d', "BuildNumber=$buildNumber",
    '-d', "ProductCode=$($productCode.ToString().ToUpperInvariant())",
    '-d', "PayloadDir=$payloadDir",
    '-d', "Cabinet=$CompressionLevel",
    '-intermediatefolder', $objDir,
    '-pdb', $wixPdb,                       # keeps the .wixpdb out of the deliverable folder
    '-sw1076',                             # ICE61 is expected; see AllowSameVersionUpgrades
    '-nologo',
    '-o', $stagedMsi
)
& $wix @wixArgs
if ($LASTEXITCODE -ne 0) { throw "wix build failed (exit $LASTEXITCODE)." }
Write-Host "    built: $stagedMsi"

# --- 5. Inspect and validate a throwaway copy ----------------------------------
# Any MSI-API read (COM or ICE) leaves msiexec holding the file for minutes, so none of them ever
# touch the staged build or the deliverable; the temp copy is the only thing left locked.
Copy-Item -LiteralPath $stagedMsi -Destination $checkMsi -Force

$wordCount = Assert-DualScopeSummary $checkMsi
Write-Host "    summary WordCount: $wordCount (bit 3 clear, so both install scopes stay available)"

Test-Ice105Invariants $checkMsi
Write-Host '    ICE105 invariants re-asserted by hand (no HKLM rows, services or system dirs)'

if ($SkipValidation) {
    Write-Host '    skipping ICE validation (-SkipValidation)'
}
else {
    Write-Host "    validating (suppressing $($SuppressIce -join ', '))"
    $valArgs = @('msi', 'validate', $checkMsi, '-pdb', $wixPdb, '-nologo')
    foreach ($ice in $SuppressIce) { $valArgs += @('-sice', $ice) }
    & $wix @valArgs
    if ($LASTEXITCODE -ne 0) { throw "wix msi validate failed (exit $LASTEXITCODE)." }
}
Remove-Item $checkMsi -Force -ErrorAction SilentlyContinue

# --- 6. Publish the deliverable ------------------------------------------------
# Copy, not move, so a stale lock on either end can never strand the build.
Invoke-FileOpWithRetry { Copy-Item -LiteralPath $stagedMsi -Destination $outMsi -Force } "Writing $outMsi"

# --- 7. Sign the installer -----------------------------------------------------
# Last mutation of the deliverable: nothing may touch it after this.
if ($script:doSign) {
    Write-Host '    signing the .msi'
    if (-not (Invoke-SignTool -SignTool $signtool -Files @($outMsi))) {
        Write-Warning 'Could not sign the installer: it will be left UNSIGNED.'
        $script:doSign = $false
    }
    else {
        & $signtool verify /pa $outMsi | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "signtool verify failed (exit $LASTEXITCODE)." }
    }
}

$msiSizeMb = [math]::Round((Get-Item $outMsi).Length / 1MB, 1)

# --- 8. Clean up staging -------------------------------------------------------
# Nothing here was ever opened through an MSI API, so this just works. The publish dir stays:
# it is shared with the msix/zip packers and is what -SkipPublish reuses.
Remove-Item $stagingDir -Recurse -Force -ErrorAction SilentlyContinue
if (Test-Path $stagingDir) { Write-Warning "Could not remove '$stagingDir'." }

# --- Done ----------------------------------------------------------------------
Write-Host ''
Write-Host 'Done.'
Write-Host "  Package : $outMsi ($msiSizeMb MB)"
Write-Host "  Version : $msiVersion  (ProductCode {$($productCode.ToString().ToUpperInvariant())})"
if ($Sign -and -not $script:doSign) {
    Write-Host '  Signed  : no (no signing certificate was found)'
    Write-Host '  Next    : sign the installer and repack before publishing the release.'
}
elseif ($script:doSign) {
    Write-Host '  Signed  : yes (payload binaries + the .msi)'
    Write-Host '  Next    : upload to the GitHub release for this version.'
}
