param(
    [string] $ContainerName = "english_tutor_postgres",
    [string] $PostgresUser = "postgres",
    [string] $DatabasePattern = "english_tutor_test_%",
    [switch] $IncludeDevelopmentDatabase
)

$ErrorActionPreference = "Stop"

if (-not (docker ps --format "{{.Names}}" | Where-Object { $_ -eq $ContainerName })) {
    Write-Host "PostgreSQL container '$ContainerName' is not running; skipping database cleanup."
    exit 0
}

function Invoke-PostgresScalar([string] $Sql) {
    docker exec $ContainerName psql -U $PostgresUser -d postgres -tAc $Sql
}

$databases = @(Invoke-PostgresScalar "SELECT datname FROM pg_database WHERE datistemplate = false AND datname LIKE '$DatabasePattern';")
$databases = $databases | ForEach-Object { $_.Trim() } | Where-Object { $_ }

if ($IncludeDevelopmentDatabase) {
    $databases += "english_tutor_db"
}

if ($databases.Count -eq 0) {
    Write-Host "No matching test databases found."
    exit 0
}

foreach ($database in $databases | Select-Object -Unique) {
    if ($database -notlike "english_tutor_test_*" -and $database -ne "english_tutor_db") {
        throw "Refusing to drop unexpected database '$database'."
    }

    Write-Host "Dropping database '$database'..."
    Invoke-PostgresScalar "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$database' AND pid <> pg_backend_pid();" | Out-Null
    Invoke-PostgresScalar "DROP DATABASE IF EXISTS `"$database`";" | Out-Null
}

Write-Host "Database cleanup complete."
