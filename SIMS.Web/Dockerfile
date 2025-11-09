FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY SIMS.Core ./SIMS.Core
COPY SIMS.Web ./SIMS.Web
WORKDIR /src/SIMS.Web
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=build /app .
COPY aspnetcore-dev-cert.pfx /https/aspnetcore-dev-cert.pfx 
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=yourpassword
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetcore-dev-cert.pfx
ENTRYPOINT ["dotnet", "SIMS.Web.dll"]
