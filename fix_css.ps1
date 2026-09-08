$path = "wwwroot\css\site.css"
$text = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)

# 1. Restore .summary-rows and .summary-row
$target1 = ".summary-rows {" + [Environment]::NewLine + "}" + [Environment]::NewLine + [Environment]::NewLine + ".summary-row.completed"
$replace1 = @'
.summary-rows {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.summary-row {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  border-radius: 18px;
  background: #f8fafc;
  border: 1.5px solid #e2e8f0;
}

.summary-row.completed {
  background: #f0fdf4;
  border-color: #bbf7d0;
}

.summary-row.practice {
  background: #fffbeb;
  border-color: #fde68a;
}

.summary-row .row-status-icon {
  font-size: 1.5rem;
  flex-shrink: 0;
}

.summary-row.completed
'@

if ($text.Contains($target1)) {
    $text = $text.Replace($target1, $replace1)
    Write-Host "Restored summary-rows"
} else {
    Write-Host "Target1 not found"
}

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
