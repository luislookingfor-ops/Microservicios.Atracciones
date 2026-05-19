# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
# Se asume que el contexto de construcción es la raíz de la solución (donde están todas las carpetas de microservicios)

COPY ["Microservicios.Atracciones.Identify/Microservicios.Atracciones.Identify.API/Microservicios.Atracciones.Identify.API.csproj", "Microservicios.Atracciones.Identify/Microservicios.Atracciones.Identify.API/"]
COPY ["Microservicios.Atracciones.Identify/Microservicios.Atracciones.Identify.Business/Microservicios.Atracciones.Identify.Business.csproj", "Microservicios.Atracciones.Identify/Microservicios.Atracciones.Identify.Business/"]
COPY ["Microservicios.Atracciones.Identify/Microservicios.Atracciones.Identify.DataAccess/Microservicios.Atracciones.Identify.DataAccess.csproj", "Microservicios.Atracciones.Identify/Microservicios.Atracciones.Identify.DataAccess/"]
COPY ["Microservicios.Atracciones.Identify/Microservicios.Atracciones.Identify.DataManagement/Microservicios.Atracciones.Identify.DataManagement.csproj", "Microservicios.Atracciones.Identify/Microservicios.Atracciones.Identify.DataManagement/"]

# PROYECTO COMPARTIDO gRPC (Ajustar ruta si el proyecto tiene otro nombre o ubicación)
# Si no existe o no se necesita aún, puedes comentar la siguiente línea.
COPY ["Microservicios.Atracciones.gRPC/Microservicios.Atracciones.gRPC.csproj", "Microservicios.Atracciones.gRPC/"]

# Restaurar dependencias
RUN dotnet restore "Microservicios.Atracciones.Identify/Microservicios.Atracciones.Identify.API/Microservicios.Atracciones.Identify.API.csproj"

# Copiar el resto de los archivos y compilar
COPY . .
WORKDIR "/src/Microservicios.Atracciones.Identify/Microservicios.Atracciones.Identify.API"
RUN dotnet build "Microservicios.Atracciones.Identify.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Microservicios.Atracciones.Identify.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Microservicios.Atracciones.Identify.API.dll"]
