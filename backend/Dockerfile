FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Calendary.slnx .
COPY src/Calendary.Api/Calendary.Api.csproj src/Calendary.Api/
COPY src/Calendary.Domain/Calendary.Domain.csproj src/Calendary.Domain/
COPY src/Calendary.Infrastructure/Calendary.Infrastructure.csproj src/Calendary.Infrastructure/
COPY src/Calendary.AI/Calendary.AI.csproj src/Calendary.AI/
RUN dotnet restore src/Calendary.Api/Calendary.Api.csproj

COPY src/ src/
RUN dotnet publish src/Calendary.Api/Calendary.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Calendary.Api.dll"]
