FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY aspnetcore-dev-cert.pfx /https/aspnetcore-dev-cert.pfx

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY SIMS.Core ./SIMS.Core
COPY SIMS.API ./SIMS.API
WORKDIR /src/SIMS.API
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=yourpassword
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetcore-dev-cert.pfx
ENTRYPOINT ["dotnet", "SIMS.API.dll"]
