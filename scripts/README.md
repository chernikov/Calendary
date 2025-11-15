# Calendary Scripts

Скрипти для автоматизації збірки, тестування та запуску проекту Calendary.

## Основні скрипти

### Повна збірка та тестування

- **`build.bat`** - Збірка всього проекту (Backend + Frontend)
  ```cmd
  .\scripts\build.bat
  ```
  - Restore та build .NET solution
  - npm install та build Angular app

- **`test.bat`** - Запуск всіх тестів (Backend + Frontend)
  ```cmd
  .\scripts\test.bat
  ```
  - .NET unit tests
  - Angular Jasmine/Karma tests

- **`build-and-test.bat`** - Збірка та тестування разом
  ```cmd
  .\scripts\build-and-test.bat
  ```

### Запуск сервісів

- **`run.bat`** - Запуск всіх сервісів у окремих вікнах
  ```cmd
  .\scripts\run.bat
  ```
  - Calendary.Api (http://localhost:5000)
  - Calendary.Consumer (RabbitMQ)
  - Calendary.Ng (http://localhost:4200)

## Окремі скрипти

### Backend (.NET)

- **`build-backend.bat`** - Збірка тільки .NET проекту
- **`test-backend.bat`** - Тестування тільки .NET проекту

### Frontend (Angular)

- **`build-frontend.bat`** - Збірка тільки Angular проекту
- **`test-frontend.bat`** - Тестування тільки Angular проекту

## Використання

### Перед першим запуском:
```cmd
.\scripts\build.bat
```

### Розробка:
```cmd
.\scripts\run.bat
```

### CI/CD pipeline:
```cmd
.\scripts\build-and-test.bat
```

### Тільки backend:
```cmd
.\scripts\build-backend.bat
.\scripts\test-backend.bat
```

### Тільки frontend:
```cmd
.\scripts\build-frontend.bat
.\scripts\test-frontend.bat
```

## Вимоги

- .NET 9.0 SDK
- Node.js 18+ та npm
- Google Chrome (для Angular тестів)

## Примітки

- Всі скрипти повертають exit code 0 при успіху та ненульовий код при помилці
- Фронтенд тести виконуються в headless режимі (ChromeHeadless)
- Backend збирається в Release конфігурації
- Frontend збирається в production режимі
