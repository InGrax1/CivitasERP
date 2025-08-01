param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$true)]
    [string]$AzureConnectionString
)

# Variables
$ProjectPath = ".\CivitasERP.csproj"
$OutputDir = ".\publish"
$ReleasesDir = ".\releases"

Write-Host "🔨 Compilando aplicación..." -ForegroundColor Yellow

# Limpiar directorios previos
Remove-Item -Path $OutputDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path $ReleasesDir -Recurse -Force -ErrorAction SilentlyContinue

# Publicar aplicación
dotnet publish $ProjectPath -c Release -o $OutputDir --self-contained true -r win-x64

Write-Host "📦 Creando paquete Velopack..." -ForegroundColor Yellow

# Crear paquete con Velopack
vpk pack -u CivitasERP -v $Version -p $OutputDir -e CivitasERP.exe -o $ReleasesDir

Write-Host "☁️ Subiendo a Azure..." -ForegroundColor Yellow

# Instalar Azure CLI si no está instalado
if (!(Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Host "Azure CLI no encontrado. Instalando..." -ForegroundColor Red
    Invoke-WebRequest -Uri https://aka.ms/installazurecliwindows -OutFile .\AzureCLI.msi
    Start-Process msiexec.exe -Wait -ArgumentList '/I AzureCLI.msi /quiet'
    Remove-Item .\AzureCLI.msi
}

# Subir archivos a Azure Blob Storage
foreach ($file in Get-ChildItem -Path $ReleasesDir -File) {
    az storage blob upload `
        --connection-string $AzureConnectionString `
        --container-name "releases" `
        --file $file.FullName `
        --name $file.Name `
        --overwrite
    
    Write-Host "✅ Subido: $($file.Name)" -ForegroundColor Green
}

Write-Host "🎉 Deploy completado!" -ForegroundColor Green
Write-Host "URL de actualizaciones: DefaultEndpointsProtocol=https;AccountName=civitasupdates;AccountKey=YRyOguohG13QBmVdrU/RuphGx26z1KNMcnAT5YmZqwdAjpcv785Kg/DbXthAeHtXJrIlNb9GBnMM+ASt7hYeCQ==;EndpointSuffix=core.windows.net" -ForegroundColor Cyan