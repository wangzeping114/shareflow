# 基于官方 .NET 8 SDK 镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY back-end/ShareFlow.sln ./
COPY back-end/src/Api/ ./Api/
COPY back-end/src/Application/ ./Application/
COPY back-end/src/Domain/ ./Domain/
COPY back-end/src/Infrastructure/ ./Infrastructure/
COPY back-end/src/Migrator/ ./Migrator/
RUN dotnet restore ./Api/ShareFlow.Api.csproj
RUN dotnet publish ./Api/ShareFlow.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShareFlow.Api.dll"]
