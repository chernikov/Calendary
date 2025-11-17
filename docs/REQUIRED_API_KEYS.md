# 🔑 Required API Keys & Configuration - Calendary

**Last Updated**: 2025-11-17
**For Production Deployment**

---

## 📋 Overview

Calendary requires multiple API keys and configuration secrets for full functionality. This document lists all required keys, where to obtain them, and how to configure them.

---

## 🔴 CRITICAL (Required for Core Functionality)

### 1. JWT Secret Key
**Purpose**: User authentication and authorization
**Required for**: Login, Register, Protected routes
**Priority**: 🔴 **CRITICAL**

**Configuration**:
```json
"Jwt": {
  "Key": "YOUR_SECRET_KEY_HERE_MINIMUM_32_CHARACTERS",
  "Issuer": "calendary.com.ua",
  "Audience": "calendary.com.ua"
}
```

**How to Generate**:
```bash
# PowerShell
$bytes = New-Object Byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)

# Or online: https://generate-secret.vercel.app/32
```

**Security**:
- ✅ Minimum 32 characters
- ✅ Use strong random string
- ✅ Different for Dev/Staging/Production
- ❌ Never commit to git

---

### 2. Database Connection String
**Purpose**: MS SQL Server database connection
**Required for**: All data operations
**Priority**: 🔴 **CRITICAL**

**Configuration**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=CalendaryDb;User Id=calendary_user;Password=YOUR_PASSWORD;MultipleActiveResultSets=True;TrustServerCertificate=True;Encrypt=True;"
}
```

**Production Setup**:
```sql
-- Create database
CREATE DATABASE CalendaryDb;
GO

-- Create user
CREATE LOGIN calendary_user WITH PASSWORD = 'YourStrongPassword123!';
GO

USE CalendaryDb;
CREATE USER calendary_user FOR LOGIN calendary_user;
GO

