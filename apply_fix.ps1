$siteCssPath = "wwwroot\css\site.css"
$cleanCssPath = "tracing_clean.css"

$siteCss = [System.IO.File]::ReadAllText($siteCssPath, [System.Text.Encoding]::UTF8)
$cleanCss = [System.IO.File]::ReadAllText($cleanCssPath, [System.Text.Encoding]::UTF8)

$startMarker = "/* Tracing Canvas Layout & Full-Immersion Notebook Board */"
$endMarker = ".learning-topbar-actions {"

$idxStart = $siteCss.IndexOf($startMarker)
$idxEnd = $siteCss.IndexOf($endMarker)

if ($idxStart -ge 0 -and $idxEnd -gt $idxStart) {
    $commentIdx = $siteCss.LastIndexOf("/*", $idxEnd)
    if ($commentIdx -gt $idxStart) {
        $idxEnd = $commentIdx
    }

    $siteCss = $siteCss.Substring(0, $idxStart) + $cleanCss + [Environment]::NewLine + [Environment]::NewLine + $siteCss.Substring($idxEnd)
    [System.IO.File]::WriteAllText($siteCssPath, $siteCss, [System.Text.Encoding]::UTF8)
    Write-Host "Replaced tracing CSS block in site.css successfully!"
} else {
    Write-Host "Could not find markers: idxStart=$idxStart, idxEnd=$idxEnd"
}
