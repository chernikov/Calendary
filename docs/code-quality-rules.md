# Правила якості коду Calendary

## Загальні принципи

### SOLID принципи
- **S**ingle Responsibility - кожен клас/функція має одну відповідальність
- **O**pen/Closed - відкритий для розширення, закритий для модифікації
- **L**iskov Substitution - підкласи мають бути взаємозамінними
- **I**nterface Segregation - багато специфічних інтерфейсів краще одного загального
- **D**ependency Inversion - залежність від абстракцій, а не конкретних реалізацій

### DRY (Don't Repeat Yourself)
- Уникати дублювання коду
- Виносити повторювану логіку в окремі функції/модулі
- Використовувати утиліти та helper функції

### KISS (Keep It Simple, Stupid)
- Простота важливіша за складність
- Чітка та зрозуміла логіка
- Уникати надмірної абстракції без потреби

---

## TypeScript / JavaScript

### 1. Загальні правила

#### Використовувати TypeScript strict mode
```json
{
  "compilerOptions": {
    "strict": true,
    "noImplicitAny": true,
    "strictNullChecks": true,
    "strictFunctionTypes": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true
  }
}
```

#### Типізація
```typescript
// ❌ Погано - неявні типи
function calculate(a, b) {
  return a + b;
}

// ✅ Добре - явні типи
function calculate(a: number, b: number): number {
  return a + b;
}

// ✅ Використовувати інтерфейси та типи
interface Order {
  id: string;
  userId: string;
  items: OrderItem[];
  totalPrice: number;
  status: OrderStatus;
}

type OrderStatus = 'pending' | 'paid' | 'printing' | 'shipped' | 'delivered' | 'cancelled';
```

#### Naming Conventions
```typescript
// Змінні та функції - camelCase
const userName = 'John';
function getUserById(id: string) {}

// Класи та інтерфейси - PascalCase
class OrderService {}
interface PaymentGateway {}

// Константи - UPPER_SNAKE_CASE
const MAX_RETRY_ATTEMPTS = 3;
const API_BASE_URL = 'https://api.example.com';

// Приватні поля класу - з підкресленням
class User {
  private _password: string;
}

// Boolean змінні - з префіксом is/has/should
const isActive = true;
const hasPermission = false;
const shouldRetry = true;
```

### 2. Функції

#### Чисті функції (Pure Functions)
```typescript
// ✅ Добре - чиста функція
function calculateDiscount(price: number, discountPercent: number): number {
  return price * (1 - discountPercent / 100);
}

// ❌ Погано - побічні ефекти
let totalPrice = 0;
function addToTotal(price: number) {
  totalPrice += price; // mutation зовнішньої змінної
}
```

#### Одна відповідальність
```typescript
// ❌ Погано - багато відповідальностей
function processOrder(order: Order) {
  // валідація
  if (!order.items.length) throw new Error('Empty order');

  // розрахунок
  const total = order.items.reduce((sum, item) => sum + item.price, 0);

  // збереження в БД
  database.save(order);

  // відправка email
  sendEmail(order.userId, 'Order confirmed');

  // логування
  console.log('Order processed');
}

// ✅ Добре - розділені відповідальності
function validateOrder(order: Order): void {
  if (!order.items.length) throw new Error('Empty order');
}

function calculateOrderTotal(items: OrderItem[]): number {
  return items.reduce((sum, item) => sum + item.price, 0);
}

async function saveOrder(order: Order): Promise<void> {
  await database.save(order);
}

async function notifyOrderConfirmation(userId: string): Promise<void> {
  await sendEmail(userId, 'Order confirmed');
}
```

#### Параметри функцій
```typescript
// ❌ Погано - багато параметрів
function createUser(
  name: string,
  email: string,
  phone: string,
  address: string,
  city: string,
  country: string
) {}

// ✅ Добре - об'єкт параметрів
interface CreateUserParams {
  name: string;
  email: string;
  phone: string;
  address: {
    street: string;
    city: string;
    country: string;
  };
}

function createUser(params: CreateUserParams) {}
```

