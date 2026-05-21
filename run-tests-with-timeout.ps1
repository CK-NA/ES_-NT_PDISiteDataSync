# Test Runner with Aggressive Timeout Handling
# Runs each test class separately with a 30-second timeout

$ErrorActionPreference = "Continue"
$testProject = "PDI_SiteDataSync.Tests\PDI_SiteDataSync.Tests.csproj"

# Kill any existing testhost processes
Write-Host "Cleaning up existing test processes..." -ForegroundColor Yellow
Get-Process -Name testhost -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2

$testClasses = @(
    "PDI_SiteDataSync.Tests.ApplicationProcessTests",
    "PDI_SiteDataSync.Tests.ArchiveManagerTests",
    "PDI_SiteDataSync.Tests.ExcelDataReaderTests",
    "PDI_SiteDataSync.Tests.LogHelperTests",
    "PDI_SiteDataSync.Tests.ProgramInitializerTests"
)

$results = @()

foreach ($testClass in $testClasses) {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Running: $testClass" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan

    $startTime = Get-Date

    # Start the test as a background job with timeout
    $job = Start-Job -ScriptBlock {
        param($project, $filter)
        dotnet test $project --no-build --filter "FullyQualifiedName~$filter" --logger "console;verbosity=normal"
    } -ArgumentList $testProject, $testClass

    # Wait for job with timeout (30 seconds)
    $timeout = 30
    $completed = Wait-Job -Job $job -Timeout $timeout

    $endTime = Get-Date
    $duration = ($endTime - $startTime).TotalSeconds

    if ($completed) {
        $output = Receive-Job -Job $job
        $exitCode = $job.State

        if ($output -match "Failed.*(\d+)") {
            $status = "FAILED"
            $color = "Red"
        }
        elseif ($output -match "Passed") {
            $status = "PASSED"
            $color = "Green"
        }
        else {
            $status = "COMPLETED"
            $color = "Yellow"
        }

        Write-Host "`n$testClass : $status ($([math]::Round($duration, 2))s)" -ForegroundColor $color

        $results += [PSCustomObject]@{
            TestClass = $testClass
            Status = $status
            Duration = [math]::Round($duration, 2)
        }
    }
    else {
        Write-Host "`n$testClass : TIMEOUT/HUNG after ${timeout}s" -ForegroundColor Red

        # Kill the hung job
        Stop-Job -Job $job -ErrorAction SilentlyContinue
        Remove-Job -Job $job -Force -ErrorAction SilentlyContinue

        # Kill any testhost processes
        Get-Process -Name testhost -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Seconds 2

        $results += [PSCustomObject]@{
            TestClass = $testClass
            Status = "HUNG"
            Duration = $timeout
        }
    }

    Remove-Job -Job $job -Force -ErrorAction SilentlyContinue
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$results | Format-Table -AutoSize

$hungTests = $results | Where-Object { $_.Status -eq "HUNG" }
if ($hungTests) {
    Write-Host "`nHUNG TESTS DETECTED:" -ForegroundColor Red
    $hungTests | ForEach-Object { Write-Host "  - $($_.TestClass)" -ForegroundColor Red }
}

# Final cleanup
Get-Process -Name testhost -ErrorAction SilentlyContinue | Stop-Process -Force
