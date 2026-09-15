#Requires -Version 7.0
<#
.SYNOPSIS
    Build (and optionally sign) an MSIX of ImageGlass.Win32 — one .msix per
    architecture, or a single x64+arm64 .msixbundle (-Bundle).

.DESCRIPTION
    Produces two flavours of MSIX from the same source, selected by the -Sign switch:

      * MSSTORE   (default)  -> for the Microsoft Store.
            The Store re-signs the package itself, so it is built UNSIGNED and
            carries the Store-reserved Identity (Name + Publisher) and the
            Store-supplied artwork (appxmanifest/Assets-msstore).
            Output: __artifacts/bundle/ImageGlass_<label>_win-<arch>-msstore.msix

      * SIGNED    (-Sign)    -> for direct download / GitHub Releases (sideload).
            Every payload .exe / .dll is Authenticode-signed, then the whole .msix
            is signed. The package Identity/Publisher is set to the EXACT Subject of
            the signing certificate (a hard MSIX requirement), a plain Identity Name
            is used, and the artwork is rendered from the app logo
            (appxmanifest/Assets-signed).
            If NO signing certificate is found, the package is still built (same
            identity/artwork) but left UNSIGNED — sign it later before publishing.
            Output: __artifacts/bundle/ImageGlass_<label>_win-<arch>.msix

    File type associations differ per flavour, because Windows takes the file icon from the
    package manifest for every type the package claims, outranking the classic DefaultIcon:
    -UnvirtualizedResources (sideload) declares NO association, leaving the app's own HKCU
    registration and the _ext_icons folder in charge; the Store flavour, where that registration
    is virtualized away, declares one association per format with its own uap:Logo.

    The package version differs per flavour, because only the Store constrains it.
    Sideload ships <IgVersion> verbatim (e.g. 10.0.6.906), matching the MSI and the
    About box; msstore must leave the 4th (revision) part 0, which the Store reserves,
    so it packs Major.Minor (from <IgBundleShortVersion>) . <IgBundleBuild> . 0 —
    e.g. 10.0.906.0. Windows refuses to install a package older than the installed
    one, so a scheme change must not lower the version an existing install carries.

    With -Bundle, both x64 and arm64 are built and packed into a single
    .msixbundle (Windows installs the matching architecture). The per-arch packages
    are payload-signed but NOT package-signed; only the .msixbundle is signed.

    The script publishes a fresh self-contained AOT build first (so the package
    always matches the current source and the version baked into the binary),
    stages the payload under an "ImageGlass\" subfolder, generates AppxManifest.xml
    from the template, packs with makeappx, and (when signing) signs with signtool.
    makeappx.exe / makepri.exe / signtool.exe are auto-located in the latest
    Windows 10/11 SDK.

.PARAMETER Platform
    Target architecture: x64 (default) or arm64. Ignored when -Bundle is used
    (a bundle always contains both).

.PARAMETER Bundle
    Build a single x64+arm64 .msixbundle instead of one .msix per architecture.

.PARAMETER Sign
    Build the signed (sideload / GitHub) flavour. The package is signed when a
    certificate is available; if none is found it is built UNSIGNED (a warning is
    printed). Omit for the msstore build.

.PARAMETER CertSubject
    Substring of the code-signing certificate Subject to select it from the
    Current User / Local Machine "My" store (passed to signtool /n). Ignored when
    -CertFile is supplied. Default: "Duong Dieu Phap".

.PARAMETER CertFile
    Path to a PFX certificate to sign with instead of a store certificate.

.PARAMETER CertPassword
    Password for -CertFile (if any).

.PARAMETER TimestampUrl
    RFC-3161 timestamp server. Default: http://timestamp.sectigo.com

.PARAMETER PackageVersion
    Override the package version for either flavour. Defaults to <IgVersion> when
    signing (sideload) and <Major>.<Minor>.<IgBundleBuild>.0 for msstore, both read
    from Directory.Build.props.

.PARAMETER SkipPublish
    Reuse the existing __artifacts/publish/win-<arch> output instead of re-publishing
    (faster iteration; the package may not reflect uncommitted source changes).

.EXAMPLE
    pwsh __assets/win/script-pack-win-msix.ps1 -Platform x64
    # Unsigned x64 package for the Microsoft Store (msstore).

