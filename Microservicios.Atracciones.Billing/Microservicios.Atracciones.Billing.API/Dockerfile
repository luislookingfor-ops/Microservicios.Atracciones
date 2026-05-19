# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
# Se asume que el contexto de construcción es la raíz de la solución (donde están todas las carpetas de microservicios)

COPY ["Microservicios.Atracciones.Billing/Microservicios.Atracciones.Billing.API/Microservicios.Atracciones.Billing.API.csproj", "Microservicios.Atracciones.Billing/Microservicios.Atracciones.Billing.API/"]
COPY ["Microservicios.Atracciones.Billing/Microservicios.Atracciones.Billing.Business/Microservicios.Atracciones.Billing.Business.csproj", "Microservicios.Atracciones.Billing/Microservicios.Atracciones.Billing.Business/"]
COPY ["Microservicios.Atracciones.Billing/Microservicios.Atracciones.Billing.DataAccess/Microservicios.Atracciones.Billing.DataAccess.csproj", "Microservicios.Atracciones.Billing/Microservicios.Atracciones.Billing.DataAccess/"]
COPY ["Microservicios.Atracciones.Billing/Microservicios.Atracciones.Billing.DataManagement/Microservicios.Atracciones.Billing.DataManagement.csproj", "Microservicios.Atracciones.Billing/Microservicios.Atracciones.Billing.DataManagement/"]

# PROYECTO COMPARTIDO gRPC (Ajustar ruta si el proyecto tiene otro nombre o ubicación)
# Si no existe o no se necesita aún, puedes comentar la siguiente línea.
COPY ["Microservicios.Atracciones.gRPC/Microservicios.Atracciones.gRPC.csproj", "Microservicios.Atracciones.gRPC/"]

# Restaurar dependencias
RUN dotnet restore "Microservicios.Atracciones.Billing/Microservicios.Atracciones.Billing.API/Microservicios.Atracciones.Billing.API.csproj"

# Copiar el resto de los archivos y compilar
COPY . .
WORKDIR "/src/Microservicios.Atracciones.Billing/Microservicios.Atracciones.Billing.API"
RUN dotnet build "Microservicios.Atracciones.Billing.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Microservicios.Atracciones.Billing.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Microservicios.Atracciones.Billing.API.dll"]
