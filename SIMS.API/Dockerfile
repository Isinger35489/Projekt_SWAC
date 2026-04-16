FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SIMS.Core/SIMS.Core.csproj ./SIMS.Core/
COPY SIMS.Web/SIMS.Web.csproj ./SIMS.Web/
COPY SIMS.API/SIMS.API.csproj ./SIMS.API/

RUN dotnet restore SIMS.API/SIMS.API.csproj

COPY SIMS.Core ./SIMS.Core
COPY SIMS.Web ./SIMS.Web
COPY SIMS.API ./SIMS.API

WORKDIR /src/SIMS.API
RUN dotnet build -c Release

RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=build /app .
COPY aspnetcore-dev-cert.pfx /https/aspnetcore-dev-cert.pfx

# VULNERABILITY: Hardcoded Secrets in Configuration Files​
# DESCRIPTION: Das Passwort für das TLS-Zertifikat wird im Dockerfile per ENV als Klartext gesetzt.​
# Jede:r der/die auf das Image Zugriff hat, kann problemlos auf die Secrets zugreifen und das Zertifikat entschlüsseln.
# MITIGATION: Secrets nicht im Dockerfile hinterlegen. Das Passwort nur zur Laufzeit über 
# einen Secret-Store, Docker Secrets, Kubernetes Secrets oder eine sichere Deployment-Konfiguration bereitstellen.

#VULNERABLE CODE:
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=yourpassword
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetcore-dev-cert.pfx
ENTRYPOINT ["dotnet", "SIMS.API.dll"]
