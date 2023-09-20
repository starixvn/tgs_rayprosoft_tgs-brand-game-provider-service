#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.Brand.GameProvider.Service/Tgs.Brand.GameProvider.Service.csproj", "Tgs.Brand.GameProvider.Service/"]
COPY ["Tgs.Brand.GameProvider.Core/Tgs.Brand.GameProvider.Core.csproj", "Tgs.Brand.GameProvider.Core/"]
COPY ["Tgs.Brand.GameProvider.Domain/Tgs.Brand.GameProvider.Domain.csproj", "Tgs.Brand.GameProvider.Domain/"]
COPY ["Tgs.Brand.GameProvider.Repositories/Tgs.Brand.GameProvider.Repositories.csproj", "Tgs.Brand.GameProvider.Repositories/"]
RUN dotnet restore "Tgs.Brand.GameProvider.Service/Tgs.Brand.GameProvider.Service.csproj"
COPY . .
WORKDIR "/src/Tgs.Brand.GameProvider.Service"
RUN dotnet build "Tgs.Brand.GameProvider.Service.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.Brand.GameProvider.Service.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.Brand.GameProvider.Service.dll"]