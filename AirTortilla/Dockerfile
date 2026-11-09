# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["AirTortilla.csproj", "./"]

RUN dotnet restore "AirTortilla.csproj"

COPY . .

RUN dotnet publish "AirTortilla.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

RUN mkdir -p /app/registros /app/rawData

ENTRYPOINT ["dotnet", "AirTortilla.dll"]
