# Робоча папка DevOps архітектора Calendary

## Про роль

DevOps архітектор відповідає за інфраструктуру, deployment, CI/CD, моніторинг та безпеку платформи Calendary.

---

## Поточна інфраструктура

### Платформа: DigitalOcean (поточна)
**Налаштування:**
- Droplet(s) з Ubuntu 22.04
- Docker + Docker Compose
- Nginx як reverse proxy та для static files
- Volumes для persistency (MS SQL, RabbitMQ, uploads)

**Компоненти:**
```yaml
services:
  - calendary_db (MS SQL Server)
  - calendary_rabbitmq (RabbitMQ)
  - calendary_api (.NET API)
  - calendary_ng (Angular Admin)
  - calendary_consumer (RabbitMQ Consumer)
```

### Майбутня платформа: Azure
**Заплановані сервіси:**
- Azure App Service / Container Apps (для API та Frontend)
- Azure SQL Database
- Azure Service Bus (замість RabbitMQ)
- Azure Blob Storage (для файлів)
- Azure CDN
- Azure Key Vault (для secrets)
- Azure Monitor + Application Insights

---

## Архітектура на DigitalOcean

### Поточна структура

```
┌─────────────────────────────────────────────┐
│           CloudFlare CDN (optional)         │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│        DigitalOcean Droplet(s)              │
│  ┌───────────────────────────────────────┐  │
│  │          Nginx (80/443)               │  │
│  │  - SSL Termination                    │  │
│  │  - Reverse Proxy                      │  │
│  │  - Static Files (Angular)             │  │
│  └───────┬───────────────┬───────────────┘  │
│          │               │                   │
│  ┌───────▼──────┐  ┌────▼──────────┐        │
│  │ calendary_ng │  │ calendary_api │        │
│  │  (Angular)   │  │   (.NET 9)    │        │
│  │   Port 80    │  │  Port 8080    │        │
│  └──────────────┘  └────┬──────────┘        │
│                          │                   │
│         ┌────────────────┼──────────┐        │
│         │                │          │        │
│  ┌──────▼─────┐  ┌───────▼────┐ ┌──▼───────┐│
│  │ calendary_ │  │ calendary_ │ │calendary_││
│  │    db      │  │ rabbitmq   │ │ consumer ││
│  │  (MS SQL)  │  │            │ │          ││
│  └────────────┘  └────────────┘ └──────────┘│
│                                               │
│  Volumes:                                     │
│  - mssqlsystem_calendary                     │
│  - mssqluser_calendary                       │
│  - rabbitmq_data                             │
│  - /calendary (uploads)                      │
└───────────────────────────────────────────────┘
```

### Networking
- **Bridge Network**: `calendary` (subnet: 20.0.0.0/24)
- **Exposed Ports**:
  - 80 (HTTP)
  - 443 (HTTPS)
  - 8080 (API - internal через Nginx)
  - 1443 (MS SQL - для dev доступу)
  - 15672 (RabbitMQ Management - для dev)

---

## Архітектура на Azure (майбутня)

### Заплановані компоненти

```
┌────────────────────────────────────────────┐
│         Azure Front Door / CDN             │
│           (Global Distribution)            │
└──────────────────┬─────────────────────────┘
                   │
         ┌─────────┴──────────┐
         │                    │
┌────────▼────────┐  ┌────────▼──────────┐
│  App Service    │  │  Container Apps   │
│  (Angular SPA)  │  │   (.NET API)      │
│                 │  │  Auto-scaling     │
└─────────────────┘  └────────┬──────────┘
                              │
        ┌─────────────────────┼─────────────┐
        │                     │             │
┌───────▼────────┐  ┌─────────▼────┐  ┌────▼──────┐
│  Azure SQL DB  │  │ Service Bus  │  │   Blob    │
│  (PaaS)        │  │ (Queues)     │  │  Storage  │
│  - Geo-replica │  │              │  │ (uploads) │
└────────────────┘  └──────────────┘  └───────────┘
        │
┌───────▼────────┐
│  Key Vault     │
│  (Secrets)     │
└────────────────┘

Monitoring & Logging:
- Azure Monitor
- Application Insights
- Log Analytics Workspace
```

