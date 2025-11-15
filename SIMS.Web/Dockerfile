FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SIMS.Core/SIMS.Core.csproj ./SIMS.Core/
COPY SIMS.Web/SIMS.Web.csproj ./SIMS.Web/
COPY SIMS.API/SIMS.API.csproj ./SIMS.API/

WORKDIR /src/SIMS.Web
RUN dotnet restore ./SIMS.Web.csproj

WORKDIR /src
COPY SIMS.Core ./SIMS.Core
COPY SIMS.Web ./SIMS.Web
COPY SIMS.API ./SIMS.API

WORKDIR /src/SIMS.Web
RUN dotnet build ./SIMS.Web.csproj -c Release

RUN dotnet publish ./SIMS.Web.csproj -c Release -o /publish/web

FROM base AS final
WORKDIR /app
COPY --from=build /publish/web .
COPY aspnetcore-dev-cert.pfx /https/aspnetcore-dev-cert.pfx 
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=yourpassword
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetcore-dev-cert.pfx
ENTRYPOINT ["dotnet", "SIMS.Web.dll"]
