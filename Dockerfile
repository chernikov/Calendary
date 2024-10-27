# Вибираємо офіційний образ .NET для запуску додатку
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80



# Встановлюємо необхідні пакети для ImageSharp
RUN apt-get update && apt-get install -y \
    libc6 \
    libgdiplus \
    libjpeg-turbo8 \
    libpng16-16 \
    && rm -rf /var/lib/apt/lists/*

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