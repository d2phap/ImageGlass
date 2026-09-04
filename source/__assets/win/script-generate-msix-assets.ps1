#Requires -Version 7.0
<#
.SYNOPSIS
    Generate the MSIX tile/store logo set from a single source image.

.DESCRIPTION
    Both MSIX flavours use logos rendered from the current app logo: the signed
    (GitHub) set from __assets/logo_c_512.png, the msstore set from
    __assets/logo_p_512.png (the Store identity itself grants Pro). This script
    mirrors a REFERENCE asset folder filename-for-filename: for every
    "<LogoName>.<qualifiers>.png" in -ReferenceDir it works out the target pixel
    size from the logo name + qualifiers, renders it from -Source, and writes it
    to -OutDir. Mirroring guarantees the same AppxManifest + resources.pri logic
    works for both the signed and msstore packages.

    Target size rules (per MSIX asset conventions):
      * base size comes from the logo name — Square150x150 -> 150x150,
        Wide310x150 -> 310x150, StoreLogo -> 50x50.
      * a "targetsize-N" qualifier  -> N x N (exact, overrides scale).
      * a "scale-N" qualifier       -> base * N / 100.
      * Wide* logos are drawn as the square logo centered on a transparent
        canvas of the full (wide) dimensions.

.PARAMETER Source
    Source image (square, high-res). Default: __assets/logo_c_512.png.

.PARAMETER ReferenceDir
    Folder whose filenames are mirrored. Default: appxmanifest/Assets-msstore.
    Must never be the same folder as -OutDir, which is wiped before enumeration.

.PARAMETER OutDir
    Output folder. Default: appxmanifest/Assets-signed.

.EXAMPLE
    pwsh __assets/win/script-generate-msix-assets.ps1
    # Regenerate appxmanifest/Assets-signed from __assets/logo_c_512.png.

.EXAMPLE
    pwsh __assets/win/script-generate-msix-assets.ps1 -Source __assets/logo_p_512.png `
        -ReferenceDir __assets/win/appxmanifest/Assets-signed `
        -OutDir __assets/win/appxmanifest/Assets-msstore
    # Regenerate appxmanifest/Assets-msstore from __assets/logo_p_512.png.
#>

[CmdletBinding()]
param(
    [string]$Source = '',
    [string]$ReferenceDir = '',
    [string]$OutDir = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$WorkspaceDir = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if (-not $Source)       { $Source = Join-Path $WorkspaceDir '__assets\logo_c_512.png' }
if (-not $ReferenceDir) { $ReferenceDir = Join-Path $PSScriptRoot 'appxmanifest\Assets-msstore' }
if (-not $OutDir)       { $OutDir = Join-Path $PSScriptRoot 'appxmanifest\Assets-signed' }

if (-not (Test-Path $Source))       { throw "Source image not found: $Source" }
if (-not (Test-Path $ReferenceDir)) { throw "Reference folder not found: $ReferenceDir" }

Add-Type -AssemblyName System.Drawing

# Base (scale-100) dimensions for a logo, derived from its name.
function Get-BaseSize([string]$LogoName) {
    if ($LogoName -match '(\d+)x(\d+)') {
        return @([int]$Matches[1], [int]$Matches[2])
    }
    if ($LogoName -like 'StoreLogo*') { return @(50, 50) }
    throw "Cannot determine base size for logo '$LogoName'."
}

# Work out (width, height) for one asset filename.
function Get-TargetSize([string]$LogoName, [string[]]$Qualifiers) {
    $base = Get-BaseSize $LogoName
    foreach ($q in $Qualifiers) {
        if ($q -match '^targetsize-(\d+)$') {
            $n = [int]$Matches[1]
            return @($n, $n)
        }
    }
    foreach ($q in $Qualifiers) {
        if ($q -match '^scale-(\d+)$') {
            $s = [int]$Matches[1] / 100.0
            return @([int][Math]::Round($base[0] * $s), [int][Math]::Round($base[1] * $s))
        }
    }
    return $base
}

$src = [System.Drawing.Image]::FromFile((Resolve-Path $Source).Path)
try {
    if (Test-Path $OutDir) { Remove-Item $OutDir -Recurse -Force }
    New-Item -ItemType Directory -Path $OutDir -Force | Out-Null

    $files = Get-ChildItem -Path $ReferenceDir -Filter '*.png' -File
    Write-Host "==> Generating $($files.Count) asset(s) from $Source"

    foreach ($f in $files) {
        # "<LogoName>.<qualifier>[_<qualifier>...].png"  (qualifier segment is optional)
        $stem  = [System.IO.Path]::GetFileNameWithoutExtension($f.Name)
        $parts = $stem.Split('.')
        $logoName   = $parts[0]
        $qualifiers = if ($parts.Count -gt 1) { ($parts[1..($parts.Count - 1)] -join '.').Split('_') } else { @() }

        $size  = Get-TargetSize $logoName $qualifiers
        $w     = $size[0]
        $h     = $size[1]
        $isWide = $logoName -like '*Wide*'

        $bmp = [System.Drawing.Bitmap]::new($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $g   = [System.Drawing.Graphics]::FromImage($bmp)
        try {
            $g.InterpolationMode  = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $g.PixelOffsetMode    = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $g.SmoothingMode      = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
            $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
            $g.Clear([System.Drawing.Color]::Transparent)

            if ($isWide) {
                # Center the square logo on the wide transparent canvas.
                $side = [Math]::Min($w, $h)
                $x = [int](($w - $side) / 2)
                $y = [int](($h - $side) / 2)
                $g.DrawImage($src, $x, $y, $side, $side)
            }
            else {
                $g.DrawImage($src, 0, 0, $w, $h)
            }
        }
        finally {
            $g.Dispose()
        }
        $bmp.Save((Join-Path $OutDir $f.Name), [System.Drawing.Imaging.ImageFormat]::Png)
        $bmp.Dispose()
    }
}
finally {
    $src.Dispose()
}

Write-Host "Done. Wrote assets to: $OutDir"