-- Grant permissions
ALTER ROLE db_owner ADD MEMBER calendary_user;
GO
```

**Security**:
- ✅ Use SQL Authentication (not Windows Auth for production)
- ✅ Strong password (min 12 chars, mixed case, numbers, symbols)
- ✅ Enable encryption (TrustServerCertificate=True for self-signed certs)
- ❌ Never use SA account

---

### 3. Replicate API Key
**Purpose**: AI image generation and LoRA model training
**Required for**: AI calendar creation (core feature)
**Priority**: 🔴 **CRITICAL**

**Configuration**:
```json
"ReplicateSettings": {
  "ApiKey": "r8_YOUR_API_KEY_HERE",
  "Owner": "chernikov",
  "TrainerModel": "ostris/flux-dev-lora-trainer",
  "TrainerVersion": "b6af14222e6bd9be257cbc1ea4afda3cd0503e1133083b9d1de0364d8568e6ef",
  "WebhookUrl": "https://calendary.com.ua/api/webhook",
  "Timeout": 30000,
  "MaxRetries": 3
}
```

**How to Get**:
1. Go to https://replicate.com
2. Sign up / Login
3. Go to Account Settings → API Tokens
4. Create new token: `calendary-production`
5. Copy token (starts with `r8_`)

**Webhook Setup**:
- Update `WebhookUrl` to your production domain
- Ensure endpoint `/api/webhook` is accessible
- Replicate will POST training/generation results here

**Cost**:
- Training (LoRA model): ~$0.50 per model
- Image generation: ~$0.03 per image
- Estimate: $10-20/month for moderate usage

**Documentation**: https://replicate.com/docs/reference/http

---

### 4. MonoBank Merchant Token
**Purpose**: Payment processing
**Required for**: Order payments
**Priority**: 🔴 **CRITICAL**

**Configuration**:
```json
"MonoBank": {
  "MerchantToken": "YOUR_MONOBANK_MERCHANT_TOKEN"
}
```

**How to Get**:
1. Open MonoBank business account
2. Apply for acquiring (payment processing)
3. Wait for approval (1-3 business days)
4. Go to MonoBank Personal Account → API Settings
5. Generate API Token for merchant

**Test Mode**:
- MonoBank provides test credentials
- Use for staging/development
- No real money charged

**Webhook**:
- MonoBank will POST payment status to your callback URL
- Configure in MonoBank Personal Account

**Documentation**: https://api.monobank.ua/docs/

---

## 🟡 HIGH PRIORITY (Important Features)

### 5. Nova Poshta API Key
**Purpose**: Shipping/delivery integration
**Required for**: Order shipping calculations, TTN generation
**Priority**: 🟡 **HIGH**

**Configuration**:
```json
"NovaPost": {
  "ApiKey": "YOUR_NOVA_POSHTA_API_KEY",
  "Endpoint": "https://api.novaposhta.ua/v2.0/json/",
  "SenderCity": "8d5a980d-391c-11dd-90d9-001a92567626",
  "SenderWarehouse": "YOUR_WAREHOUSE_REF",
  "SenderContact": "YOUR_CONTACT_REF",
  "SenderPhone": "+380XXXXXXXXX"
}
```

**How to Get**:
1. Go to https://my.novaposhta.ua/
2. Register / Login
3. Go to Settings → API Keys
4. Generate new key
5. Copy key

**Setup Sender Info**:
1. In Nova Poshta cabinet, create:
   - Sender contact person (will get Ref)
   - Preferred warehouse for sending (will get Ref)
2. Get Kyiv city Ref: `8d5a980d-391c-11dd-90d9-001a92567626`
3. Update configuration

**Test Mode**:
- Nova Poshta API works same in dev/prod
- Use real API key even for testing
- No separate test environment

**Documentation**: https://developers.novaposhta.ua/

---

### 6. SendGrid API Key
**Purpose**: Email notifications
**Required for**: Email verification, order confirmations, password reset
**Priority**: 🟡 **HIGH**

**Configuration**:
```json
"SendGrid": {
  "ApiKey": "SG.YOUR_SENDGRID_API_KEY"
}
```

**How to Get**:
1. Go to https://sendgrid.com
2. Sign up (Free tier: 100 emails/day)
3. Go to Settings → API Keys
4. Create API Key with "Full Access"
5. Copy key (starts with `SG.`)

**Email Templates**:
- Welcome email
- Email verification
- Password reset
- Order confirmation
- AI training completion

**Free Tier**:
- 100 emails/day forever free
- Sufficient for small-medium traffic
- Upgrade to paid for higher volume

**Alternative**: AWS SES, Mailgun, SMTP

**Documentation**: https://docs.sendgrid.com/

---

### 7. SMS Club API Key
**Purpose**: SMS notifications
**Required for**: SMS verification (optional)
**Priority**: 🟡 **MEDIUM**

**Configuration**:
```json
"SmsClub": {
  "ApiKey": "YOUR_SMS_CLUB_API_KEY"
}
```

**How to Get**:
1. Go to https://smsclub.mobi/
2. Register account
3. Go to API → API Keys
4. Generate new key
5. Top up balance

**Cost**:
- ~0.50 UAH per SMS (Ukraine)
- Pay-as-you-go
- Minimum top-up: 100 UAH

**Alternative**: Twilio (international), Vonage

**Documentation**: https://smsclub.mobi/api-documentation

---

## 🟢 OPTIONAL (Advanced Features)

### 8. OpenAI API Key
**Purpose**: AI prompt enhancement (optional feature)
**Required for**: Automatic prompt improvement
**Priority**: 🟢 **OPTIONAL**

**Configuration**:
```json
"OpenAI": {
  "ApiKey": "sk-YOUR_OPENAI_API_KEY",
  "Model": "gpt-4",
  "MaxTokens": 500,
  "Temperature": 0.7
}
```

**How to Get**:
1. Go to https://platform.openai.com
2. Sign up / Login
3. Go to API Keys
4. Create new secret key
5. Copy key (starts with `sk-`)

**Cost**:
- GPT-4: ~$0.03 per 1K tokens
- GPT-3.5-turbo: ~$0.002 per 1K tokens
- Use GPT-3.5-turbo for lower cost

**Usage**:
- Enhances user prompts before sending to Replicate
- Can be disabled by setting Provider to None

**Documentation**: https://platform.openai.com/docs/

---

### 9. Anthropic (Claude) API Key
**Purpose**: Alternative AI prompt enhancement
**Required for**: Prompt improvement (alternative to OpenAI)
**Priority**: 🟢 **OPTIONAL**

**Configuration**:
```json
"Anthropic": {
  "ApiKey": "sk-ant-YOUR_ANTHROPIC_API_KEY",
  "Model": "claude-3-sonnet-20240229",
  "MaxTokens": 500,
  "Temperature": 0.7
}
```

**How to Get**:
1. Go to https://console.anthropic.com
2. Sign up (waitlist may apply)
3. Go to API Keys
4. Create new key
5. Copy key (starts with `sk-ant-`)

**Cost**:
- Claude 3 Sonnet: ~$0.015 per 1K tokens
- Claude 3 Haiku: ~$0.0025 per 1K tokens

**Usage**:
- Alternative to OpenAI for prompt enhancement
- Set `PromptEnhancer.Provider` to "Anthropic"

**Documentation**: https://docs.anthropic.com/

---

### 10. RabbitMQ Credentials
**Purpose**: Message queue for async tasks
**Required for**: Background image generation
**Priority**: 🟡 **MEDIUM**

**Configuration**:
```json
"RabbitMQ": {
  "Host": "localhost",
  "User": "calendary_user",
  "Password": "YOUR_RABBITMQ_PASSWORD",
  "Queues": [
    "create-prediction"
  ]
}
```

**Production Setup**:
```bash
# Install RabbitMQ
sudo apt-get install rabbitmq-server

