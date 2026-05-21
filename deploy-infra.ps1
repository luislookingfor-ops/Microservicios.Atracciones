# =========================================================================================
# Script de Aprovisionamiento de Infraestructura en Azure para Microservicios
# =========================================================================================
# Este script creará de manera automatizada:
# 1. Un Grupo de Recursos (Resource Group)
# 2. Un Registro de Contenedores de Azure (Azure Container Registry - ACR)
# 3. Un Entorno de Azure Container Apps (Container Apps Environment)
# 4. 4 Azure Container Apps (Billing, Booking, Catalog, Identify) con Ingress en puerto 8080.
# =========================================================================================

# Configurar codificación para caracteres especiales en español
$OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "  APROVISIONAMIENTO DE MICROSERVICIOS EN MICROSOFT AZURE  " -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

# 1. Verificar si Azure CLI está instalado
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI no está instalado en este sistema. Por favor, completa la instalación antes de ejecutar este script."
    exit 1
}

# 2. Verificar Inicio de Sesión en Azure
Write-Host "`n[1/6] Verificando sesión activa en Azure..." -ForegroundColor Yellow
$account = az account show --query name -o tsv 2>$null
if (-not $account) {
    Write-Host "No se detectó una sesión activa. Iniciando proceso de inicio de sesión en tu navegador..." -ForegroundColor Cyan
    az login
    $account = az account show --query name -o tsv
}
Write-Host "¡Sesión verificada exitosamente en la cuenta: $account!" -ForegroundColor Green

# 3. Definir Variables de Infraestructura
Write-Host "`n[2/6] Configurando variables del sistema..." -ForegroundColor Yellow

# Suffix aleatorio para asegurar nombres globales únicos (por ejemplo, para el Container Registry)
$suffix = Get-Random -Minimum 1000 -Maximum 9999
$LOCATION = "eastus2" # East US 2 es excelente para Azure for Students (costo bajo y disponible)
$RESOURCE_GROUP = "rg-microservicios-atracciones"
$ACR_NAME = "acratracciones$suffix"
$ENVIRONMENT_NAME = "cae-atracciones"

Write-Host "Variables configuradas:" -ForegroundColor DarkCyan
Write-Host "  - Grupo de Recursos: $RESOURCE_GROUP" -ForegroundColor DarkCyan
Write-Host "  - Ubicación/Región: $LOCATION" -ForegroundColor DarkCyan
Write-Host "  - Registro de Contenedores (ACR): $ACR_NAME" -ForegroundColor DarkCyan
Write-Host "  - Entorno de Container Apps (CAE): $ENVIRONMENT_NAME" -ForegroundColor DarkCyan

# 4. Crear Grupo de Recursos
Write-Host "`n[3/6] Creando Grupo de Recursos..." -ForegroundColor Yellow
az group create --name $RESOURCE_GROUP --location $LOCATION --output table
Write-Host "¡Grupo de recursos '$RESOURCE_GROUP' listo!" -ForegroundColor Green

# 5. Crear Azure Container Registry (ACR)
Write-Host "`n[4/6] Creando Azure Container Registry (ACR)..." -ForegroundColor Yellow
az acr create --resource-group $RESOURCE_GROUP --name $ACR_NAME --sku Basic --admin-enabled true --output table
Write-Host "¡Registro de Contenedores '$ACR_NAME' creado exitosamente!" -ForegroundColor Green

# Obtener credenciales de administrador de ACR
Write-Host "Obteniendo credenciales del registro..." -ForegroundColor DarkCyan
$ACR_CREDENTIALS = az acr credential show --name $ACR_NAME --query "{username:username, passwords:passwords[0].value}" -o json | ConvertFrom-Json
$ACR_USERNAME = $ACR_CREDENTIALS.username
$ACR_PASSWORD = $ACR_CREDENTIALS.passwords

# 6. Crear el Entorno de Azure Container Apps (CAE)
Write-Host "`n[5/6] Creando Entorno de Azure Container Apps (esto puede tomar 1-2 minutos)..." -ForegroundColor Yellow
az containerapp env create --name $ENVIRONMENT_NAME --resource-group $RESOURCE_GROUP --location $LOCATION --output table
Write-Host "¡Entorno de Container Apps '$ENVIRONMENT_NAME' creado!" -ForegroundColor Green

