param(
    [string]$Project = "src/DGC.Sample.Infrastructure/DGC.Sample.Infrastructure.csproj",
    [string]$StartupProject = "src/DGC.Sample.Api/DGC.Sample.Api.csproj",
    [string]$Context = "AppDbContext",
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

Write-Host "Applying EF Core migrations..." -ForegroundColor Cyan
Write-Host "Project: $Project"
Write-Host "Startup: $StartupProject"
Write-Host "Context: $Context"

dotnet ef database update `
  --project $Project `
  --startup-project $StartupProject `
  --context $Context `
  --configuration $Configuration

if ($LASTEXITCODE -ne 0) {
    throw "Migration failed."
}

Write-Host "Database migration completed." -ForegroundColor Green
