# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BurstroyMonitoring.slnx ./
COPY BurstroyMonitoring.Data/BurstroyMonitoring.Data.csproj BurstroyMonitoring.Data/
COPY BurstroyMonitoring.TCM/BurstroyMonitoring.TCM.csproj BurstroyMonitoring.TCM/
COPY BurstroyMonitoring.Worker/BurstroyMonitoring.Worker.csproj BurstroyMonitoring.Worker/
COPY BurstroyMonitoring.Api/BurstroyMonitoring.Api.csproj BurstroyMonitoring.Api/
COPY BurstroyMonitoring.VideoMonitoring/BurstroyMonitoring.VideoMonitoring.csproj BurstroyMonitoring.VideoMonitoring/

RUN dotnet restore BurstroyMonitoring.TCM/BurstroyMonitoring.TCM.csproj

COPY BurstroyMonitoring.Data/ BurstroyMonitoring.Data/
COPY BurstroyMonitoring.TCM/ BurstroyMonitoring.TCM/

RUN dotnet publish BurstroyMonitoring.TCM/BurstroyMonitoring.TCM.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends ffmpeg libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*

RUN groupadd -r burstroy && useradd -r -g burstroy -s /sbin/nologin burstroy
RUN mkdir -p /app/logs && chown -R burstroy:burstroy /app

COPY --from=build --chown=burstroy:burstroy /app/publish .

USER burstroy

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD wget -qO- http://localhost:8080/health > /dev/null || exit 1

ENTRYPOINT ["dotnet", "BurstroyMonitoring.TCM.dll"]