### 3. Обробка помилок

```typescript
// ✅ Використовувати кастомні Error класи
class ValidationError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'ValidationError';
  }
}

class PaymentError extends Error {
  constructor(
    message: string,
    public readonly code: string,
    public readonly details?: unknown
  ) {
    super(message);
    this.name = 'PaymentError';
  }
}

// ✅ Обробка помилок у async функціях
async function processPayment(orderId: string): Promise<void> {
  try {
    const order = await getOrder(orderId);

    if (!order) {
      throw new ValidationError('Order not found');
    }

    const result = await paymentGateway.charge(order.totalPrice);

    if (!result.success) {
      throw new PaymentError('Payment failed', result.code, result.details);
    }

    await updateOrderStatus(orderId, 'paid');
  } catch (error) {
    if (error instanceof ValidationError) {
      logger.warn('Validation error:', error.message);
    } else if (error instanceof PaymentError) {
      logger.error('Payment error:', error);
      // можливо retry логіка
    } else {
      logger.error('Unexpected error:', error);
    }
    throw error; // re-throw для обробки на вищому рівні
  }
}
```

### 4. Async/Await

```typescript
// ❌ Погано - callback hell
getData((data) => {
  processData(data, (result) => {
    saveResult(result, () => {
      console.log('Done');
    });
  });
});

// ✅ Добре - async/await
async function handleData() {
  const data = await getData();
  const result = await processData(data);
  await saveResult(result);
  console.log('Done');
}

// ✅ Паралельне виконання незалежних операцій
async function loadDashboard() {
  const [orders, users, analytics] = await Promise.all([
    getOrders(),
    getUsers(),
    getAnalytics()
  ]);

  return { orders, users, analytics };
}
```

---

## React / Next.js

### 1. Компоненти

#### Функціональні компоненти
```typescript
// ✅ Використовувати функціональні компоненти з hooks
import { FC, useState, useEffect } from 'react';

interface CalendarEditorProps {
  templateId: string;
  onSave: (design: CalendarDesign) => void;
}

export const CalendarEditor: FC<CalendarEditorProps> = ({ templateId, onSave }) => {
  const [design, setDesign] = useState<CalendarDesign | null>(null);

  useEffect(() => {
    loadTemplate(templateId).then(setDesign);
  }, [templateId]);

  if (!design) return <Spinner />;

  return (
    <div className="calendar-editor">
      {/* ... */}
    </div>
  );
};
```

#### Розділення логіки та UI
```typescript
// ✅ Custom hooks для логіки
function useOrderManagement(orderId: string) {
  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadOrder();
  }, [orderId]);

  const loadOrder = async () => {
    setLoading(true);
    try {
      const data = await api.getOrder(orderId);
      setOrder(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const updateStatus = async (status: OrderStatus) => {
    await api.updateOrderStatus(orderId, status);
    await loadOrder();
  };

  return { order, loading, error, updateStatus };
}

// Використання в компоненті
export const OrderDetails: FC<{ orderId: string }> = ({ orderId }) => {
  const { order, loading, error, updateStatus } = useOrderManagement(orderId);

  if (loading) return <Spinner />;
  if (error) return <Error message={error} />;
  if (!order) return <NotFound />;

  return <OrderView order={order} onUpdateStatus={updateStatus} />;
};
```

### 2. State Management

```typescript
// ✅ Zustand store приклад
import { create } from 'zustand';

interface CartState {
  items: CartItem[];
  addItem: (item: CartItem) => void;
  removeItem: (itemId: string) => void;
  clearCart: () => void;
  total: () => number;
}

export const useCartStore = create<CartState>((set, get) => ({
  items: [],

  addItem: (item) => set((state) => ({
    items: [...state.items, item]
  })),

  removeItem: (itemId) => set((state) => ({
    items: state.items.filter(item => item.id !== itemId)
  })),

  clearCart: () => set({ items: [] }),

  total: () => get().items.reduce((sum, item) => sum + item.price, 0)
}));
```

