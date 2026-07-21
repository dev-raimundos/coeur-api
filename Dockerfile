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
COPY ["src/Modules/Users/Users.Domain/Users.Domain.csproj", "src/Modules/Users/Users.Domain/"]
COPY ["src/Modules/Users/Users.Application/Users.Application.csproj", "src/Modules/Users/Users.Application/"]
COPY ["src/Modules/Users/Users.Infrastructure/Users.Infrastructure.csproj", "src/Modules/Users/Users.Infrastructure/"]
COPY ["src/Modules/Users/Users.Presentation/Users.Presentation.csproj", "src/Modules/Users/Users.Presentation/"]
COPY ["src/Modules/Authentication/Authentication.Application/Authentication.Application.csproj", "src/Modules/Authentication/Authentication.Application/"]
COPY ["src/Modules/Authentication/Authentication.Infrastructure/Authentication.Infrastructure.csproj", "src/Modules/Authentication/Authentication.Infrastructure/"]
COPY ["src/Modules/Authentication/Authentication.Presentation/Authentication.Presentation.csproj", "src/Modules/Authentication/Authentication.Presentation/"]
COPY ["src/Modules/Shopping/Shopping.Domain/Shopping.Domain.csproj", "src/Modules/Shopping/Shopping.Domain/"]
COPY ["src/Modules/Shopping/Shopping.Application/Shopping.Application.csproj", "src/Modules/Shopping/Shopping.Application/"]
COPY ["src/Modules/Shopping/Shopping.Infrastructure/Shopping.Infrastructure.csproj", "src/Modules/Shopping/Shopping.Infrastructure/"]
COPY ["src/Modules/Shopping/Shopping.Presentation/Shopping.Presentation.csproj", "src/Modules/Shopping/Shopping.Presentation/"]
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
