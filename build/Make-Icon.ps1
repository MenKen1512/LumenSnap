$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$target = Join-Path $PSScriptRoot '../artifacts/app.ico'
New-Item -ItemType Directory -Force (Split-Path $target) | Out-Null
$bitmap = [System.Drawing.Bitmap]::new(64,64)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$pen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(101,183,255),5)
$brush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(23,34,52))
$memory = [System.IO.MemoryStream]::new()
try {
    $graphics.SmoothingMode = 'AntiAlias'
    $graphics.FillRectangle($brush,0,0,64,64)
    $graphics.DrawLine($pen,14,14,36,14)
    $graphics.DrawLine($pen,14,14,14,36)
    $graphics.DrawLine($pen,50,50,28,50)
    $graphics.DrawLine($pen,50,50,50,28)
    $graphics.DrawLine($pen,25,39,39,25)
    $bitmap.Save($memory,[System.Drawing.Imaging.ImageFormat]::Png)
    $bytes = $memory.ToArray()
    $file = [System.IO.File]::Create($target)
    $writer = [System.IO.BinaryWriter]::new($file)
    try {
        $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]1)
        $writer.Write([byte]64); $writer.Write([byte]64); $writer.Write([byte]0); $writer.Write([byte]0)
        $writer.Write([uint16]1); $writer.Write([uint16]32); $writer.Write([uint32]$bytes.Length); $writer.Write([uint32]22)
        $writer.Write($bytes)
    } finally { $writer.Dispose() }
} finally { $graphics.Dispose(); $bitmap.Dispose(); $pen.Dispose(); $brush.Dispose(); $memory.Dispose() }
