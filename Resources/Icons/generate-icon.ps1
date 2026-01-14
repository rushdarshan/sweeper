param(
    [string]$Source = "Resources/Icons/source.png",
    [string]$OutIco = "Resources/Icons/sweeper.ico",
    [int[]]$Sizes = @(16,32,48,64,128,256)
)

if (-not (Test-Path $Source)) {
    Write-Error "Source image not found: $Source`nPlace your logo at this path and re-run the script."
    exit 1
}

[Reflection.Assembly]::LoadWithPartialName("System.Drawing") | Out-Null

# Helper: resize image to square size
function Resize-ToPngBytes([System.Drawing.Image]$img, [int]$size) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)
    $g.DrawImage($img, 0, 0, $size, $size)
    $g.Dispose()
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    return ,($ms.ToArray())
}

$orig = [System.Drawing.Image]::FromFile($Source)
$pngs = @()
foreach ($s in $Sizes) {
    $png = Resize-ToPngBytes $orig $s
    $pngs += [PSCustomObject]@{ Size = $s; Bytes = $png }
}
$orig.Dispose()

# Build ICO file with PNG-format images (modern Windows accepts PNG-containing ICOs)
$outDir = Split-Path $OutIco -Parent
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }

$bw = New-Object System.IO.BinaryWriter([System.IO.File]::Open($OutIco, [System.IO.FileMode]::Create))

# ICONDIR: Reserved(2) Type(2) Count(2)
$bw.Write([uint16]0)          # Reserved
$bw.Write([uint16]1)          # Type = 1 (icon)
$bw.Write([uint16]($pngs.Count))

# Calculate header sizes
$headerSize = 6 + (16 * $pngs.Count)
$offset = $headerSize

foreach ($entry in $pngs) {
    $bytes = $entry.Bytes
    $len = $bytes.Length
    $w = if ($entry.Size -ge 256) { 0 } else { [byte]$entry.Size }
    $h = $w
    $bw.Write([byte]$w)                    # width
    $bw.Write([byte]$h)                    # height
    $bw.Write([byte]0)                     # color palette
    $bw.Write([byte]0)                     # reserved
    $bw.Write([uint16]0)                   # color planes (0 for PNG)
    $bw.Write([uint16]32)                  # bit count (32 for PNG with alpha)
    $bw.Write([uint32]$len)                # bytes in resource
    $bw.Write([uint32]$offset)             # offset of image data
    $offset += $len
}

foreach ($entry in $pngs) {
    $bw.Write($entry.Bytes)
}

$bw.Flush()
$bw.Close()

Write-Host "Created ICO: $OutIco"
foreach ($e in $pngs) { Write-Host " - $($e.Size)x$($e.Size)" }

# Also export standalone PNG sizes next to the ICO for convenience
foreach ($e in $pngs) {
    $path = Join-Path $outDir ("sweeper_$($e.Size)x$($e.Size).png")
    [System.IO.File]::WriteAllBytes($path, $e.Bytes)
}

Write-Host "Also exported PNG sizes alongside the ICO."