.EXAMPLE
    pwsh __assets/win/script-pack-win-msix.ps1 -Platform arm64 -Sign
    # Signed arm64 package for GitHub Releases (cert selected by Subject).

.EXAMPLE
    pwsh __assets/win/script-pack-win-msix.ps1 -Platform x64 -Sign -CertFile C:\ig.pfx -CertPassword hunter2

.EXAMPLE
    pwsh __assets/win/script-pack-win-msix.ps1 -Bundle -Sign
    # Signed x64+arm64 .msixbundle for GitHub Releases.

.EXAMPLE
    pwsh __assets/win/script-pack-win-msix.ps1 -Bundle
    # Unsigned x64+arm64 .msixbundle for the Microsoft Store.
#>

[CmdletBinding()]
param(
    [ValidateSet('x64', 'arm64')]
    [string]$Platform = 'x64',

    [switch]$Sign,

    [string]$CertSubject = 'Duong Dieu Phap',
    [string]$CertFile = '',
    [string]$CertPassword = '',
    [string]$TimestampUrl = 'http://timestamp.sectigo.com',

    [string]$PackageVersion = '',

    # msstore identity (unsigned build) — reserved name assigned by Partner Center.
    [string]$MsStoreIdentityName = '9662DuongDieuPhap.ImageGlass',
    [string]$MsStorePublisher = 'CN=29F1B9EC-D220-4DC3-BEDB-01A9CCA51904',

    # Sideload identity (signed build) — Publisher is overwritten with the cert Subject.
    [string]$SideloadIdentityName = 'DuongDieuPhap.ImageGlass',
    [string]$PublisherDisplayName = 'Duong Dieu Phap',

    [switch]$SkipPublish,

    # Pack x64 + arm64 into a single .msixbundle instead of one .msix per arch.
    # -Platform is ignored in this mode.
    [switch]$Bundle,

    # Opt out of resources virtualization (unvirtualizedResources) so classic file-association
    # registration + custom ext icons reach the real HKCU, and drop the manifest file type
    # associations that would otherwise own the icon. Sideload/GitHub only; NOT the Store.
    [switch]$UnvirtualizedResources,

    # Signed license bundled into the msstore payload so a Store customer can export it for their
    # macOS/Linux machines. Required for the msstore flavour, never shipped in the signed one.
    # Defaults to the single *.iglicense.json in __artifacts\store-license (git-ignored).
    [string]$StoreLicenseFile = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# --- Paths ---------------------------------------------------------------------
$WorkspaceDir = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$ProjectFile  = Join-Path $WorkspaceDir 'ImageGlass.Win32\ImageGlass.Win32.csproj'
$BuildProps   = Join-Path $WorkspaceDir 'Directory.Build.props'
$ManifestTpl  = Join-Path $PSScriptRoot 'appxmanifest\AppxManifest.xml'
# Signed build uses logos rendered from the app logo; msstore uses the
# Store-supplied artwork. (Regenerate the signed set with script-generate-msix-assets.ps1.)
$AssetsDir    = Join-Path $PSScriptRoot ($Sign ? 'appxmanifest\Assets-signed' : 'appxmanifest\Assets-msstore')
$AppExtras    = Join-Path $WorkspaceDir '__assets\__app'
$DistDir      = Join-Path $WorkspaceDir '__artifacts\bundle'

# --- Helpers -------------------------------------------------------------------

# Locate a Windows SDK tool (makeappx.exe / signtool.exe), preferring the newest
# SDK and an x64 host build, falling back to whatever is already on PATH.
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

    throw "Could not find $Name. Install the Windows 10/11 SDK (includes makeappx & signtool)."
}

# Read a single <Tag>value</Tag> from Directory.Build.props.
function Get-BuildProp([string]$Tag) {
    $m = Select-String -Path $BuildProps -Pattern "<$Tag>(.*?)</$Tag>" | Select-Object -First 1
    if ($m) { return $m.Matches[0].Groups[1].Value.Trim() }
    return ''
}