# Create user
sudo rabbitmqctl add_user calendary_user YOUR_PASSWORD
sudo rabbitmqctl set_user_tags calendary_user administrator
sudo rabbitmqctl set_permissions -p / calendary_user ".*" ".*" ".*"

# Enable management plugin
sudo rabbitmq-plugins enable rabbitmq_management
```

**Cloud Options**:
- CloudAMQP (free tier available)
- AWS MQ
- Azure Service Bus

**Default Credentials** (DEV ONLY):
- User: `guest`
- Password: `guest`
- ⚠️ Change for production!

**Documentation**: https://www.rabbitmq.com/documentation.html

---

## 🔧 Configuration Management

### Environment-Specific Configuration

**Development** (`appsettings.Development.json`):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CalendaryDb_Dev;..."
  },
  "ReplicateSettings": {
    "WebhookUrl": "https://localhost:7001/api/webhook"
  }
}
```

**Production** (`appsettings.Production.json`):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=CalendaryDb;..."
  },
  "ReplicateSettings": {
    "WebhookUrl": "https://calendary.com.ua/api/webhook"
  }
}
```

---

### User Secrets (Development)

**Setup**:
```bash
cd src/Calendary.Api
dotnet user-secrets init
```

**Add Secrets**:
```bash
# JWT
dotnet user-secrets set "Jwt:Key" "your-secret-key-here"

# Replicate
dotnet user-secrets set "ReplicateSettings:ApiKey" "r8_your_key"

# MonoBank
dotnet user-secrets set "MonoBank:MerchantToken" "your_token"

# Nova Poshta
dotnet user-secrets set "NovaPost:ApiKey" "your_key"
dotnet user-secrets set "NovaPost:SenderWarehouse" "warehouse_ref"
dotnet user-secrets set "NovaPost:SenderContact" "contact_ref"
dotnet user-secrets set "NovaPost:SenderPhone" "+380XXXXXXXXX"

# SendGrid
dotnet user-secrets set "SendGrid:ApiKey" "SG.your_key"

# SMS Club
dotnet user-secrets set "SmsClub:ApiKey" "your_key"

# OpenAI (optional)
dotnet user-secrets set "OpenAI:ApiKey" "sk-your_key"

# Anthropic (optional)
dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-your_key"

# Database
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=CalendaryDb;..."
```

**View Secrets**:
```bash
dotnet user-secrets list
```

---

### Environment Variables (Production)

**Docker Compose** (`docker-compose.yml`):
```yaml
services:
  api:
    environment:
      - Jwt__Key=${JWT_SECRET_KEY}
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - ReplicateSettings__ApiKey=${REPLICATE_API_KEY}
      - MonoBank__MerchantToken=${MONOBANK_TOKEN}
      - NovaPost__ApiKey=${NOVAPOSHTA_API_KEY}
      - SendGrid__ApiKey=${SENDGRID_API_KEY}
      - SmsClub__ApiKey=${SMSCLUB_API_KEY}
      - OpenAI__ApiKey=${OPENAI_API_KEY}
      - Anthropic__ApiKey=${ANTHROPIC_API_KEY}
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__User=${RABBITMQ_USER}
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
```

**`.env` file** (for docker-compose):
```bash
# JWT
JWT_SECRET_KEY=your-secret-key-here

# Database
DB_CONNECTION_STRING=Server=db;Database=CalendaryDb;User Id=sa;Password=YourPassword123!

# Replicate
REPLICATE_API_KEY=r8_your_key

# MonoBank
MONOBANK_TOKEN=your_token

# Nova Poshta
NOVAPOSHTA_API_KEY=your_key

# Email
SENDGRID_API_KEY=SG.your_key

# SMS
SMSCLUB_API_KEY=your_key

# AI (Optional)
OPENAI_API_KEY=sk-your_key
ANTHROPIC_API_KEY=sk-ant-your_key

