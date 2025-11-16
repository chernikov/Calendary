# Task 21: Реєстрація та автентифікація

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P0 (Критичний)
**Складність**: Висока
**Час**: 6-8 годин
**Відповідальний AI**: Claude
**Паралельно з**: Task 24, 25, 26

## Опис задачі

Імплементувати JWT-based автентифікацію: реєстрація, логін, token refresh, protected routes на фронтенді та бекенді.

## Проблема

Користувачі повинні мати можливість створити акаунт, авторизуватися та мати доступ до персональних функцій (профіль, замовлення, збережені календарі).

## Що треба зробити

### Backend

1. **Створити Authentication Service**
   - `src/Calendary.Application/Services/AuthService.cs`
   - Register (email, password)
   - Login (email, password) → JWT token
   - Refresh token mechanism
   - Password hashing (BCrypt або ASP.NET Identity)

2. **JWT Token Generation**
   - Access token (short-lived, 15 min)
   - Refresh token (long-lived, 7 days)
   - Claims: UserId, Email, Roles
   - Sign with secret key

3. **API Endpoints**
   - `POST /api/auth/register` - реєстрація
   - `POST /api/auth/login` - логін
   - `POST /api/auth/refresh` - оновити токен
   - `POST /api/auth/logout` - вихід (invalidate refresh token)

4. **Middleware для JWT**
   - Налаштувати JWT Bearer authentication
   - Validate tokens на кожен request
   - Authorize attribute для protected endpoints

### Frontend

5. **Створити Auth Context/Store**
   - `src/store/authStore.ts` (Zustand)
   - State: user, accessToken, isAuthenticated
   - Actions: login, register, logout, refreshToken

6. **Login/Register сторінки**
   - `src/app/auth/login/page.tsx`
   - `src/app/auth/register/page.tsx`
   - Forms з валідацією (React Hook Form + Zod)

7. **Protected Routes**
   - `src/middleware.ts` (Next.js middleware)
   - Redirect на /auth/login якщо не авторизований
   - Protect /profile, /editor, /checkout routes

8. **Axios Interceptor для токенів**
   - Додавати Authorization header до requests
   - Auto-refresh token коли expired
   - Redirect на login при 401

## Файли для створення/модифікації

### Backend
- `src/Calendary.Core/Interfaces/IAuthService.cs`
- `src/Calendary.Application/Services/AuthService.cs`
- `src/Calendary.API/Controllers/AuthController.cs`
- `src/Calendary.API/DTOs/Auth/RegisterRequest.cs`
- `src/Calendary.API/DTOs/Auth/LoginRequest.cs`
- `src/Calendary.API/DTOs/Auth/AuthResponse.cs`
- `src/Calendary.Core/Entities/RefreshToken.cs`
- `Program.cs` - налаштувати JWT

### Frontend
- `src/store/authStore.ts`
- `src/app/auth/login/page.tsx`
- `src/app/auth/register/page.tsx`
- `src/middleware.ts`
- `src/lib/axios.ts` - interceptor
- `src/components/features/auth/LoginForm.tsx`
- `src/components/features/auth/RegisterForm.tsx`

## Критерії успіху

- [ ] Можна зареєструватися (email, password)
- [ ] Email унікальний (валідація)
- [ ] Password хешується перед збереженням
- [ ] Можна авторизуватися (login)
- [ ] JWT токен повертається після login
- [ ] Токен додається до API requests
- [ ] Refresh token працює автоматично
- [ ] Protected routes redirect на login
- [ ] Logout працює (очищає токени)

## Залежності

Немає (незалежна задача)

## Блокується наступні задачі

- Task 22: Профіль потребує auth
- Task 23: Історія замовлень потребує auth

## Технічні деталі

### Backend: AuthService.cs
```csharp
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if user exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new BadRequestException("Email already registered");

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate tokens
        var tokens = GenerateTokens(user);

        return new AuthResponse
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials");

        var tokens = GenerateTokens(user);

        return new AuthResponse
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            }
        };
    }

    private (string AccessToken, string RefreshToken) GenerateTokens(User user)
    {
        var jwtSecret = _config["Jwt:Secret"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        // Access token (15 min)
        var accessClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var accessToken = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: accessClaims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        // Refresh token (7 days)
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        _context.SaveChanges();

        return (
            new JwtSecurityTokenHandler().WriteToken(accessToken),
            refreshToken.Token
        );
    }
}
```

### Program.cs JWT Configuration
```csharp
// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])
            )
        };
    });
```

### Frontend: authStore.ts
```typescript
import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { authService } from '@/services/authService'

interface User {
  id: string
  email: string
  firstName: string
  lastName: string
}

interface AuthStore {
  user: User | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean

  login: (email: string, password: string) => Promise<void>
  register: (data: RegisterData) => Promise<void>
  logout: () => Promise<void>
  refreshAccessToken: () => Promise<void>
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set, get) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,

      login: async (email, password) => {
        const response = await authService.login(email, password)
        set({
          user: response.user,
          accessToken: response.accessToken,
          refreshToken: response.refreshToken,
          isAuthenticated: true,
        })
      },

      register: async (data) => {
        const response = await authService.register(data)
        set({
          user: response.user,
          accessToken: response.accessToken,
          refreshToken: response.refreshToken,
          isAuthenticated: true,
        })
      },

      logout: async () => {
        const refreshToken = get().refreshToken
        if (refreshToken) {
          await authService.logout(refreshToken)
        }
        set({
          user: null,
          accessToken: null,
          refreshToken: null,
          isAuthenticated: false,
        })
      },

      refreshAccessToken: async () => {
        const refreshToken = get().refreshToken
        if (!refreshToken) throw new Error('No refresh token')

        const response = await authService.refresh(refreshToken)
        set({ accessToken: response.accessToken })
      },
    }),
    {
      name: 'auth-storage',
    }
  )
)
```

### Axios Interceptor
```typescript
import axios from 'axios'
import { useAuthStore } from '@/store/authStore'

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
})

// Add token to requests
api.interceptors.request.use((config) => {
  const { accessToken } = useAuthStore.getState()
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`
  }
  return config
})

// Auto-refresh token on 401
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      try {
        await useAuthStore.getState().refreshAccessToken()
        return api(originalRequest)
      } catch (refreshError) {
        useAuthStore.getState().logout()
        window.location.href = '/auth/login'
      }
    }

    return Promise.reject(error)
  }
)

export default api
```

## Примітки

- JWT краще за session-based auth для SPA
- Refresh token зберігається в БД для можливості revoke
- BCrypt для password hashing (secure)
- Zustand persist зберігає auth state в localStorage

## Чому Claude?

Складна security-критична задача:
- JWT implementation з refresh tokens
- Password hashing і security
- Token validation middleware
- Auto-refresh logic
- Protected routes architecture

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
