FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["source/Server/AkkaServer/AkkaServer.fsproj", "source/Server/AkkaServer/"]
RUN dotnet restore "source/Server/AkkaServer/AkkaServer.fsproj"
COPY . .
WORKDIR "/src/source/Server/AkkaServer"
RUN dotnet build "AkkaServer.fsproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AkkaServer.fsproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AkkaServer.dll"]
