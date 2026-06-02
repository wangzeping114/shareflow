# 基于官方 .NET 8 SDK 镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ShareFlow.sln ./
COPY src/Api/ ./Api/
COPY src/Application/ ./Application/
COPY src/Domain/ ./Domain/
COPY src/Infrastructure/ ./Infrastructure/
COPY src/Migrator/ ./Migrator/
RUN dotnet restore ./Api/ShareFlow.Api.csproj
RUN dotnet publish ./Api/ShareFlow.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN apt-get update \
	&& apt-get install -y --no-install-recommends fonts-noto-cjk \
	&& rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShareFlow.Api.dll"]
