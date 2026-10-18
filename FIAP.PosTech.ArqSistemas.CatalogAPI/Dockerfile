# Estágio Base para Execução
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 4040
EXPOSE 4041

# Estágio de Compilação (Build)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["FIAP.PosTech.ArqSistemas.CatalogAPI/FIAP.PosTech.ArqSistemas.CatalogAPI.csproj", "FIAP.PosTech.ArqSistemas.CatalogAPI/"]
RUN dotnet restore "FIAP.PosTech.ArqSistemas.CatalogAPI/FIAP.PosTech.ArqSistemas.CatalogAPI.csproj"

COPY . .
WORKDIR "/src/FIAP.PosTech.ArqSistemas.CatalogAPI"
RUN dotnet build "FIAP.PosTech.ArqSistemas.CatalogAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Estágio de Publicação (Publish)
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FIAP.PosTech.ArqSistemas.CatalogAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Estágio Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FIAP.PosTech.ArqSistemas.CatalogAPI.dll"]