### Azure Services детально

#### 1. Azure App Service / Container Apps
**API (.NET):**
- Auto-scaling based on CPU/Memory
- Deployment slots (staging, production)
- Managed SSL certificates
- Integration з Key Vault

**Frontend (Angular):**
- Static Web App або App Service
- CDN integration
- Custom domain

#### 2. Azure SQL Database
- Service Tier: Standard S2 або вище
- Geo-replication для disaster recovery
- Automated backups
- Point-in-time restore
- Advanced security (TDE, Always Encrypted)

#### 3. Azure Service Bus
- Замінює RabbitMQ
- Topics/Subscriptions model
- Dead-letter queues
- Message sessions

#### 4. Azure Blob Storage
- Hot tier для active uploads
- Cool tier для архіву
- CDN integration
- Lifecycle management

#### 5. Azure Key Vault
- Secrets management (API keys, connection strings)
- Certificates
- Managed identities integration

---

## CI/CD Pipeline

### GitHub Actions Workflow

**Файл: `.github/workflows/deploy.yml`**

```yaml
name: Deploy Calendary

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  DOTNET_VERSION: '9.0.x'
  NODE_VERSION: '20.x'

jobs:
  # ==================== BACKEND ====================
  build-api:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Test
        run: dotnet test --no-restore --verbosity normal

      - name: Publish
        run: dotnet publish src/Calendary.Api -c Release -o ./publish

      - name: Upload artifact
        uses: actions/upload-artifact@v4
        with:
          name: api-artifact
          path: ./publish

  build-consumer:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      - name: Build & Publish Consumer
        run: dotnet publish src/Calendary.Consumer -c Release -o ./publish-consumer
      - name: Upload artifact
        uses: actions/upload-artifact@v4
        with:
          name: consumer-artifact
          path: ./publish-consumer

  # ==================== FRONTEND ====================
  build-angular:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}

      - name: Install dependencies
        working-directory: ./src/Calendary.Ng
        run: npm ci

      - name: Build Angular
        working-directory: ./src/Calendary.Ng
        run: npm run build -- --configuration production

      - name: Upload artifact
        uses: actions/upload-artifact@v4
        with:
          name: angular-artifact
          path: ./src/Calendary.Ng/dist

  # ==================== DOCKER BUILD ====================
  docker-build-push:
    needs: [build-api, build-consumer, build-angular]
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v4

      - name: Login to Docker Hub
        uses: docker/login-action@v3
        with:
          username: ${{ secrets.DOCKER_USERNAME }}
          password: ${{ secrets.DOCKER_PASSWORD }}

      - name: Build and push API
        uses: docker/build-push-action@v5
        with:
          context: .
          file: ./Dockerfile
          push: true
          tags: chernikov/calendary.api:latest

      - name: Build and push Angular
        uses: docker/build-push-action@v5
        with:
          context: ./src/Calendary.Ng
          file: ./src/Calendary.Ng/Dockerfile
          push: true
          tags: chernikov/calendary.ng:latest

      - name: Build and push Consumer
        uses: docker/build-push-action@v5
        with:
          context: .
          file: ./Dockerfile.consumer
          push: true
          tags: chernikov/calendary.consumer:latest

  # ==================== DEPLOY ====================
  deploy-digitalocean:
    needs: docker-build-push
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - name: Deploy to DigitalOcean
        uses: appleboy/ssh-action@master
        with:
          host: ${{ secrets.DO_HOST }}
          username: ${{ secrets.DO_USERNAME }}
          key: ${{ secrets.DO_SSH_KEY }}
          script: |
            cd /opt/calendary
            docker-compose pull
            docker-compose up -d
            docker system prune -f
```

### Secrets потрібні в GitHub

