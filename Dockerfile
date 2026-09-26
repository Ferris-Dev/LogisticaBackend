# Imagen de la API para Railway. Se compila desde la raíz del repositorio,
# así Railway no necesita "Root Directory" ni otra configuración en el panel.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero los .csproj para aprovechar la caché del restore.
COPY backend/src/Tracking.Api/Tracking.Api.csproj backend/src/Tracking.Api/
COPY backend/src/Tracking.Application/Tracking.Application.csproj backend/src/Tracking.Application/
COPY backend/src/Tracking.Domain/Tracking.Domain.csproj backend/src/Tracking.Domain/
COPY backend/src/Tracking.Infrastructure/Tracking.Infrastructure.csproj backend/src/Tracking.Infrastructure/
RUN dotnet restore backend/src/Tracking.Api/Tracking.Api.csproj

COPY backend/src/ backend/src/
RUN dotnet publish backend/src/Tracking.Api/Tracking.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# Railway inyecta PORT; Program.cs lo lee y usa 8080 si no existe.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "Tracking.Api.dll"]
