# Clear Roslyn Analyzer Cache for Visual Studio
# This forces Visual Studio to reload all analyzers with the latest changes

Write-Host "Clearing Roslyn Analyzer Cache..." -ForegroundColor Yellow

# Find Visual Studio processes
$vsProcesses = Get-Process -Name "devenv" -ErrorAction SilentlyContinue

if ($vsProcesses) {
    Write-Host "WARNING: Visual Studio is currently running!" -ForegroundColor Red
    Write-Host "Please close Visual Studio before running this script." -ForegroundColor Red
    Write-Host ""
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

# Clear analyzer cache directories
$cacheLocations = @(
    "$env:LOCALAPPDATA\Microsoft\VisualStudio\*\ComponentModelCache",
    "$env:LOCALAPPDATA\Microsoft\VisualStudio\*\*.ide",
    "$env:LOCALAPPDATA\Temp\.NET\*",
    "$env:TEMP\VBCSCompiler\*"
)

$deletedCount = 0

foreach ($pattern in $cacheLocations) {
    $paths = Get-Item $pattern -ErrorAction SilentlyContinue
    foreach ($path in $paths) {
        try {
            Write-Host "Deleting: $path" -ForegroundColor Cyan
            Remove-Item $path -Recurse -Force -ErrorAction Stop
            $deletedCount++
        }
        catch {
            Write-Host "  (skipped - in use or inaccessible)" -ForegroundColor Gray
        }
    }
}

# Kill any lingering VBCSCompiler processes
$vbcsProcesses = Get-Process -Name "VBCSCompiler" -ErrorAction SilentlyContinue
if ($vbcsProcesses) {
    Write-Host "Stopping VBCSCompiler processes..." -ForegroundColor Yellow
    $vbcsProcesses | Stop-Process -Force
}

Write-Host ""
Write-Host "? Analyzer cache cleared successfully!" -ForegroundColor Green
Write-Host "  Deleted $deletedCount cache location(s)" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Reopen Visual Studio" -ForegroundColor White
Write-Host "2. Rebuild the solution" -ForegroundColor White
Write-Host "3. The analyzer should now use the latest version" -ForegroundColor White
Write-Host ""