---

## Backend / API

### 1. Структура проекту

```
src/
├── modules/
│   ├── orders/
│   │   ├── orders.controller.ts
│   │   ├── orders.service.ts
│   │   ├── orders.repository.ts
│   │   ├── dto/
│   │   ├── entities/
│   │   └── tests/
│   ├── payments/
│   └── users/
├── common/
│   ├── decorators/
│   ├── guards/
│   ├── interceptors/
│   └── utils/
└── config/
```

### 2. API Design

#### RESTful принципи
```typescript
// ✅ Правильні endpoint'и
GET    /api/orders              - список замовлень
POST   /api/orders              - створити замовлення
GET    /api/orders/:id          - отримати замовлення
PATCH  /api/orders/:id          - оновити замовлення
DELETE /api/orders/:id          - видалити замовлення

// Вкладені ресурси
GET    /api/orders/:id/items    - items конкретного замовлення
POST   /api/orders/:id/items    - додати item

// Дії над ресурсами
POST   /api/orders/:id/cancel   - скасувати замовлення
POST   /api/orders/:id/confirm  - підтвердити замовлення
```

#### DTO (Data Transfer Objects)
```typescript
// create-order.dto.ts
import { IsArray, IsString, ValidateNested } from 'class-validator';
import { Type } from 'class-transformer';

export class CreateOrderDto {
  @IsString()
  userId: string;

  @IsArray()
  @ValidateNested({ each: true })
  @Type(() => OrderItemDto)
  items: OrderItemDto[];

  @IsString()
  deliveryAddress: string;
}

export class OrderItemDto {
  @IsString()
  calendarId: string;

  @IsNumber()
  @Min(1)
  quantity: number;
}
```

### 3. Service Layer

```typescript
// orders.service.ts
import { Injectable, NotFoundException } from '@nestjs/common';

@Injectable()
export class OrdersService {
  constructor(
    private readonly ordersRepository: OrdersRepository,
    private readonly paymentsService: PaymentsService,
    private readonly notificationsService: NotificationsService,
  ) {}

  async createOrder(dto: CreateOrderDto): Promise<Order> {
    // Валідація
    await this.validateOrder(dto);

    // Створення
    const order = await this.ordersRepository.create({
      ...dto,
      status: 'pending',
      createdAt: new Date(),
    });

    // Побічні ефекти
    await this.notificationsService.sendOrderConfirmation(order.id);

    return order;
  }

  async getOrder(id: string): Promise<Order> {
    const order = await this.ordersRepository.findById(id);

    if (!order) {
      throw new NotFoundException(`Order ${id} not found`);
    }

    return order;
  }

  private async validateOrder(dto: CreateOrderDto): Promise<void> {
    if (!dto.items.length) {
      throw new BadRequestException('Order must have at least one item');
    }

    // Додаткова валідація...
  }
}
```

---

## Database

### 1. Prisma Schema

```prisma
model User {
  id        String   @id @default(cuid())
  email     String   @unique
  name      String
  role      Role     @default(CUSTOMER)
  orders    Order[]
  createdAt DateTime @default(now())
  updatedAt DateTime @updatedAt

  @@index([email])
  @@map("users")
}

model Order {
  id              String      @id @default(cuid())
  userId          String
  user            User        @relation(fields: [userId], references: [id])
  items           OrderItem[]
  status          OrderStatus @default(PENDING)
  totalPrice      Decimal     @db.Decimal(10, 2)
  deliveryAddress String?
  trackingNumber  String?
  createdAt       DateTime    @default(now())
  updatedAt       DateTime    @updatedAt

  @@index([userId])
  @@index([status])
  @@index([createdAt])
  @@map("orders")
}

enum OrderStatus {
  PENDING
  PAID
  PRINTING
  SHIPPED
  DELIVERED
  CANCELLED
}
```

