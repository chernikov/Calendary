# Вибираємо офіційний образ .NET для запуску додатку
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Вибираємо SDK образ для збирання додатку
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish Calendary.Api -c Release -o /app/publish

# Копіюємо опублікований додаток в остаточний образ
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Calendary.Api.dll"]