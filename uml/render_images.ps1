$files = @(
  "01_usecase_overview",
  "02_usecase_employee",
  "03_usecase_hr_admin",
  "04_class_domain_model",
  "05_class_layered_architecture",
  "06_sequence_login",
  "07_sequence_payroll_calculation",
  "08_sequence_lock_payroll",
  "09_sequence_ai_inquiry",
  "10_activity_timesheet_leave",
  "11_activity_payroll_settlement",
  "12_state_payroll_period",
  "13_deployment_diagram"
)

$imagesDir = "c:\Users\TUF\Desktop\bangChamCong\uml\images"
if (!(Test-Path $imagesDir)) {
    New-Item -ItemType Directory -Path $imagesDir -Force | Out-Null
}

foreach ($item in $files) {
    $pumlPath = "c:\Users\TUF\Desktop\bangChamCong\uml\$item.puml"
    $pngPath = "$imagesDir\$item.png"
    Write-Host "Rendering $item..."
    try {
        $content = Get-Content -Path $pumlPath -Raw -Encoding UTF8
        Invoke-RestMethod -Uri "https://kroki.io/plantuml/png" -Method Post -Body $content -ContentType "text/plain; charset=utf-8" -OutFile $pngPath
        $fileSize = (Get-Item $pngPath).Length
        Write-Host "OK: $item ($fileSize bytes)"
    } catch {
        Write-Host "FAILED: $item - $($_.Exception.Message)"
    }
}
Write-Host "Finished rendering all 13 diagrams!"
