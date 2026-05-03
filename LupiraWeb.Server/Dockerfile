FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY LupiraWeb.Domain/ ./LupiraWeb.Domain/
COPY LupiraWeb.Server/ ./LupiraWeb.Server/

WORKDIR /src/LupiraWeb.Server

ARG BUILD_CONFIGURATION=Release
RUN dotnet restore "./LupiraWeb.Server.csproj"
RUN dotnet publish "./LupiraWeb.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:80;https://+:443
COPY --from=build /app/publish .

USER app

EXPOSE 80
ENTRYPOINT ["dotnet", "LupiraWeb.Server.dll"]
