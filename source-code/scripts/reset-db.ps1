# =============================================================
# reset-db.ps1
# Drops and recreates the Atlas development database.
# Seed data is applied automatically on next app startup.
#
# Usage (from solution root):
#   .\scripts\reset-db.ps1
#
# Requires: dotnet-ef tool
#   dotnet tool install --global dotnet-ef
# =============================================================

$ErrorActionPreference = "Stop"

$infra   = "src\Atlas.Identity.Infrastructure\Atlas.Identity.Infrastructure.csproj"
$startup = "src\Atlas.API\Atlas.API.csproj"

Write-Host ""
Write-Host ">>> Dropping database..." -ForegroundColor Yellow
dotnet ef database drop --project $infra --startup-project $startup --force

Write-Host ""
Write-Host ">>> Recreating schema (migrations)..." -ForegroundColor Yellow
dotnet ef database update --project $infra --startup-project $startup

Write-Host ""
Write-Host "Done. Start the API to apply seed data." -ForegroundColor Green
