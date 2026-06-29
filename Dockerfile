# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src
COPY ["NeonVertexApi.csproj", "./"]
RUN dotnet restore "NeonVertexApi.csproj"

FROM restore AS build
COPY . .
RUN dotnet build "NeonVertexApi.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "NeonVertexApi.csproj" -c Release -o /app/publish \
    --no-restore /p:UseAppHost=false

FROM restore AS migrations
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-ef --version 10.0.0
COPY . .
ENTRYPOINT ["dotnet", "ef"]

FROM base AS production
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "NeonVertexApi.dll"]
