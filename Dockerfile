# API Corpro para Render (runtime Docker). Se construye desde la raíz del repositorio.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaurar primero (capa en caché mientras no cambien los .csproj)
COPY src/Corpro.Dominio/Corpro.Dominio.csproj src/Corpro.Dominio/
COPY src/Corpro.Contratos/Corpro.Contratos.csproj src/Corpro.Contratos/
COPY src/Corpro.Aplicacion/Corpro.Aplicacion.csproj src/Corpro.Aplicacion/
COPY src/Corpro.Infraestructura/Corpro.Infraestructura.csproj src/Corpro.Infraestructura/
COPY src/Corpro.Api/Corpro.Api.csproj src/Corpro.Api/
RUN dotnet restore src/Corpro.Api/Corpro.Api.csproj

COPY src/Corpro.Dominio/ src/Corpro.Dominio/
COPY src/Corpro.Contratos/ src/Corpro.Contratos/
COPY src/Corpro.Aplicacion/ src/Corpro.Aplicacion/
COPY src/Corpro.Infraestructura/ src/Corpro.Infraestructura/
COPY src/Corpro.Api/ src/Corpro.Api/
RUN dotnet publish src/Corpro.Api/Corpro.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
# Render enruta al puerto 10000 por defecto
ENV ASPNETCORE_HTTP_PORTS=10000 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "Corpro.Api.dll"]