### 2. Queries

```typescript
// ✅ Використовувати select для оптимізації
const user = await prisma.user.findUnique({
  where: { id: userId },
  select: {
    id: true,
    name: true,
    email: true,
    // не вибирати passwordHash
  }
});

// ✅ Використовувати include обережно
const order = await prisma.order.findUnique({
  where: { id: orderId },
  include: {
    items: true,
    user: {
      select: {
        id: true,
        name: true,
        email: true,
      }
    }
  }
});

// ✅ Pagination
const orders = await prisma.order.findMany({
  take: 20,
  skip: page * 20,
  orderBy: { createdAt: 'desc' },
  where: {
    status: 'pending'
  }
});
```

---

## Testing

### 1. Unit Tests

```typescript
// orders.service.spec.ts
describe('OrdersService', () => {
  let service: OrdersService;
  let repository: jest.Mocked<OrdersRepository>;

  beforeEach(() => {
    repository = {
      create: jest.fn(),
      findById: jest.fn(),
      update: jest.fn(),
    } as any;

    service = new OrdersService(repository);
  });

  describe('createOrder', () => {
    it('should create order with valid data', async () => {
      const dto: CreateOrderDto = {
        userId: 'user-1',
        items: [{ calendarId: 'cal-1', quantity: 2 }],
        deliveryAddress: 'Test address'
      };

      const expectedOrder = { id: 'order-1', ...dto, status: 'pending' };
      repository.create.mockResolvedValue(expectedOrder);

      const result = await service.createOrder(dto);

      expect(result).toEqual(expectedOrder);
      expect(repository.create).toHaveBeenCalledWith(
        expect.objectContaining(dto)
      );
    });

    it('should throw error for empty items', async () => {
      const dto: CreateOrderDto = {
        userId: 'user-1',
        items: [],
        deliveryAddress: 'Test address'
      };

      await expect(service.createOrder(dto)).rejects.toThrow(
        'Order must have at least one item'
      );
    });
  });
});
```

### 2. Integration Tests

```typescript
describe('Orders API (e2e)', () => {
  let app: INestApplication;

  beforeAll(async () => {
    const moduleFixture = await Test.createTestingModule({
      imports: [AppModule],
    }).compile();

    app = moduleFixture.createNestApplication();
    await app.init();
  });

  afterAll(async () => {
    await app.close();
  });

  it('/api/orders (POST)', async () => {
    const dto = {
      userId: 'user-1',
      items: [{ calendarId: 'cal-1', quantity: 1 }],
      deliveryAddress: 'Test'
    };

    return request(app.getHttpServer())
      .post('/api/orders')
      .send(dto)
      .expect(201)
      .expect((res) => {
        expect(res.body).toHaveProperty('id');
        expect(res.body.status).toBe('pending');
      });
  });
});
```

---

## Security

### 1. Validation

```typescript
// Завжди валідувати вхідні дані
app.useGlobalPipes(new ValidationPipe({
  whitelist: true,       // видаляти невідомі поля
  forbidNonWhitelisted: true, // помилка при невідомих полях
  transform: true,       // автоматична трансформація типів
}));
```

### 2. Authentication & Authorization

```typescript
// auth.guard.ts
@Injectable()
export class JwtAuthGuard extends AuthGuard('jwt') {
  canActivate(context: ExecutionContext) {
    return super.canActivate(context);
  }
}

// roles.guard.ts
@Injectable()
export class RolesGuard implements CanActivate {
  canActivate(context: ExecutionContext): boolean {
    const requiredRoles = this.reflector.get<Role[]>('roles', context.getHandler());
    if (!requiredRoles) return true;

    const { user } = context.switchToHttp().getRequest();
    return requiredRoles.some(role => user.role === role);
  }
}

// Використання
@Controller('admin/orders')
@UseGuards(JwtAuthGuard, RolesGuard)
export class AdminOrdersController {
  @Get()
  @Roles(Role.ADMIN, Role.MANAGER)
  async getOrders() {
    // ...
  }
}
```

