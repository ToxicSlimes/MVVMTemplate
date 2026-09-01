param(
    [string]$UnityVersion = "6000.3.17f1",
    [string]$ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

$ErrorActionPreference = "Stop"

function Find-UnityEditor {
    param([string]$Version)

    $hubEditors = Join-Path ${env:ProgramFiles} "Unity\Hub\Editor"
    if (Test-Path $hubEditors) {
        $candidate = Join-Path $hubEditors "$Version\Editor\Unity.exe"
        if (Test-Path $candidate) {
            return $candidate
        }

        $latest = Get-ChildItem $hubEditors -Directory | Sort-Object Name -Descending | Select-Object -First 1
        if ($latest) {
            $fallback = Join-Path $latest.FullName "Editor\Unity.exe"
            if (Test-Path $fallback) {
                Write-Warning "Unity $Version not found. Using $($latest.Name)."
                return $fallback
            }
        }
    }

    $macPath = "/Applications/Unity/Hub/Editor/$Version/Unity.app/Contents/MacOS/Unity"
    if (Test-Path $macPath) {
        return $macPath
    }

    throw "Unity Editor not found. Install Unity $Version via Unity Hub or set UNITY_PATH."
}

$unityExe = if ($env:UNITY_PATH) { $env:UNITY_PATH } else { Find-UnityEditor -Version $UnityVersion }

$logFile = Join-Path $ProjectPath "Logs\ci-compile.log"
New-Item -ItemType Directory -Force -Path (Split-Path $logFile) | Out-Null

Write-Host "Project: $ProjectPath"
Write-Host "Unity:   $unityExe"
Write-Host "Log:     $logFile"

& $unityExe `
    -batchmode `
    -nographics `
    -quit `
    -projectPath $ProjectPath `
    -executeMethod Core.Editor.CI.UnityCompileCheck.Run `
    -logFile $logFile

$exitCode = $LASTEXITCODE
if ($exitCode -ne 0) {
    if (Test-Path $logFile) {
        Get-Content $logFile -Tail 80 | Write-Host
    }

    Write-Error "Unity compile check failed with exit code $exitCode."
}

Write-Host "Unity compile check passed."
exit $exitCode