```
DOCKER_USERNAME
DOCKER_PASSWORD
DO_HOST (IP droplet'а)
DO_USERNAME
DO_SSH_KEY
```

---

## Deployment процес

### DigitalOcean Deployment

#### Початкове налаштування Droplet

```bash
# 1. Оновлення системи
sudo apt update && sudo apt upgrade -y

# 2. Встановлення Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER

# 3. Встановлення Docker Compose
sudo apt install docker-compose-plugin -y

# 4. Створення директорій
sudo mkdir -p /opt/calendary
sudo mkdir -p /calendary  # для uploads

# 5. Створення volumes
docker volume create mssqlsystem_calendary
docker volume create mssqluser_calendary
docker volume create rabbitmq_data

# 6. Налаштування firewall
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable

# 7. Клонування репо або завантаження docker-compose.yml
cd /opt/calendary
# Завантажити docker-compose.yml

# 8. Налаштування environment variables
nano .env

# 9. Запуск
docker-compose up -d

# 10. Перевірка логів
docker-compose logs -f
```

#### Оновлення (через CI/CD або manually)

```bash
cd /opt/calendary
docker-compose pull
docker-compose up -d
docker system prune -f  # очистка старих images
```

### Azure Deployment (майбутній)

#### Підготовка Azure Resources

```bash
# Azure CLI
az login

# Створення Resource Group
az group create --name calendary-rg --location westeurope

# Створення Azure SQL
az sql server create \
  --name calendary-sql-server \
  --resource-group calendary-rg \
  --location westeurope \
  --admin-user sqladmin \
  --admin-password <strong-password>

az sql db create \
  --resource-group calendary-rg \
  --server calendary-sql-server \
  --name calendary-db \
  --service-objective S2

# Створення Service Bus
az servicebus namespace create \
  --resource-group calendary-rg \
  --name calendary-sb \
  --location westeurope

az servicebus queue create \
  --resource-group calendary-rg \
  --namespace-name calendary-sb \
  --name calendar-processing

# Створення Storage Account
az storage account create \
  --name calendarystorage \
  --resource-group calendary-rg \
  --location westeurope \
  --sku Standard_LRS

# Створення Container Apps Environment
az containerapp env create \
  --name calendary-env \
  --resource-group calendary-rg \
  --location westeurope

# Deploy API
az containerapp create \
  --name calendary-api \
  --resource-group calendary-rg \
  --environment calendary-env \
  --image chernikov/calendary.api:latest \
  --target-port 8080 \
  --ingress external \
  --env-vars \
    ConnectionStrings__DefaultConnection="<connection-string>" \
    MonoBank__MerchantToken="<token>"
```

---

## Monitoring та Logging

### Поточний setup (DigitalOcean)

#### Serilog configuration (appsettings.json)

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "/app/logs/calendary-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  }
}
```

#### Docker logs monitoring

```bash
# Реального часу
docker-compose logs -f calendary_api

# Фільтрування
docker-compose logs calendary_api | grep ERROR

# Останні 100 рядків
docker-compose logs --tail=100 calendary_api
```

#### Встановлення Prometheus + Grafana (опціонально)

```yaml
# Додати до docker-compose.yml
prometheus:
  image: prom/prometheus
  volumes:
    - ./prometheus.yml:/etc/prometheus/prometheus.yml
  ports:
    - "9090:9090"

grafana:
  image: grafana/grafana
  ports:
    - "3000:3000"
  environment:
    - GF_SECURITY_ADMIN_PASSWORD=admin
```

### Azure setup (майбутній)

#### Application Insights

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry(
    builder.Configuration["ApplicationInsights:ConnectionString"]
);
```

#### Log Analytics Workspace

```bash
az monitor log-analytics workspace create \
  --resource-group calendary-rg \
  --workspace-name calendary-logs
```

#### Alerts

```bash
# CPU Alert
az monitor metrics alert create \
  --name high-cpu-alert \
  --resource-group calendary-rg \
  --scopes /subscriptions/.../calendary-api \
  --condition "avg Percentage CPU > 80" \
  --description "Alert when CPU > 80%"
```