# 7. Crear las 4 Container Apps con una imagen placeholder (hola-mundo)
# La imagen real será desplegada después por GitHub Actions
Write-Host "`n[6/6] Creando las 4 Container Apps (Billing, Booking, Catalog, Identify)..." -ForegroundColor Yellow
$microservices = @("ca-billing", "ca-booking", "ca-catalog", "ca-identify")
$placeholderImage = "mcr.microsoft.com/azuredocs/aci-helloworld:latest"

foreach ($service in $microservices) {
    Write-Host "`n-> Creando Container App: $service..." -ForegroundColor Cyan
    az containerapp create `
        --name $service `
        --resource-group $RESOURCE_GROUP `
        --environment $ENVIRONMENT_NAME `
        --image $placeholderImage `
        --target-port 8080 `
        --ingress external `
        --query "properties.configuration.ingress.fqdn" `
        --output tsv
    Write-Host "¡Container App '$service' creada con éxito!" -ForegroundColor Green
}

# 8. Intentar crear Service Principal para GitHub Actions
Write-Host "`n========================================================" -ForegroundColor Cyan
Write-Host "   CREACIÓN DE CREDENCIALES PARA GITHUB ACTIONS (CI/CD)  " -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "Intentando crear Service Principal para la automatización en GitHub..." -ForegroundColor Yellow

$subscriptionId = az account show --query id -o tsv
$spName = "sp-github-actions-atracciones"

Write-Host "ID de Suscripción: $subscriptionId" -ForegroundColor DarkCyan

try {
    # Comando para crear la credencial de Azure para GitHub
    $spJson = az ad sp create-for-rbac `
        --name $spName `
        --role contributor `
        --scopes "/subscriptions/$subscriptionId/resourceGroups/$RESOURCE_GROUP" `
        --sdk-auth `
        -o json 2>$null

    if ($spJson) {
        Write-Host "`n¡Credenciales de GitHub creadas exitosamente!" -ForegroundColor Green
        Write-Host "Por favor, guarda el siguiente bloque JSON en un secreto de GitHub llamado 'AZURE_CREDENTIALS':`n" -ForegroundColor Yellow
        Write-Host $spJson -ForegroundColor Gray
    } else {
        Write-Host "`n[AVISO] No se pudo crear el Service Principal automáticamente." -ForegroundColor Warning
        Write-Host "Esto ocurre comúnmente en suscripciones 'Azure for Students' debido a restricciones de administrador en tu directorio escolar." -ForegroundColor Warning
        Write-Host "¡No te preocupes! Podrás configurar el despliegue manual o usar credenciales del ACR más adelante." -ForegroundColor Cyan
    }
} catch {
    Write-Host "`n[AVISO] Ocurrió un error al intentar crear las credenciales. Es probable que no tengas permisos de administrador en el inquilino (Tenant) de tu institución educativa." -ForegroundColor Warning
}

# Mostrar credenciales de ACR para GitHub Secrets alternativos
Write-Host "`n========================================================" -ForegroundColor Cyan
Write-Host "   INFORMACIÓN IMPORTANTE PARA CONFIGURAR GITHUB SECRETS  " -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "Agrega los siguientes secretos en tu repositorio de GitHub (Configuración > Secrets and variables > Actions):" -ForegroundColor Yellow
Write-Host "`n1. Nombre: REGISTRY_LOGIN_SERVER" -ForegroundColor Yellow
Write-Host "   Valor:  $ACR_NAME.azurecr.io" -ForegroundColor Gray
Write-Host "`n2. Nombre: REGISTRY_USERNAME" -ForegroundColor Yellow
Write-Host "   Valor:  $ACR_USERNAME" -ForegroundColor Gray
Write-Host "`n3. Nombre: REGISTRY_PASSWORD" -ForegroundColor Yellow
Write-Host "   Valor:  $ACR_PASSWORD" -ForegroundColor Gray
Write-Host "`n4. Nombre: AZURE_RESOURCE_GROUP" -ForegroundColor Yellow
Write-Host "   Valor:  $RESOURCE_GROUP" -ForegroundColor Gray

Write-Host "`n¡Infraestructura aprovisionada con éxito! Ahora puedes subir los cambios a tu repositorio y el pipeline de GitHub Actions se encargará del resto." -ForegroundColor Green
