# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

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

# Copia e restaura apenas o projeto para aproveitar o cache de camadas do Docker
COPY ["FIAP.PosTech.ArqSistemas.Catalog/FIAP.PosTech.ArqSistemas.CatalogAPI/FIAP.PosTech.ArqSistemas.CatalogAPI.csproj", "FIAP.PosTech.ArqSistemas.CatalogAPI/"]
RUN dotnet restore "FIAP.PosTech.ArqSistemas.CatalogAPI/FIAP.PosTech.ArqSistemas.CatalogAPI.csproj"

# Copia o restante dos arquivos
COPY . .
# Ajustado o WORKDIR para refletir a estrutura correta gerada pelo COPY global
WORKDIR "/src/FIAP.PosTech.ArqSistemas.Catalog/FIAP.PosTech.ArqSistemas.CatalogAPI"
RUN dotnet build "FIAP.PosTech.ArqSistemas.CatalogAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Estágio de Publicação (Publish)
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# Como herdamos o WORKDIR anterior (/src/FIAP.PosTech.ArqSistemas.CatalogAPI), referenciamos diretamente o arquivo na pasta atual
RUN dotnet publish "FIAP.PosTech.ArqSistemas.CatalogAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Estágio Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FIAP.PosTech.ArqSistemas.CatalogAPI.dll"]