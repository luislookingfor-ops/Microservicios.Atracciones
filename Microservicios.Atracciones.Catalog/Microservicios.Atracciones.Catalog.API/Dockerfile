# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
# Se asume que el contexto de construcción es la raíz de la solución (donde están todas las carpetas de microservicios)

COPY ["Microservicios.Atracciones.Catalog/Microservicios.Atracciones.Catalog.API/Microservicios.Atracciones.Catalog.API.csproj", "Microservicios.Atracciones.Catalog/Microservicios.Atracciones.Catalog.API/"]
COPY ["Microservicios.Atracciones.Catalog/Microservicios.Atracciones.Catalog.Business/Microservicios.Atracciones.Catalog.Business.csproj", "Microservicios.Atracciones.Catalog/Microservicios.Atracciones.Catalog.Business/"]
COPY ["Microservicios.Atracciones.Catalog/Microservicios.Atracciones.Catalog.DataAccess/Microservicios.Atracciones.Catalog.DataAccess.csproj", "Microservicios.Atracciones.Catalog/Microservicios.Atracciones.Catalog.DataAccess/"]
COPY ["Microservicios.Atracciones.Catalog/Microservicios.Atracciones.Catalog.DataManagement/Microservicios.Atracciones.Catalog.DataManagement.csproj", "Microservicios.Atracciones.Catalog/Microservicios.Atracciones.Catalog.DataManagement/"]

# PROYECTO COMPARTIDO gRPC (Ajustar ruta si el proyecto tiene otro nombre o ubicación)
# Si no existe o no se necesita aún, puedes comentar la siguiente línea.
COPY ["Microservicios.Atracciones.gRPC/Microservicios.Atracciones.gRPC.csproj", "Microservicios.Atracciones.gRPC/"]

# Restaurar dependencias
RUN dotnet restore "Microservicios.Atracciones.Catalog/Microservicios.Atracciones.Catalog.API/Microservicios.Atracciones.Catalog.API.csproj"

# Copiar el resto de los archivos y compilar
COPY . .
WORKDIR "/src/Microservicios.Atracciones.Catalog/Microservicios.Atracciones.Catalog.API"
RUN dotnet build "Microservicios.Atracciones.Catalog.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Microservicios.Atracciones.Catalog.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Microservicios.Atracciones.Catalog.API.dll"]
