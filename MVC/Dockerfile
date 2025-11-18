# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install curl pour le HealthCheck
USER root
RUN apt-get update && apt-get install -y
RUN apt-get install curl -y

# Install ping tool for diagnostic
USER root
RUN apt-get update && apt-get install -y iputils-ping

# Pour le managed identity en Dev seulement.
ENV AZURE_CLIENT_ID="49b8dde5-2dda-4a7c-b316-fb4f6a16912b"
ENV AZURE_CLIENT_SECRET="kTn8Q~LTtZjeGpvbLxobiU04SVRJIC4BPpLh~bX1"
ENV AZURE_TENANT_ID="a3367121-31c1-4410-860a-5703cb95a5b4"

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MVC/MVC.csproj", "MVC/"]
RUN dotnet restore "./MVC/MVC.csproj"
COPY . .
WORKDIR "/src/MVC"
RUN dotnet build "MVC.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MVC.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MVC.dll"]
