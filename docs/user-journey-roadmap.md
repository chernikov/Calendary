# User Journey Roadmap - Calendary

## 📊 Аналіз Поточної Архітектури

### ✅ Що вже є (Backend)
- **API (.NET 9.0)**: Повний RESTful API
- **Database**: MS SQL з Entity Framework Core
- **Інтеграції**: Replicate AI, MonoBank, Nova Poshta, SendGrid, SmsClub
- **Queue**: RabbitMQ для асинхронних задач
- **Auth**: JWT токени
- **PDF Generation**: Для календарів
- **Deployment**: Docker + DigitalOcean

### ✅ Що вже є (Frontend)
- **Angular 20** з SSR
- **Admin Panel**: Управління користувачами, замовленнями, промптами, моделями
- **Pages**: Home (landing), Login, Register, Profile, Cart, Checkout, Editor, Catalog
- **Master Wizard**: Процес створення моделі (model-wizard)
- **Guards**: AdminGuard, UserGuard

### ⚠️ Проблеми Поточного User Journey

1. **Складний початок**: Користувач одразу потрапляє на `/master`, який вимагає створення AI моделі
2. **Немає onboarding**: Користувач не розуміє, що робити
3. **Відсутність каталогу готових рішень**: Нема можливості швидко почати без AI
4. **Розірваний процес**: Багато різних сторінок без зрозумілого flow
5. **Технічна складність**: Користувач бачить терміни "Flux Model", "Training", "Synthesis"

---

## 🎯 Новий User Journey (Цільовий)

### Концепція: "3 Шляхи до Календаря"

```
┌─────────────────────────────────────────────────────────────┐
│                     🏠 LANDING PAGE                         │
│  - Пояснення сервісу                                       │
│  - Демо приклади календарів                                │
│  - 3 варіанти старту (CTA)                                 │
└──────────────────┬──────────────────────────────────────────┘
                   │
       ┌───────────┼───────────┐
       │           │           │
       ▼           ▼           ▼
  ┌─────────┐ ┌─────────┐ ┌──────────┐
  │ Шлях 1  │ │ Шлях 2  │ │ Шлях 3   │
  │ AI Magic│ │ Шаблони │ │ Подарунок│
  └─────────┘ └─────────┘ └──────────┘
```

#### Шлях 1: "AI Magic" (Персоналізований з AI)
**Цільова аудиторія**: Люди, які хочуть унікальний календар зі своїми фото

```
Landing → Register/Login → Onboarding → Upload Photos (10-20шт)
  → AI Training (20-30хв) → Choose Prompts/Themes
  → Generate 12 Images → Review Gallery → Editor (опційно)
  → Cart → Checkout → Payment → Order Confirmation
```

#### Шлях 2: "Швидкий Старт" (Готові шаблони)
**Цільова аудиторія**: Люди, які хочуть швидко створити календар без AI

```
Landing → Catalog → Choose Template → Customize (текст, дати)
  → Optional: Upload own photos → Editor → Cart
  → Register/Login (при checkout) → Checkout → Payment → Order
```

#### Шлях 3: "Подарунок" (Gift Card / Готовий дизайн)
**Цільова аудиторія**: Покупці подарунків

```
Landing → Gift Card Product → Add to Cart → Register/Login
  → Checkout → Payment → Order Confirmation
  → Email з подарунковим кодом
```

---

## 📋 Детальний План Реалізації

### Phase 1: Покращення Landing Page та Onboarding (Тиждень 1-2)

#### ✅ Вже зроблено:
- [x] Сучасний Landing з секціями (Hero, Features, How It Works, Testimonials, FAQ, Pricing)

#### 🔨 Треба додати:

**1.1 Landing Page Enhancement**
- [ ] Додати реальні зображення замість emoji (використати `docs/landing-page-images-requirements.md`)
- [ ] Створити 3 великі CTA кнопки для 3 шляхів:
  - "Створити з AI" → `/onboarding?path=ai`
  - "Обрати шаблон" → `/catalog`
  - "Подарувати" → `/gift-card`
- [ ] Додати секцію "Приклади Робіт" з реальними календарями
- [ ] Додати Trust Indicators (кількість задоволених клієнтів, 5★ відгуки)
- [ ] Video/GIF демонстрація процесу (опційно)

