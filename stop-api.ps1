$ErrorActionPreference = "Stop"

Write-Host "Stopping HRMS.API processes..."
Get-Process -Name HRMS.API -ErrorAction SilentlyContinue | Stop-Process -Force

Write-Host "Done."
