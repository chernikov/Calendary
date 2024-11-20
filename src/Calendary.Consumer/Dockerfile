# Вказуємо базовий образ .NET для запуску
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Базовий образ для створення додатка
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копіюємо .csproj файли
COPY Calendary.Consumer/Calendary.Consumer.csproj Calendary.Consumer/
COPY Calendary.Core/Calendary.Core.csproj Calendary.Core/
COPY Calendary.Model/Calendary.Model.csproj Calendary.Model/
COPY Calendary.Repos/Calendary.Repos.csproj Calendary.Repos/

# Відновлюємо залежності
RUN dotnet restore Calendary.Consumer/Calendary.Consumer.csproj

# Копіюємо всі файли для збірки
COPY Calendary.Consumer/ Calendary.Consumer/
COPY Calendary.Repos/ Calendary.Repos/
COPY Calendary.Core/ Calendary.Core/
COPY Calendary.Model/ Calendary.Model/

# Збираємо проект
WORKDIR /src/Calendary.Consumer
RUN dotnet build Calendary.Consumer.csproj -c Release -o /app/build

# Публікуємо додаток
FROM build AS publish
RUN dotnet publish Calendary.Consumer.csproj -c Release -o /app/publish

# Створюємо фінальний образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Calendary.Consumer.dll"]