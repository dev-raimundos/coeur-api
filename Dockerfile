# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8000 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src
COPY ["coeur-api.slnx", "./"]
COPY ["src/SharedKernel/SharedKernel.csproj", "src/SharedKernel/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Api/Api.csproj", "src/Api/"]
COPY ["src/modules/Users/Users.csproj", "src/modules/Users/"]
COPY ["src/modules/Authentication/Authentication.csproj", "src/modules/Authentication/"]
COPY ["src/modules/Shopping/Shopping.csproj", "src/modules/Shopping/"]
RUN dotnet restore "src/Api/Api.csproj"

FROM restore AS build
COPY . .
RUN dotnet build "src/Api/Api.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "src/Api/Api.csproj" -c Release -o /app/publish \
    --no-restore /p:UseAppHost=false

FROM restore AS migrations
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-ef --version 10.0.0
COPY . .
ENTRYPOINT ["dotnet", "ef", "--project", "src/Infrastructure", "--startup-project", "src/Api"]

FROM base AS production
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "Api.dll"]