# MSIX versions are exactly 4 parts, each 0..65535, with a non-zero major; a shorter one is
# padded rather than rejected so a 3-part <IgVersion> still packs.
function Assert-PackageVersion([string]$Version) {
    $parts = $Version.Split('.')
    if ($parts.Count -lt 3 -or $parts.Count -gt 4) {
        throw "Package version '$Version' must have 3 or 4 numeric parts."
    }
    if ($parts.Count -eq 3) { $parts += '0' }

    for ($i = 0; $i -lt 4; $i++) {
        $parsed = 0
        if (-not [int]::TryParse($parts[$i], [ref]$parsed)) {
            throw "Package version '$Version' part $($i + 1) is not a number."
        }
        if ($parsed -lt 0 -or $parsed -gt 65535) {
            throw "Package version '$Version' part $($i + 1) must be 0..65535."
        }
        $parts[$i] = "$parsed"
    }
    if ([int]$parts[0] -eq 0) { throw "Package version '$Version' major part cannot be 0." }

    return ($parts -join '.')
}

# Explorer's file-icon sizes, emitted as MRT "targetsize-N" variants of each extension logo.
$ExtIconSizes = @(16, 32, 48, 96, 256)

# Read the extensions from Const.IMAGE_FORMATS so the manifest cannot drift from the app.
function Get-SupportedExtensions {
    $constFile = Join-Path $WorkspaceDir 'ImageGlass.Lib\Common\Types\Const.cs'
    $m = Select-String -Path $constFile -Pattern 'IMAGE_FORMATS\s*=\s*"([^"]+)"' | Select-Object -First 1
    if (-not $m) { throw "Could not read IMAGE_FORMATS from $constFile" }

    return $m.Matches[0].Groups[1].Value.Split(';', [StringSplitOptions]::RemoveEmptyEntries)
}

# Write one .ico as the PNG variants MRT resolves a uap:Logo against. A 256px frame is stored
# PNG-compressed inside the .ico, so it is copied out verbatim instead of re-encoded.
function Export-ExtIconPngs([string]$IcoPath, [string]$OutDir, [string]$BaseName) {
    $bytes = [System.IO.File]::ReadAllBytes($IcoPath)
    $count = [BitConverter]::ToUInt16($bytes, 4)

    foreach ($size in $ExtIconSizes) {
        $outFile = Join-Path $OutDir "$BaseName.targetsize-$size.png"
        $rawPng = $null

        for ($i = 0; $i -lt $count; $i++) {
            $entry = 6 + $i * 16
            $width = $bytes[$entry]
            if ($width -eq 0) { $width = 256 }
            if ($width -ne $size) { continue }

            $length = [BitConverter]::ToUInt32($bytes, $entry + 8)
            $offset = [BitConverter]::ToUInt32($bytes, $entry + 12)
            if ($bytes[$offset] -eq 0x89 -and $bytes[$offset + 1] -eq 0x50) {
                $rawPng = New-Object byte[] $length
                [Array]::Copy($bytes, $offset, $rawPng, 0, $length)
            }
            break
        }

        if ($rawPng) {
            [System.IO.File]::WriteAllBytes($outFile, $rawPng)
            continue
        }

        # BMP-encoded frame (or no exact size): let GDI+ pick the closest frame and re-encode
        $icon = [System.Drawing.Icon]::new($IcoPath, $size, $size)
        try {
            $bmp = $icon.ToBitmap()
            try { $bmp.Save($outFile, [System.Drawing.Imaging.ImageFormat]::Png) }
            finally { $bmp.Dispose() }
        }
        finally { $icon.Dispose() }
    }

    # MRT needs one candidate without a targetsize qualifier to fall back on
    Copy-Item (Join-Path $OutDir "$BaseName.targetsize-256.png") (Join-Path $OutDir "$BaseName.png") -Force
}