---

## Backup стратегія

### DigitalOcean

#### Database Backups

```bash
# Automated backup script
#!/bin/bash
BACKUP_DIR="/backups/mssql"
DATE=$(date +%Y%m%d_%H%M%S)

docker exec calendary_db /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'yourStrong(!)Password' \
  -Q "BACKUP DATABASE calendary TO DISK = '/var/opt/mssql/backup/calendary_$DATE.bak'"

docker cp calendary_db:/var/opt/mssql/backup/calendary_$DATE.bak \
  $BACKUP_DIR/calendary_$DATE.bak

# Видалення старих backup (>7 днів)
find $BACKUP_DIR -type f -name "*.bak" -mtime +7 -delete

# Upload to S3-compatible storage (опціонально)
# aws s3 cp $BACKUP_DIR/calendary_$DATE.bak s3://calendary-backups/
```

**Cron job:**
```bash
# Щоденний backup о 2:00
0 2 * * * /opt/calendary/scripts/backup-db.sh
```

#### Uploads Backup

```bash
# Rsync до remote storage
rsync -avz /calendary/uploads/ user@backup-server:/backups/calendary/uploads/
```

#### DigitalOcean Snapshots

- Автоматичні snapshots Droplet'а (weekly)
- Volume snapshots

### Azure

- **Azure SQL**: Automated backups (7-35 днів retention)
- **Blob Storage**: Soft delete (7-365 днів)
- **Geo-redundancy**: LRS → GRS

---

## Security

### SSL/TLS

#### DigitalOcean з Let's Encrypt

```bash
# Встановлення certbot
sudo apt install certbot python3-certbot-nginx -y

# Отримання сертифікату
sudo certbot --nginx -d calendary.com -d www.calendary.com

# Автоматичне оновлення
sudo certbot renew --dry-run
```

#### Nginx SSL config

```nginx
server {
    listen 443 ssl http2;
    server_name calendary.com;

    ssl_certificate /etc/letsencrypt/live/calendary.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/calendary.com/privkey.pem;

    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;

    location / {
        proxy_pass http://calendary_ng;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }

    location /api {
        proxy_pass http://calendary_api:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### Secrets Management

#### DigitalOcean (.env file)

```bash
# Обмеження доступу
chmod 600 .env
chown root:root .env
```

#### Azure Key Vault (майбутній)

```bash
# Створення Key Vault
az keyvault create \
  --name calendary-kv \
  --resource-group calendary-rg \
  --location westeurope

# Додавання secrets
az keyvault secret set \
  --vault-name calendary-kv \
  --name MonoBankToken \
  --value "<token>"

# Managed Identity для Container App
az containerapp identity assign \
  --name calendary-api \
  --resource-group calendary-rg \
  --system-assigned

# Grant access
az keyvault set-policy \
  --name calendary-kv \
  --object-id <identity-principal-id> \
  --secret-permissions get list
```

### Firewall Rules

#### DigitalOcean

```bash
# UFW
sudo ufw status
sudo ufw allow from <office-ip> to any port 1443  # SQL тільки з офісу
sudo ufw allow from <office-ip> to any port 15672  # RabbitMQ Management
```

#### Azure

```bash
# Network Security Groups
az network nsg rule create \
  --resource-group calendary-rg \
  --nsg-name calendary-nsg \
  --name allow-https \
  --priority 100 \
  --source-address-prefixes '*' \
  --destination-port-ranges 443 \
  --access Allow \
  --protocol Tcp
```

---

## Disaster Recovery Plan

### RTO/RPO Targets
- **RTO** (Recovery Time Objective): 4 години
- **RPO** (Recovery Point Objective): 1 година

### Backup Schedule
- Database: Щоденно о 02:00
- Uploads: Щоденно о 03:00
- Configuration: При кожній зміні

### Recovery Procedures

#### Database Recovery

```bash
# Restore з backup
docker exec -it calendary_db /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'yourStrong(!)Password' \
  -Q "RESTORE DATABASE calendary FROM DISK = '/var/opt/mssql/backup/calendary_YYYYMMDD.bak' WITH REPLACE"
