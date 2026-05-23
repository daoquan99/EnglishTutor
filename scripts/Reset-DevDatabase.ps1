param(
    [string] $ComposeProjectName = "englishtutor"
)

$ErrorActionPreference = "Stop"

Write-Host "Stopping local compose services and removing persistent volumes..."
docker compose -p $ComposeProjectName down --volumes

Write-Host "Starting PostgreSQL and Redis with fresh volumes..."
docker compose -p $ComposeProjectName up -d postgres redis

Write-Host "Development database reset complete. Start the API/Worker with Database__AutoMigrate=true to recreate schemas."