# Build the <uap:Extension> file-type-association blocks and stage their logos. A claimed type takes
# its icon AND type name from the package, outranking the classic registration, so only the Store
# flavour claims any; keep DisplayName in step with Win32DefaultAppApi.GetFriendlyTypeName.
function New-FileTypeAssociationXml([string]$StagingDir) {
    if ($UnvirtualizedResources) { return '' }

    Add-Type -AssemblyName System.Drawing

    $iconSrcDir = Join-Path $AppExtras '_ext_icons'
    $iconOutDir = Join-Path $StagingDir 'Assets\ExtIcons'
    New-Item -ItemType Directory -Path $iconOutDir -Force | Out-Null

    $extensions = Get-SupportedExtensions
    $blocks = [System.Text.StringBuilder]::new()
    $unbranded = 0
    $indent = ' ' * 8

    # one association per format, never a grouped one: DisplayName is per association, and a group
    # would label every format in it identically (the classic-registration bug, in manifest form)
    foreach ($ext in $extensions) {
        $baseName = $ext.TrimStart('.').ToUpperInvariant()
        $icoPath = Join-Path $iconSrcDir "$baseName.ico"
        $hasIcon = Test-Path $icoPath

        if ($hasIcon) { Export-ExtIconPngs -IcoPath $icoPath -OutDir $iconOutDir -BaseName $baseName }
        else { $unbranded++ }

        [void]$blocks.AppendLine("$indent<uap:Extension Category=`"windows.fileTypeAssociation`" EntryPoint=`"Windows.FullTrustApplication`" Executable=`"ImageGlass\ImageGlass.exe`">")
        [void]$blocks.AppendLine("$indent  <uap:FileTypeAssociation Name=`"imageglass$ext`">")

        # Explorer's Type column; without it the shell falls back to a bare "<EXT> File"
        [void]$blocks.AppendLine("$indent    <uap:DisplayName>ImageGlass $baseName File</uap:DisplayName>")

        if ($hasIcon) {
            [void]$blocks.AppendLine("$indent    <uap:Logo>Assets\ExtIcons\$baseName.png</uap:Logo>")
        }

        [void]$blocks.AppendLine("$indent    <uap:SupportedFileTypes>")
        [void]$blocks.AppendLine("$indent      <uap:FileType>$ext</uap:FileType>")
        [void]$blocks.AppendLine("$indent    </uap:SupportedFileTypes>")
        [void]$blocks.AppendLine("$indent  </uap:FileTypeAssociation>")
        [void]$blocks.AppendLine("$indent</uap:Extension>")
    }

    Write-Host "    file type associations: $($extensions.Count) format(s), $unbranded without a bundled icon"

    return $blocks.ToString().TrimEnd()
}

# Locate the signed license to bundle into the msstore payload. -StoreLicenseFile wins; otherwise
# take the single *.iglicense.json in __artifacts\store-license, which .gitignore keeps out of the
# repo. Throws rather than shipping a Store package a customer cannot export a license from.
function Resolve-StoreLicense {
    if ($StoreLicenseFile) {
        if (-not (Test-Path -LiteralPath $StoreLicenseFile -PathType Leaf)) {
            throw "-StoreLicenseFile not found: $StoreLicenseFile"
        }
        return (Resolve-Path -LiteralPath $StoreLicenseFile).Path
    }

    $defaultDir = Join-Path $WorkspaceDir '__artifacts\store-license'
    $hits = @()
    if (Test-Path $defaultDir) {
        $hits = @(Get-ChildItem -Path $defaultDir -Filter '*.iglicense.json' -File -ErrorAction SilentlyContinue)
    }

    if ($hits.Count -eq 0) {
        throw "No store license to bundle. Put the signed <licenseId>.iglicense.json in '$defaultDir', or pass -StoreLicenseFile."
    }
    if ($hits.Count -gt 1) {
        throw "Found $($hits.Count) *.iglicense.json in '$defaultDir'. Pass -StoreLicenseFile to choose one."
    }

    return $hits[0].FullName
}

# Read the licenseId out of a license file, for the build log.
function Get-LicenseId([string]$Path) {
    try { return (Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json).licenseId }
    catch { return '(unreadable)' }
}

# Find a usable signing certificate and report its EXACT Subject DN (needed for
# the manifest Publisher, which must match the signature byte-for-byte) and which
# store it lives in (so signtool searches the same one). Returns a hashtable
# @{ Subject; Machine } or $null when none is found — the caller then builds an
# UNSIGNED package rather than failing.
$script:UseMachineStore = $false
function Resolve-SigningCert {
    if ($CertFile) {
        if (-not (Test-Path $CertFile)) {
            Write-Warning "Certificate file not found: $CertFile"
            return $null
        }
        try {
            $cert = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($CertFile, $CertPassword)
            return @{ Subject = $cert.Subject; Machine = $false }
        }
        catch {
            Write-Warning "Could not load certificate '$CertFile': $($_.Exception.Message)"
            return $null
        }
    }
    foreach ($store in @(
            @{ Path = 'Cert:\CurrentUser\My';  Machine = $false },
            @{ Path = 'Cert:\LocalMachine\My'; Machine = $true })) {
        $cert = Get-ChildItem $store.Path -CodeSigningCert -ErrorAction SilentlyContinue |
            Where-Object { $_.Subject -like "*$CertSubject*" -and $_.HasPrivateKey } |
            Select-Object -First 1
        if ($cert) { return @{ Subject = $cert.Subject; Machine = $store.Machine } }
    }
    return $null
}

# Sign one or more files with signtool (Authenticode, SHA-256, timestamped).
# Returns $true on success, $false on failure (the caller decides whether to
# continue UNSIGNED) — never throws.
function Invoke-SignTool([string]$SignTool, [string[]]$Files) {
    if ($Files.Count -eq 0) { return $true }
    $common = @('sign', '/fd', 'SHA256', '/tr', $TimestampUrl, '/td', 'SHA256')
    if ($CertFile) {
        $common += @('/f', $CertFile)
        if ($CertPassword) { $common += @('/p', $CertPassword) }
    }
    else {
        $common += @('/n', $CertSubject, '/a')
        # signtool /n defaults to the CurrentUser store; switch to the machine
        # store when that is where the certificate was found.
        if ($script:UseMachineStore) { $common += '/sm' }
    }
    & $SignTool @common @Files
    return ($LASTEXITCODE -eq 0)
}

# Build ONE architecture's .msix (publish -> stage -> manifest -> payload-sign ->
# resource index -> pack) and write it to $OutMsixPath. The package itself is NOT
# signed here — the caller signs the final artifact (the .msix in single mode, or
# the .msixbundle in bundle mode). Reads the flavour-level $identityName,
# $publisher, $pkgVersion, $script:doSign and the located SDK tools from script scope.
function New-MsixPackage([string]$Platform, [string]$OutMsixPath) {
    $rid         = "win-$Platform"
    $msbuildPlat = if ($Platform -eq 'x64') { 'x64' } else { 'ARM64' }
    $publishDir  = Join-Path $WorkspaceDir "__artifacts\publish\$rid"
    $stagingDir  = Join-Path $WorkspaceDir "__artifacts\bundle\$rid-msix"
    $payloadDir  = Join-Path $stagingDir 'ImageGlass'

    Write-Host ''
    Write-Host "==> [$Platform] Building MSIX package"

    # 1. Publish a fresh self-contained AOT build.
    if ($SkipPublish -and (Test-Path (Join-Path $publishDir 'ImageGlass.exe'))) {
        Write-Host "    reusing publish output: $publishDir"
    }
    else {
        Write-Host "    publishing $rid (Release, AOT, self-contained)"
        if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
        & dotnet publish $ProjectFile -c Release -r $rid -p:Platform=$msbuildPlat -o $publishDir
        if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $rid (exit $LASTEXITCODE)." }
        # Bundle the shared app assets (themes, credits, etc.) — mirrors the publish-win tasks.
        Copy-Item -Path (Join-Path $AppExtras '*') -Destination $publishDir -Recurse -Force
    }
    if (-not (Test-Path (Join-Path $publishDir 'ImageGlass.exe'))) {
        throw "Publish did not produce ImageGlass.exe in $publishDir"
    }

    # 2. Stage the layout:  <staging>\AppxManifest.xml + \Assets\* + \ImageGlass\*
    if (Test-Path $stagingDir) { Remove-Item $stagingDir -Recurse -Force }
    New-Item -ItemType Directory -Path $payloadDir -Force | Out-Null
    Copy-Item -Path (Join-Path $publishDir '*') -Destination $payloadDir -Recurse -Force
    # Drop debug symbols — they bloat the package and are not part of the product.
    Get-ChildItem -Path $payloadDir -Recurse -Include '*.pdb' -File -ErrorAction SilentlyContinue |
        Remove-Item -Force

    # Without the virtualization opt-out the app can never register these, so its file icons come
    # from the manifest logos instead and the .ico set is dead weight.
    if (-not $UnvirtualizedResources) {
        $payloadExtIcons = Join-Path $payloadDir '_ext_icons'
        if (Test-Path $payloadExtIcons) { Remove-Item $payloadExtIcons -Recurse -Force }
    }

    # Store only: stage the exportable license in _store\, a subfolder the app's license scan never
    # looks in, so it can only leave the package deliberately.
    $storeLicenseDir = Join-Path $payloadDir '_store'
    if ($Sign) {
        # a stale -SkipPublish reuse must never leak the store license into the signed package
        if (Test-Path $storeLicenseDir) {
            throw "Refusing to build the signed package: '$storeLicenseDir' exists. The store license ships only in the msstore flavour."
        }
    }
    else {
        if (Test-Path $storeLicenseDir) { Remove-Item $storeLicenseDir -Recurse -Force }
        New-Item -ItemType Directory -Path $storeLicenseDir -Force | Out-Null
        Copy-Item -LiteralPath $script:storeLicensePath -Destination $storeLicenseDir -Force
    }

    Copy-Item -Path $AssetsDir -Destination (Join-Path $stagingDir 'Assets') -Recurse -Force

    # 3. Generate AppxManifest.xml from the template (UTF-8 BOM, as the SDK expects).
    # -UnvirtualizedResources (GitHub/sideload) opts out of resources virtualization; Store stays virtualized
    $regVirt   = if ($UnvirtualizedResources) { '<desktop6:RegistryWriteVirtualization>disabled</desktop6:RegistryWriteVirtualization>' } else { '' }
    $unvirtCap = if ($UnvirtualizedResources) { '<rescap:Capability Name="unvirtualizedResources" />' } else { '' }

    $fileTypes = New-FileTypeAssociationXml -StagingDir $stagingDir

    $manifest = Get-Content -Path $ManifestTpl -Raw
    $manifest = $manifest.Replace('{{IDENTITY_NAME}}', $identityName).
                          Replace('{{PUBLISHER}}', $publisher).
                          Replace('{{PUBLISHER_DISPLAY_NAME}}', $PublisherDisplayName).
                          Replace('{{VERSION}}', $pkgVersion).
                          Replace('{{ARCH}}', $Platform).
                          Replace('{{REGISTRY_VIRTUALIZATION}}', $regVirt).
                          Replace('{{UNVIRTUALIZED_CAPABILITY}}', $unvirtCap).
                          Replace('        {{FILE_TYPE_ASSOCIATIONS}}', $fileTypes)
    $utf8Bom = [System.Text.UTF8Encoding]::new($true)
    [System.IO.File]::WriteAllText((Join-Path $stagingDir 'AppxManifest.xml'), $manifest, $utf8Bom)

    # 4. Sign payload binaries so installed .exe/.dll carry a trust chain.
    if ($script:doSign) {
        $binaries = Get-ChildItem -Path $payloadDir -Recurse -Include '*.exe', '*.dll' -File |
            Select-Object -ExpandProperty FullName
        Write-Host "    signing $($binaries.Count) payload binary file(s)"
        if (-not (Invoke-SignTool -SignTool $signtool -Files $binaries)) {
            Write-Warning "Could not sign payload binaries — the package will be left UNSIGNED."
            $script:doSign = $false
        }
    }

    # 5. Build the resource index so the manifest's unqualified logo names resolve
    #    to the scale-qualified assets (and Windows picks the right tile per DPI).
    $priConfig = Join-Path (Split-Path $stagingDir) "$rid-msix.priconfig.xml"
    $manOut    = Join-Path $stagingDir 'AppxManifest.xml'
    $priOut    = Join-Path $stagingDir 'resources.pri'
    if (Test-Path $priConfig) { Remove-Item $priConfig -Force }
    & $makepri createconfig /cf $priConfig /dq en-US /o
    if ($LASTEXITCODE -ne 0) { throw "makepri createconfig failed (exit $LASTEXITCODE)." }
    & $makepri new /pr $stagingDir /cf $priConfig /mn $manOut /of $priOut /o
    if ($LASTEXITCODE -ne 0) { throw "makepri new failed (exit $LASTEXITCODE)." }

    # 6. Pack the .msix.
    New-Item -ItemType Directory -Path (Split-Path $OutMsixPath) -Force | Out-Null
    if (Test-Path $OutMsixPath) { Remove-Item $OutMsixPath -Force }
    & $makeappx pack /o /d $stagingDir /p $OutMsixPath
    if ($LASTEXITCODE -ne 0) { throw "makeappx pack failed for $rid (exit $LASTEXITCODE)." }
    Write-Host "    packed: $OutMsixPath"
}

# --- Version -------------------------------------------------------------------
$igVersion = Get-BuildProp 'IgVersion'
if (-not $igVersion) { throw "Could not read <IgVersion> from $BuildProps" }
$igReleaseType = Get-BuildProp 'IgReleaseType'

$relLabel = if ($igReleaseType) { "$igVersion-$igReleaseType" } else { $igVersion }

# The package version scheme differs per flavour, because only the Store constrains it:
# it reserves the 4th (revision) part and requires a 0 there, so the msstore flavour has
# 3 usable parts for a 4-part <IgVersion> and keeps Major.Minor + IgBundleBuild
# (10.0.6.906 -> 10.0.906.0), dropping the patch. Windows itself has no such rule, so the
# sideload flavour ships <IgVersion> verbatim and matches the MSI and the About box.
if ($PackageVersion) {
    $pkgVersion = $PackageVersion
}
elseif ($Sign) {
    $pkgVersion = $igVersion
}
else {
    $shortVer = Get-BuildProp 'IgBundleShortVersion'
    if (-not $shortVer) { $shortVer = $igVersion }
    $bundleBuild = Get-BuildProp 'IgBundleBuild'
    if (-not $bundleBuild) { $bundleBuild = '0' }

    $sp    = $shortVer.Split('.')
    $major = $sp[0]
    $minor = if ($sp.Count -gt 1) { $sp[1] } else { '0' }
    $pkgVersion = "$major.$minor.$bundleBuild.0"
}

# A malformed version only surfaces as an opaque makeappx failure minutes into the publish.
$pkgVersion = Assert-PackageVersion $pkgVersion

# --- Identity / publisher per flavour -----------------------------------------
# (Flavour-level: identical across architectures; only ProcessorArchitecture, set
# inside New-MsixPackage, differs — which is exactly what a .msixbundle requires.)
if ($Sign) {
    $identityName = $SideloadIdentityName
    $cert         = Resolve-SigningCert
    if ($cert) {
        $publisher              = $cert.Subject
        $script:doSign          = $true
        $script:UseMachineStore = $cert.Machine
    }
    else {
        # No usable certificate — build the GitHub package(s) UNSIGNED. Use a
        # placeholder Publisher; they must be signed before they can install.
        $publisher     = "CN=$PublisherDisplayName"
        $script:doSign = $false
        Write-Warning "No signing certificate found — building an UNSIGNED package."
    }
}
else {
    $identityName  = $MsStoreIdentityName
    $publisher     = $MsStorePublisher
    $script:doSign = $false
}

# Resolve the bundled license up front, so a missing one fails before the long publish.
$script:storeLicensePath = ''
if (-not $Sign) {
    $script:storeLicensePath = Resolve-StoreLicense
}

# --- Output artifact name ------------------------------------------------------
$ext         = if ($Bundle) { 'msixbundle' } else { 'msix' }
$archTag     = if ($Bundle) { 'win' } else { "win-$Platform" }
$storeSuffix = if ($Sign) { '' } else { '-msstore' }
$outArtifact = Join-Path $DistDir "ImageGlass_${relLabel}_${archTag}${storeSuffix}.$ext"

$flavourLabel = if (-not $Sign) { 'MSSTORE (unsigned, Microsoft Store)' }
                elseif ($script:doSign) { 'SIGNED (sideload / GitHub)' }
                else { 'GitHub (UNSIGNED — no certificate found)' }
Write-Host "==> Packing ImageGlass $igVersion as $(if ($Bundle) { 'MSIXBUNDLE (x64 + arm64)' } else { "MSIX ($Platform)" })"
Write-Host "    Flavour     : $flavourLabel"
Write-Host "    Identity    : $identityName"
Write-Host "    Publisher   : $publisher"
Write-Host "    Version     : $pkgVersion"
Write-Host "    Assets      : $(Split-Path $AssetsDir -Leaf)"
if (-not $Sign) {
    Write-Host "    License     : $(Get-LicenseId $script:storeLicensePath) ($script:storeLicensePath)"
}
Write-Host "    Output      : $outArtifact"

# --- Locate SDK tools ----------------------------------------------------------
$makeappx = Find-SdkTool 'makeappx.exe'
$makepri  = Find-SdkTool 'makepri.exe'
$signtool = if ($script:doSign) { Find-SdkTool 'signtool.exe' } else { '' }
Write-Host "    makeappx    : $makeappx"
Write-Host "    makepri     : $makepri"
if ($script:doSign) { Write-Host "    signtool    : $signtool" }

# --- Build the package(s) ------------------------------------------------------
New-Item -ItemType Directory -Path $DistDir -Force | Out-Null
if (Test-Path $outArtifact) { Remove-Item $outArtifact -Force }

if ($Bundle) {
    # Build each arch into a clean input dir (makeappx bundle /d requires a folder
    # holding ONLY the packages to bundle), then bundle them.
    $bundleInput = Join-Path $WorkspaceDir '__artifacts\bundle\win-msixbundle-input'
    if (Test-Path $bundleInput) { Remove-Item $bundleInput -Recurse -Force }
    New-Item -ItemType Directory -Path $bundleInput -Force | Out-Null

    foreach ($arch in @('x64', 'arm64')) {
        New-MsixPackage -Platform $arch -OutMsixPath (Join-Path $bundleInput "ImageGlass-$arch.msix")
    }

    Write-Host ''
    Write-Host "==> Bundling x64 + arm64 into .msixbundle"
    & $makeappx bundle /o /d $bundleInput /bv $pkgVersion /p $outArtifact
    if ($LASTEXITCODE -ne 0) { throw "makeappx bundle failed (exit $LASTEXITCODE)." }
}
else {
    New-MsixPackage -Platform $Platform -OutMsixPath $outArtifact
}

# --- Sign the final artifact (.msix or .msixbundle) ----------------------------
if ($script:doSign) {
    Write-Host ''
    Write-Host "==> Signing the $ext"
    if (Invoke-SignTool -SignTool $signtool -Files @($outArtifact)) {
        Write-Host "==> Verifying signature"
        & $signtool verify /pa $outArtifact
        if ($LASTEXITCODE -ne 0) { throw "signtool verify failed (exit $LASTEXITCODE)." }
    }
    else {
        Write-Warning "Could not sign the $ext — it has been left UNSIGNED."
        $script:doSign = $false
    }
}

# --- Clean up staging / temp produced during packing --------------------------
# The .msix / .msixbundle in __artifacts\bundle\ is the deliverable; the per-arch
# staging layouts, priconfig files and the bundle input folder are intermediate.
$packTemp = [System.Collections.Generic.List[string]]::new()
if ($Bundle) {
    $packTemp.Add((Join-Path $DistDir 'win-msixbundle-input'))
    foreach ($arch in @('x64', 'arm64')) {
        $packTemp.Add((Join-Path $DistDir "win-$arch-msix"))
        $packTemp.Add((Join-Path $DistDir "win-$arch-msix.priconfig.xml"))
    }
}
else {
    $packTemp.Add((Join-Path $DistDir "win-$Platform-msix"))
    $packTemp.Add((Join-Path $DistDir "win-$Platform-msix.priconfig.xml"))
}
foreach ($p in $packTemp) {
    if (Test-Path $p) { Remove-Item $p -Recurse -Force -ErrorAction SilentlyContinue }
}

# --- Done ----------------------------------------------------------------------
Write-Host ''
Write-Host 'Done.'
Write-Host "  Package : $outArtifact"
if ($script:doSign) {
    Write-Host '  Signed  : yes (payload binaries + package)'
    Write-Host '  Next    : upload to the GitHub release for this version.'
}
elseif ($Sign) {
    Write-Host '  Signed  : no (no signing certificate was found)'
    Write-Host "  Next    : sign the $ext before publishing it to the GitHub release."
}
else {
    Write-Host '  Signed  : no (the Microsoft Store signs it on submission)'
    Write-Host '  Next    : upload to Partner Center (Microsoft Store) as-is.'
    Write-Host "            Do NOT sign this msstore $ext yourself."
}
