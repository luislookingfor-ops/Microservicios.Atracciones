# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Microservicios.Atracciones.Gateway/Microservicios.Atracciones.Gateway.csproj", "Microservicios.Atracciones.Gateway/"]

# Restaurar dependencias
RUN dotnet restore "Microservicios.Atracciones.Gateway/Microservicios.Atracciones.Gateway.csproj"

# Copiar el resto de los archivos y compilar
COPY . .
WORKDIR "/src/Microservicios.Atracciones.Gateway"
RUN dotnet build "Microservicios.Atracciones.Gateway.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Microservicios.Atracciones.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Microservicios.Atracciones.Gateway.dll"]
