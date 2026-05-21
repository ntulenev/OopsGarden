# OopsGarden

ASP.NET Core + EF Core SQL Server MVP for tracking house plants and watering.

## Run

The SQL Server connection string is intentionally not stored in `appsettings.json`.
Set it as an environment variable:

```powershell
$env:ConnectionStrings__OopsGarden = "<connection string>"
dotnet run --project src/OopsGarden/OopsGarden.csproj --urls http://localhost:5297
```

Admin accounts are configured in `src/OopsGarden/appsettings.json`.
Default MVP admin:

- user: `admin`
- password: `ChangeMe123!`

Build the solution from the repository root:

```powershell
dotnet build src/OopsGarden.slnx
```

## EF

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/Storage/Storage.csproj --startup-project src/OopsGarden/OopsGarden.csproj
```