### 3. SQL Injection Prevention

```typescript
// ❌ Погано - SQL injection
const query = `SELECT * FROM users WHERE email = '${email}'`;

// ✅ Добре - параметризовані запити (Prisma робить це автоматично)
const user = await prisma.user.findUnique({
  where: { email }
});
```

---

## Performance

### 1. Caching

```typescript
// cache.service.ts
@Injectable()
export class CacheService {
  constructor(private readonly redis: Redis) {}

  async get<T>(key: string): Promise<T | null> {
    const data = await this.redis.get(key);
    return data ? JSON.parse(data) : null;
  }

  async set(key: string, value: any, ttl: number = 3600): Promise<void> {
    await this.redis.set(key, JSON.stringify(value), 'EX', ttl);
  }

  async del(key: string): Promise<void> {
    await this.redis.del(key);
  }
}

// Використання
async getOrder(id: string): Promise<Order> {
  const cacheKey = `order:${id}`;

  const cached = await this.cache.get<Order>(cacheKey);
  if (cached) return cached;

  const order = await this.repository.findById(id);
  await this.cache.set(cacheKey, order, 600); // 10 хв

  return order;
}
```

### 2. N+1 Problem

```typescript
// ❌ Погано - N+1 queries
const orders = await prisma.order.findMany();
for (const order of orders) {
  order.user = await prisma.user.findUnique({ where: { id: order.userId } });
}

// ✅ Добре - один запит з include
const orders = await prisma.order.findMany({
  include: {
    user: true
  }
});
```

---

## Git

### 1. Commit Messages

```
feat: додати можливість корпоративних замовлень
fix: виправити розрахунок знижки для bulk orders
docs: оновити API документацію для payments
refactor: переписати OrderService на clean architecture
test: додати тести для PaymentService
chore: оновити залежності
```

### 2. Branch Naming

```
feature/calendar-editor
feature/monobank-integration
bugfix/payment-validation
hotfix/critical-security-issue
refactor/order-service
```

### 3. Pull Requests

- Опис змін
- Скріншоти (для UI)
- Checklist тестування
- Breaking changes
- Міграції БД (якщо є)

---

## Code Review Checklist

- [ ] Код відповідає TypeScript strict mode
- [ ] Всі функції мають типи параметрів та return type
- [ ] Немає any типів (або вони обгрунтовані)
- [ ] Naming conventions дотримані
- [ ] Немає дублювання коду
- [ ] Функції мають одну відповідальність
- [ ] Обробка помилок присутня
- [ ] Валідація вхідних даних
- [ ] Тести написані і проходять
- [ ] Немає console.log (використовувати logger)
- [ ] Безпека (auth, validation, SQL injection)
- [ ] Performance (N+1, caching)
- [ ] Документація оновлена

---

## Інструменти

### ESLint config
```json
{
  "extends": [
    "next/core-web-vitals",
    "plugin:@typescript-eslint/recommended",
    "plugin:prettier/recommended"
  ],
  "rules": {
    "@typescript-eslint/no-explicit-any": "error",
    "@typescript-eslint/explicit-function-return-type": "warn",
    "no-console": ["warn", { "allow": ["warn", "error"] }],
    "prefer-const": "error"
  }
}
```

### Prettier config
```json
{
  "semi": true,
  "trailingComma": "all",
  "singleQuote": true,
  "printWidth": 100,
  "tabWidth": 2
}
```

### Husky (pre-commit hooks)
```bash
#!/bin/sh
npm run lint
npm run type-check
npm run test
```