# RabbitMQ
RABBITMQ_USER=calendary_user
RABBITMQ_PASSWORD=your_password
```

⚠️ **Never commit `.env` file to git!** Add to `.gitignore`

---

### Azure App Service Configuration

**Portal**:
1. Go to Azure Portal → Your App Service
2. Configuration → Application settings
3. Add each setting as key-value pair:
   - `Jwt__Key` = your-secret-key
   - `ConnectionStrings__DefaultConnection` = your-connection-string
   - etc.

**Azure CLI**:
```bash
az webapp config appsettings set --resource-group YourRG --name YourAppName --settings \
  Jwt__Key="your-secret-key" \
  ReplicateSettings__ApiKey="r8_your_key" \
  MonoBank__MerchantToken="your_token"
```

---

## ✅ Configuration Checklist

### Pre-Deployment
- [ ] JWT secret key generated (min 32 chars)
- [ ] Database connection string configured
- [ ] Replicate API key obtained
- [ ] MonoBank merchant token obtained
- [ ] Nova Poshta API key obtained
- [ ] SendGrid API key obtained
- [ ] SMS Club API key obtained (if using SMS)
- [ ] OpenAI/Anthropic API key (if using prompt enhancement)
- [ ] RabbitMQ credentials configured
- [ ] All secrets stored securely (not in git)
- [ ] Production webhook URLs updated

### Post-Deployment
- [ ] Test authentication (JWT)
- [ ] Test database connection
- [ ] Test AI image generation (Replicate)
- [ ] Test payment flow (MonoBank)
- [ ] Test shipping calculation (Nova Poshta)
- [ ] Test email sending (SendGrid)
- [ ] Test SMS sending (if enabled)
- [ ] Verify webhook endpoints working
- [ ] Check RabbitMQ queue processing

---

## 🔒 Security Best Practices

1. **Never commit secrets to git**
   - Use `.gitignore` for `appsettings.*.json` with secrets
   - Use user secrets (dev) or env variables (prod)

2. **Use strong passwords**
   - Min 16 characters for production
   - Mix uppercase, lowercase, numbers, symbols
   - Use password generator

3. **Rotate keys regularly**
   - API keys: every 90 days
   - Database passwords: every 180 days
   - JWT secret: every 365 days

4. **Limit key permissions**
   - Use least privilege principle
   - Separate keys for dev/staging/prod
   - Disable unused keys

5. **Monitor usage**
   - Check API usage in provider dashboards
   - Set up billing alerts
   - Review logs for suspicious activity

6. **Encrypt in transit**
   - Use HTTPS for all APIs
   - Enable SSL for database connections
   - Use TLS for RabbitMQ

---

## 📊 Cost Estimate (Monthly)

| Service | Tier | Cost | Usage |
|---------|------|------|-------|
| **Replicate** | Pay-as-you-go | $10-50 | 20-100 models, 240-1200 images |
| **MonoBank** | Merchant fees | 1.5-2.5% | Per transaction |
| **Nova Poshta** | Free | $0 | API is free |
| **SendGrid** | Free | $0 | Up to 100 emails/day |
| **SMS Club** | Pay-as-you-go | $5-20 | ~10-40 SMS |
| **OpenAI** | Pay-as-you-go | $5-15 | ~500-1000 requests |
| **RabbitMQ** | Self-hosted | $0 | Included in server |
| **Database** | Cloud | $10-30 | Azure SQL / AWS RDS |
| **TOTAL** | | **$30-150** | Based on usage |

---

## 🆘 Troubleshooting

### Common Issues

**"JWT Key not configured"**
- Check `Jwt:Key` is set in configuration
- Verify minimum 32 characters
- Check user secrets or environment variables

**"Replicate API authentication failed"**
- Verify API key starts with `r8_`
- Check key is active in Replicate dashboard
- Ensure no trailing spaces in configuration

**"Database connection failed"**
- Check connection string format
- Verify server/database exists
- Test SQL authentication credentials
- Check firewall allows connections

**"MonoBank payment failed"**
- Verify merchant token is correct
- Check MonoBank merchant account is active
- Ensure webhook URL is accessible

**"Email not sending"**
- Check SendGrid API key is valid
- Verify sender email is verified in SendGrid
- Check email quota not exceeded

**"RabbitMQ connection refused"**
- Verify RabbitMQ service is running
- Check host/port configuration
- Verify credentials are correct

---

## 📚 Additional Resources

- [Replicate Documentation](https://replicate.com/docs)
- [MonoBank API Docs](https://api.monobank.ua/docs/)
- [Nova Poshta API Docs](https://developers.novaposhta.ua/)
- [SendGrid Docs](https://docs.sendgrid.com/)
- [OpenAI API Reference](https://platform.openai.com/docs/api-reference)
- [RabbitMQ Tutorials](https://www.rabbitmq.com/tutorials.html)

---

**Last Updated**: 2025-11-17
**Version**: 1.0
**Generated by**: Claude Code
