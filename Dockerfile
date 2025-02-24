# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copiar archivos y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del código y construir la aplicación
COPY . .
RUN dotnet publish -c Release -o out

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copiar archivos desde la etapa de construcción
COPY --from=build /app/out ./

# Configurar el puerto y el punto de entrada
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "Kiosco.dll"]