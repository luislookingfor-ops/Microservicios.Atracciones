# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
# Se asume que el contexto de construcción es la raíz de la solución (donde están todas las carpetas de microservicios)

COPY ["Microservicios.Atracciones.Booking/Microservicios.Atracciones.Booking.API/Microservicios.Atracciones.Booking.API.csproj", "Microservicios.Atracciones.Booking/Microservicios.Atracciones.Booking.API/"]
COPY ["Microservicios.Atracciones.Booking/Microservicios.Atracciones.Booking.Business/Microservicios.Atracciones.Booking.Business.csproj", "Microservicios.Atracciones.Booking/Microservicios.Atracciones.Booking.Business/"]
COPY ["Microservicios.Atracciones.Booking/Microservicios.Atracciones.Booking.DataAccess/Microservicios.Atracciones.Booking.DataAccess.csproj", "Microservicios.Atracciones.Booking/Microservicios.Atracciones.Booking.DataAccess/"]
COPY ["Microservicios.Atracciones.Booking/Microservicios.Atracciones.Booking.DataManagement/Microservicios.Atracciones.Booking.DataManagement.csproj", "Microservicios.Atracciones.Booking/Microservicios.Atracciones.Booking.DataManagement/"]

# PROYECTO COMPARTIDO gRPC (Ajustar ruta si el proyecto tiene otro nombre o ubicación)
# Si no existe o no se necesita aún, puedes comentar la siguiente línea.
COPY ["Microservicios.Atracciones.gRPC/Microservicios.Atracciones.gRPC.csproj", "Microservicios.Atracciones.gRPC/"]

# Restaurar dependencias
RUN dotnet restore "Microservicios.Atracciones.Booking/Microservicios.Atracciones.Booking.API/Microservicios.Atracciones.Booking.API.csproj"

# Copiar el resto de los archivos y compilar
COPY . .
WORKDIR "/src/Microservicios.Atracciones.Booking/Microservicios.Atracciones.Booking.API"
RUN dotnet build "Microservicios.Atracciones.Booking.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Microservicios.Atracciones.Booking.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Microservicios.Atracciones.Booking.API.dll"]
