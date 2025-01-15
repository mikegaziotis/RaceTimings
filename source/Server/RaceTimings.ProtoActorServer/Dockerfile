FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["source/Server/RaceTimings.ProtoActorServer/RaceTimings.ProtoActorServer.csproj", "source/Server/RaceTimings.ProtoActorServer/"]
RUN dotnet restore "source/Server/RaceTimings.ProtoActorServer/RaceTimings.ProtoActorServer.csproj"
COPY . .
WORKDIR "/src/source/Server/RaceTimings.ProtoActorServer"
RUN dotnet build "RaceTimings.ProtoActorServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RaceTimings.ProtoActorServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaceTimings.ProtoActorServer.dll"]
