# Вибираємо офіційний образ .NET для запуску додатку
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Вибираємо SDK образ для збирання додатку
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Calendary.Api/Calendary.Api.csproj", "Calendary.Api/"]
RUN dotnet restore "Calendary.Api/Calendary.Api.csproj"

COPY . .
WORKDIR "/src/Calendary.Api"
RUN dotnet build "Calendary.Api.csproj" -c Release -o /app/build

# Публікуємо додаток
FROM build AS publish
RUN dotnet publish "Calendary.Api.csproj" -c Release -o /app/publish

# Копіюємо опублікований додаток в остаточний образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Calendary.Api.dll"]