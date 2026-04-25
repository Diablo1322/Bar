# BAR/Dockerfile — в корне решения

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BAR/BAR.csproj", "BAR/"]
RUN dotnet restore "BAR/BAR.csproj"

COPY . .
WORKDIR "/src/BAR"

RUN dotnet build "BAR.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BAR.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BAR.dll"]