**1.2 Onboarding Flow (нова сторінка)**

Створити `/onboarding` компонент:

```typescript
// src/app/pages/onboarding/onboarding.component.ts
interface OnboardingStep {
  title: string;
  description: string;
  image: string; // приклад
  action: string; // кнопка
}

steps = [
  {
    title: "Завантажте ваші фото",
    description: "10-20 фотографій для AI моделі",
    image: "assets/onboarding/step1.png",
    action: "Далі"
  },
  {
    title: "AI навчиться на ваших фото",
    description: "20-30 хвилин обробки",
    image: "assets/onboarding/step2.png",
    action: "Зрозуміло"
  },
  {
    title: "Виберіть теми для місяців",
    description: "12 унікальних зображень",
    image: "assets/onboarding/step3.png",
    action: "Почати!"
  }
];
```

**Структура:**
- `/onboarding` - Welcome screen з вибором шляху (якщо не передано `?path=`)
- `/onboarding/ai-intro` - Пояснення AI процесу (3 слайди)
- `/onboarding/template-intro` - Пояснення роботи з шаблонами
- `/onboarding/gift-intro` - Інформація про подарункові сертифікати

**Completion Criteria:**
- [ ] Onboarding компонент створено
- [ ] Smooth transitions між кроками
- [ ] Skip onboarding (для досвідчених користувачів)
- [ ] Progress indicator
- [ ] Зберігання стану в localStorage (чи пройшов користувач onboarding)

---

### Phase 2: Покращення AI Model Creation Flow (Тиждень 2-3)

**2.1 Refactor Model Wizard** (`/master` → `/create-ai-model`)

Поточний `/master` (model-wizard) має багато кроків і незрозумілий:

```
Поточний flow:
/master → flux-model → photo-upload → photo-instructions
  → generation-status → prompt-selection → image-generation
  → prompt-results → calendar-months → calendar-dates
  → calendar-ready → payment
```

**Проблеми:**
- Надто багато екранів
- Технічні терміни ("flux-model", "synthesis")
- Немає чіткого прогресу

**Нова структура:**

```
/create-ai-model (Wizard з 5 головних кроків)
├─ Step 1: Upload Photos (10-20 photos)
│  └─ Drag & Drop, Превью, Валідація
├─ Step 2: AI Training (Processing screen)
│  └─ Progress bar, Estimated time, Can leave and return
├─ Step 3: Choose Themes (12 months)
│  └─ Категорії промптів: Nature, Travel, Family, Art, etc.
├─ Step 4: Generate Images
│  └─ Preview gallery, Regenerate окремих
└─ Step 5: Review & Customize
   └─ Grid view, можливість замінити зображення, → Editor
```

**Реалізація:**

```typescript
// src/app/pages/create-ai-model/create-ai-model.component.ts

enum WizardStep {
  UploadPhotos = 1,
  AITraining = 2,
  ChooseThemes = 3,
  GenerateImages = 4,
  ReviewGallery = 5
}

interface ModelCreationState {
  currentStep: WizardStep;
  modelId?: number;
  trainingId?: number;
  uploadedPhotos: Photo[];
  selectedThemes: PromptTheme[]; // 12 themes (по одній на місяць)
  generatedImages: GeneratedImage[];
  isComplete: boolean;
}
```

**UI Компоненти:**
- [ ] `StepIndicator` - Прогрес bar з 5 кроками
- [ ] `PhotoUploader` - Drag & Drop з превью та валідацією
- [ ] `TrainingProgress` - Real-time статус тренування
- [ ] `ThemeSelector` - Картки з категоріями тем
- [ ] `ImageGallery` - Grid view згенерованих зображень
- [ ] `ImageRegenerate` - Можливість перегенерувати окремі місяці

**Backend Changes:**
- [ ] Додати endpoint `/api/models/status/{modelId}` для polling статусу
- [ ] Webhook обробка для автоматичного переходу на наступний крок
- [ ] Зберігання проміжних результатів

**Completion Criteria:**
- [ ] Wizard з 5 чіткими кроками
- [ ] Можливість зберегти прогрес і повернутися пізніше
- [ ] Зрозумілі назви без технічних термінів
- [ ] Email notification після завершення AI тренування
- [ ] Mobile-friendly UI

