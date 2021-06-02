FROM mcr.microsoft.com/dotnet/aspnet:5.0
COPY bin/Release/net5.0/publish/ DockerHeartbeat/
WORKDIR /DockerHeartbeat
ENTRYPOINT ["dotnet", "DockerHeartbeat.dll"]
ENV COMPlus_EnableDiagnostics=0docker