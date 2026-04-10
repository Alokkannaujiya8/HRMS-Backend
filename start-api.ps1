param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    [string]$LaunchProfile = "https"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiProjectDir = Join-Path $repoRoot "HRMS.API"

Write-Host "Stopping existing HRMS.API processes..."
Get-Process -Name HRMS.API -ErrorAction SilentlyContinue | Stop-Process -Force

Start-Sleep -Milliseconds 700

Write-Host "Starting API ($Configuration, profile: $LaunchProfile)..."
Push-Location $apiProjectDir
try {
    dotnet run -c $Configuration --launch-profile $LaunchProfile
}
finally {
    Pop-Location
}