---

### Phase 3: Catalog & Template System (Тиждень 3-4)

**3.1 Покращення Каталогу Шаблонів**

Поточний `/catalog` існує, але потребує доопрацювання:

**Features to add:**
- [ ] **Фільтри**:
  - За категорією (Сім'я, Подорожі, Мінімалізм, Бізнес, Природа)
  - За кольоровою схемою
  - За стилем (Modern, Classic, Fun, Elegant)
  - За ціною

- [ ] **Сортування**:
  - Популярні
  - Нові
  - За ціною (дешевші/дорожчі)

- [ ] **Preview**:
  - Hover для quick preview
  - Click для детального перегляду (modal)
  - Можливість переглянути всі 12 місяців

- [ ] **Quick Actions**:
  - "Використати цей шаблон" → Editor
  - "Додати в обране"
  - "Поділитися"

**3.2 Template Customization Flow**

```
/catalog → Select Template → /customize-template/:id → Editor → Cart → Checkout
```

Створити `/customize-template/:id`:
- [ ] Попередній перегляд шаблону
- [ ] Швидке редагування:
  - Зміна кольорів
  - Додавання дат
  - Вибір мови (UK/EN)
  - Початок тижня (Пн/Нд)
- [ ] Кнопка "Відкрити в редакторі" для детальнішого редагування

**Completion Criteria:**
- [ ] Каталог з мінімум 20 шаблонами
- [ ] Фільтри працюють
- [ ] Preview system реалізовано
- [ ] Швидка кастомізація працює
- [ ] Можливість зберегти кастомізований шаблон

---

### Phase 4: Editor Enhancement (Тиждень 4-5)

**4.1 Покращення Редактора Календаря**

Поточний `/editor` є, але потребує UX покращень:

**Основні компоненти:**
```
/editor
├─ Toolbar (top)
├─ Sidebar (left) - Months, Elements, Text, Images
├─ Canvas (center) - Month view
├─ Properties Panel (right) - Selected element properties
└─ Bottom Bar - Save, Preview, Export
```

**Features to add:**
- [ ] **Templates Library** в sidebar
- [ ] **Drag & Drop** для елементів
- [ ] **Undo/Redo** functionality
- [ ] **Autosave** кожні 30 секунд
- [ ] **Version History** (останні 5 збережень)
- [ ] **Hotkeys**: Ctrl+Z, Ctrl+S, Del, Ctrl+C/V
- [ ] **Grid & Guides** для вирівнювання
- [ ] **Zoom** (50%, 75%, 100%, 150%, 200%)
- [ ] **Multi-month view** для перегляду всього календаря
- [ ] **Export Options**:
  - PDF для друку
  - PNG для соцмереж
  - Calendar file (.ics) для Google Calendar

**4.2 Element Types**
- [ ] Text (з різними шрифтами)
- [ ] Images (upload власних)
- [ ] Shapes (rectangles, circles, lines)
- [ ] Icons (з бібліотеки)
- [ ] Backgrounds (patterns, gradients, solid colors)
- [ ] Stickers/Decorations

**4.3 Mobile Editor**
- [ ] Спрощена версія для мобільних
- [ ] Touch gestures (pinch to zoom, drag to move)

**Completion Criteria:**
- [ ] Редактор працює smooth на desktop
- [ ] Всі базові функції реалізовані
- [ ] Autosave працює
- [ ] Export в PDF працює коректно
- [ ] Mobile версія функціональна

---

### Phase 5: Cart, Checkout & Payment (Тиждень 5-6)

**5.1 Shopping Cart Enhancement**

Поточний `/cart` є, але needs improvement:

**Features:**
- [ ] **Item Preview** в кошику
- [ ] **Quantity selector** (якщо хтось хоче кілька копій)
- [ ] **Promo Codes** система
- [ ] **Saved for Later** (переміщення з кошика)
- [ ] **Estimate Delivery** (розрахунок Нова Пошта)
- [ ] **Persistent Cart** (зберігання між сесіями)

**5.2 Checkout Process**

```
/checkout
├─ Step 1: Shipping Info (Nova Poshta)
│  ├─ Вибір міста
│  ├─ Вибір відділення
│  └─ Контактні дані
├─ Step 2: Payment Method
│  ├─ MonoBank (online)
│  └─ Cash on Delivery (опційно)
└─ Step 3: Order Review
   ├─ Перевірка деталей
   ├─ Agreements (Privacy, Terms)
   └─ Place Order
```

**Features to add:**
- [ ] **Address Autocomplete** (Nova Poshta API)
- [ ] **Cost Calculator** (Delivery + Product)
- [ ] **Order Summary** sidebar (завжди видимий)
- [ ] **Guest Checkout** (опційно, без реєстрації)
- [ ] **Save Address** для наступних замовлень

**5.3 Payment Integration**

MonoBank вже інтегровано, але треба:
- [ ] **Payment Status Page** (`/payment/status/:orderId`)
- [ ] **Webhook Handler** для підтвердження оплати
- [ ] **Email Confirmation** після успішної оплати
- [ ] **Failed Payment** handling і retry

**Completion Criteria:**
- [ ] Cart працює smooth
- [ ] Checkout процес зрозумілий і швидкий (max 3 кроки)
- [ ] Payment інтеграція стабільна
- [ ] Email notifications працюють
- [ ] Статуси замовлення оновлюються автоматично

---

### Phase 6: User Profile & Dashboard (Тиждень 6-7)

**6.1 User Profile Enhancement**

Поточний `/profile` exists, потрібно додати:

**Sections:**
- [ ] **Dashboard** (overview)
  - Активні замовлення
  - Збережені дизайни
  - AI моделі статус
  - Quick actions

- [ ] **My Orders** (`/profile/orders`)
  - Список замовлень
  - Статус tracking
  - Download PDF/Invoice
  - Reorder button

- [ ] **My Designs** (`/profile/designs`)
  - Збережені календарі
  - Draft calendars
  - Можливість дублювати/редагувати

- [ ] **My AI Models** (`/profile/models`)
  - Список AI моделей
  - Статус тренування
  - Можливість створити нову модель
  - Можливість видалити старі моделі

- [ ] **Settings** (`/profile/settings`)
  - Personal info
  - Change password
  - Email preferences
  - Delete account

**6.2 Credits System (опційно, для майбутнього)**

Якщо хочете монетизацію через кредити:
- [ ] Credits balance
- [ ] Buy credits page (`/credits-shop`)
- [ ] Transaction history
- [ ] Credit usage по операціях (AI training, generation)

**Completion Criteria:**
- [ ] Профіль з усіма секціями
- [ ] Order tracking працює
- [ ] My Designs показує всі збережені роботи
- [ ] Settings працюють коректно

---

### Phase 7: Authentication & Security (Тиждень 7)

**7.1 Auth Flow Enhancement**

Поточна auth є, але треба:

**Login:**
- [ ] Social Login (Google, Facebook) опційно
- [ ] "Remember Me" checkbox
- [ ] Password strength indicator на register
- [ ] CAPTCHA (Google reCAPTCHA) на register

**Password Recovery:**
- [ ] `/forgot-password` покращення
- [ ] Email з reset link
- [ ] `/reset-password/:token` сторінка
- [ ] Підтвердження після зміни пароля

**Email Verification:**
- [ ] `/verify/:token` покращення
- [ ] Resend verification email
- [ ] Verification reminder на login (якщо не verified)

**Completion Criteria:**
- [ ] Smooth login/register flow
- [ ] Forgot password працює
- [ ] Email verification працює
- [ ] Security measures на місці

---

### Phase 8: Admin Panel Enhancement (Тиждень 8)

**8.1 Orders Management**

Поточна `/admin/orders` є, треба:
- [ ] Bulk actions (mark as shipped, print labels)
- [ ] Filters (status, date range, amount)
- [ ] Export to CSV/Excel
- [ ] Nova Poshta TTN creation (якщо не реалізовано)
- [ ] Order analytics dashboard

**8.2 Users Management**

- [ ] User segmentation (Active, Inactive, VIP)
- [ ] Bulk email campaigns
- [ ] User activity logs

**8.3 Content Management**

- [ ] Templates management (upload, edit, delete)
- [ ] Prompts library management
- [ ] Categories management

**8.4 Marketing Dashboard** (новий розділ)

- [ ] Sales analytics
- [ ] Conversion funnel
- [ ] Traffic sources
- [ ] Popular templates
- [ ] Revenue reports

**Completion Criteria:**
- [ ] Admin може ефективно керувати замовленнями
- [ ] Аналітика допомагає приймати рішення
- [ ] Всі CRUD операції працюють

---

## 🎨 Design System & UI/UX Improvements

### Global Improvements (Паралельно з phases)

**1. Створити Design System**
- [ ] Component Library (buttons, cards, inputs, modals)
- [ ] Typography guidelines
- [ ] Color palette
- [ ] Spacing system (8px grid)
- [ ] Icons library
- [ ] Animation library

**2. Accessibility**
- [ ] WCAG 2.1 Level AA compliance
- [ ] Keyboard navigation
- [ ] Screen reader support
- [ ] Alt texts для зображень
- [ ] Focus indicators

**3. Performance**
- [ ] Lazy loading для зображень
- [ ] Code splitting для routes
- [ ] PWA support (опційно)
- [ ] Caching strategy
- [ ] Optimize bundle size

**4. Mobile Experience**
- [ ] Responsive на всіх сторінках
- [ ] Touch-friendly buttons (min 44x44px)
- [ ] Mobile navigation (hamburger menu)
- [ ] Mobile-optimized forms

---

## 📊 Success Metrics (KPIs)

### User Journey Metrics

**Acquisition:**
- Landing page → Register conversion: **target >5%**
- Social media → Landing conversion: **target >2%**

**Activation:**
- Register → Create first model: **target >60%**
- Register → Create from template: **target >80%**

**Retention:**
- 7-day return rate: **target >30%**
- 30-day return rate: **target >15%**

**Revenue:**
- Average order value: **target >500 грн**
- Conversion rate (visit → purchase): **target >2%**

**Referral:**
- Share rate: **target >10%**
- Referral conversion: **target >5%**

---

## ⏱️ Timeline & Priorities

### Sprint 1 (2 weeks): Landing + Onboarding
- **Priority**: 🔥 High
- Landing page з реальними зображеннями
- Onboarding flow для 3 шляхів
- A/B testing setup

### Sprint 2 (2 weeks): AI Model Creation
- **Priority**: 🔥 High
- Refactor model wizard
- Improved step-by-step flow
- Real-time progress tracking

### Sprint 3 (2 weeks): Catalog & Templates
- **Priority**: 🟡 Medium-High
- 20+ шаблонів
- Filters & Search
- Quick customization

### Sprint 4 (2 weeks): Editor
- **Priority**: 🟡 Medium
- Enhanced editor features
- Autosave & Version history
- Export options

### Sprint 5 (2 weeks): Checkout & Payment
- **Priority**: 🔥 High
- Streamlined checkout
- Payment stability
- Email confirmations

### Sprint 6 (1 week): Profile & Dashboard
- **Priority**: 🟢 Medium-Low
- User dashboard
- Order tracking
- Saved designs

### Sprint 7 (1 week): Auth & Security
- **Priority**: 🟡 Medium
- Enhanced security
- Social login (optional)
- Email verification improvements

### Sprint 8 (1 week): Admin Panel
- **Priority**: 🟢 Low
- Order management improvements
- Analytics dashboard
- Marketing tools

---

## 🚀 Quick Wins (Можна зробити одразу)

1. **Landing CTA кнопки** - додати 3 чіткі варіанти (1 день)
2. **Progress Indicators** - додати прогрес в wizard (2 дні)
3. **Autosave в Editor** - критична фіча (3 дні)
4. **Email після AI Training** - користувачі не чекають (2 дні)
5. **Order Tracking** - показати статус доставки (3 дні)
6. **Mobile Responsive** - критично для конверсії (5 днів)

---

## 📝 Documentation Needs

- [ ] **User Guide** - Як створити календар (для клієнтів)
- [ ] **API Documentation** - Swagger/OpenAPI
- [ ] **Developer Guide** - Для команди розробки
- [ ] **Admin Manual** - Як працювати з панеллю
- [ ] **Marketing Materials** - Screenshots, video demos

---

**Створено**: 2025-11-16
**Автор**: AI Assistant (Claude)
**Версія**: 1.0
**Статус**: Draft - Ready for Review
