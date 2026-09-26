# Build context is src/api (set in docker-compose.yml), not this folder, so the paths
# below are relative to the directory that holds the project folders.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# 8080, not 80: since .NET 8 the ASP.NET images run as the non-root 'app' user, which
# cannot bind a privileged port. Nothing here needs root, so leave it that way.
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
# The release version CI stamps into the assembly, so a running container can be traced
# back to the tag it was cut from. See .github/workflows/release.yml.
ARG APP_VERSION=0.0.0-local
WORKDIR /src

# Restore against the project files alone. Docker caches this layer until a .csproj
# actually changes, so ordinary source edits skip the whole NuGet round trip.
COPY ["StarterApp.Domain/StarterApp.Domain.csproj", "StarterApp.Domain/"]
COPY ["StarterApp.Application/StarterApp.Application.csproj", "StarterApp.Application/"]
COPY ["StarterApp.Infrastructure/StarterApp.Infrastructure.csproj", "StarterApp.Infrastructure/"]
COPY ["StarterApp.Api/StarterApp.Api.csproj", "StarterApp.Api/"]
RUN dotnet restore "StarterApp.Api/StarterApp.Api.csproj"

COPY . .
WORKDIR "/src/StarterApp.Api"
RUN dotnet publish "StarterApp.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    /p:Version=$APP_VERSION

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StarterApp.Api.dll"]
