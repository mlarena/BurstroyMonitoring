# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BurstroyMonitoring.slnx ./
COPY BurstroyMonitoring.Data/BurstroyMonitoring.Data.csproj BurstroyMonitoring.Data/
COPY BurstroyMonitoring.TCM/BurstroyMonitoring.TCM.csproj BurstroyMonitoring.TCM/
COPY BurstroyMonitoring.Worker/BurstroyMonitoring.Worker.csproj BurstroyMonitoring.Worker/
COPY BurstroyMonitoring.Api/BurstroyMonitoring.Api.csproj BurstroyMonitoring.Api/
COPY BurstroyMonitoring.VideoMonitoring/BurstroyMonitoring.VideoMonitoring.csproj BurstroyMonitoring.VideoMonitoring/

RUN dotnet restore BurstroyMonitoring.Worker/BurstroyMonitoring.Worker.csproj

COPY BurstroyMonitoring.Data/ BurstroyMonitoring.Data/
COPY BurstroyMonitoring.Worker/ BurstroyMonitoring.Worker/

RUN dotnet publish BurstroyMonitoring.Worker/BurstroyMonitoring.Worker.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN groupadd -r burstroy && useradd -r -g burstroy -s /sbin/nologin burstroy
RUN mkdir -p /app/logs && chown -R burstroy:burstroy /app

COPY --from=build --chown=burstroy:burstroy /app/publish .

USER burstroy

ENTRYPOINT ["dotnet", "BurstroyMonitoring.Worker.dll"]