```

#### Full System Recovery

```bash
# 1. Створити новий Droplet
# 2. Встановити Docker
# 3. Відновити volumes з backup
# 4. Відновити docker-compose.yml та .env
# 5. Запустити containers
docker-compose up -d
```

---

## Performance Optimization

### Database

```sql
-- Індекси для часто запитуваних таблиць
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_Orders_CreatedAt ON Orders(CreatedAt);

-- Query optimization
UPDATE STATISTICS Orders;
```

### Caching

```csharp
// Redis integration
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
});
```

### CDN для static files

- CloudFlare CDN для зображень та статики
- Azure CDN після міграції

---

## Cost Optimization

### DigitalOcean
**Поточні витрати (приблизно):**
- Droplet (4GB RAM): $24/міс
- Volumes (100GB): $10/міс
- Backups: $5/міс
- **Total**: ~$40/міс

### Azure (очікувані витрати)
- App Service (B2): $70/міс
- Azure SQL (S2): $120/міс
- Service Bus: $10/міс
- Blob Storage: $5/міс
- CDN: $10/міс
- **Total**: ~$215/міс

**Оптимізація:**
- Reserved Instances (1-3 роки) - економія до 40%
- Auto-scaling - платити тільки за використання
- Cool tier для старих файлів

---

## Checklist для запуску

### Pre-deployment
- [ ] Код пройшов code review
- [ ] Всі тести проходять
- [ ] Database migrations готові
- [ ] Environment variables налаштовані
- [ ] Secrets захищені
- [ ] SSL сертифікати готові

### Deployment
- [ ] Backup поточної версії
- [ ] Deploy нової версії
- [ ] Smoke tests
- [ ] Health check endpoints
- [ ] Перевірка логів

### Post-deployment
- [ ] Моніторинг метрик
- [ ] Перевірка alerts
- [ ] Комунікація з командою
- [ ] Документування змін

---

## Корисні команди

### Docker

```bash
# Статус всіх контейнерів
docker-compose ps

# Перезапуск одного сервісу
docker-compose restart calendary_api

# Логи
docker-compose logs -f --tail=100 calendary_api

# Exec в контейнер
docker exec -it calendary_api /bin/bash

# Видалення всього
docker-compose down -v

# Очистка
docker system prune -a --volumes
```

### MS SQL

```bash
# Підключення до SQL
docker exec -it calendary_db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'yourStrong(!)Password'

# Backup
BACKUP DATABASE calendary TO DISK = '/var/opt/mssql/backup/calendary.bak';

# Restore
RESTORE DATABASE calendary FROM DISK = '/var/opt/mssql/backup/calendary.bak' WITH REPLACE;
```

### Nginx

```bash
# Перевірка конфігурації
nginx -t

# Reload
nginx -s reload

# Логи
tail -f /var/log/nginx/access.log
tail -f /var/log/nginx/error.log
```

---

## Контакти

### Провайдери
- DigitalOcean Support
- Azure Support (після міграції)
- Domain registrar

### Команда
- Project Manager: [ім'я]
- Backend Developer: [ім'я]
- Frontend Developer: [ім'я]

---

## Документація та ресурси

### Official Docs
- [Docker Documentation](https://docs.docker.com/)
- [.NET Docker](https://learn.microsoft.com/en-us/dotnet/core/docker/)
- [DigitalOcean Docs](https://docs.digitalocean.com/)
- [Azure Docs](https://docs.microsoft.com/en-us/azure/)

### Tools
- Docker Hub: hub.docker.com
- GitHub Actions: github.com/features/actions
- Azure Portal: portal.azure.com

### Monitoring
- Grafana Dashboards
- Azure Monitor Workbooks
