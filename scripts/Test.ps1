param(
    [switch] $SkipDatabaseCleanup
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

if (-not $SkipDatabaseCleanup) {
    & "$PSScriptRoot\Clear-TestDatabases.ps1"
}

dotnet test "$repoRoot\EnglishTutor.API\EnglishTutor.slnx"
