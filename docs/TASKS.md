# Calendary - Development Tasks (User Journey Focus)

**Версія**: 1.0
**Дата створення**: 2025-11-17
**Базується на**: user-journey-roadmap.md

---

## 🎯 Пріоритетна Структура Розробки

### 📦 PHASE 1: Landing Page & Onboarding (Week 1-2) 🔥 HIGH PRIORITY

#### ✅ DONE
- [x] Task 1.1: Create modern landing page with emoji placeholders (PR #265)
- [x] Task 1.2: Create landing page images requirements documentation
- [x] Task 1.3: User journey analysis and roadmap

#### 🚀 READY TO START

**Task 1.4: Add Real Images to Landing Page**
- **Priority**: 🔥 CRITICAL
- **Estimate**: 1 day
- **Description**: Replace emoji placeholders with real images
- **Files to modify**:
  - `src/Calendary.Ng/src/app/pages/home/home.component.html`
  - Add images to `src/Calendary.Ng/src/assets/landing/`
- **Acceptance Criteria**:
  - [ ] 12 professional images added per `docs/landing-page-images-requirements.md`
  - [ ] Hero mockup image (calendar preview)
  - [ ] Step illustrations (4 images)
  - [ ] Feature icons replaced with styled images
  - [ ] All images optimized (WebP format, <200KB each)
  - [ ] Lazy loading implemented
  - [ ] Alt texts added for accessibility
- **Testing**:
  - [ ] Visual review on desktop (1920x1080, 1366x768)
  - [ ] Visual review on mobile (375x667, 414x896)
  - [ ] Lighthouse performance score >90

---

**Task 1.5: Create 3 CTA Buttons for User Paths**
- **Priority**: 🔥 CRITICAL
- **Estimate**: 4 hours
- **Description**: Add 3 distinct Call-to-Action buttons for 3 user paths
- **Files to modify**:
  - `src/Calendary.Ng/src/app/pages/home/home.component.html` (Hero section)
  - `src/Calendary.Ng/src/app/pages/home/home.component.ts` (navigation logic)
- **Buttons**:
  1. "Створити з AI" → `/onboarding?path=ai` (Primary button, glowing effect)
  2. "Обрати шаблон" → `/catalog` (Secondary button)
  3. "Подарувати" → `/gift-card` (Tertiary button)
- **Acceptance Criteria**:
  - [ ] 3 buttons with distinct styles
  - [ ] Hover animations
  - [ ] Click tracking (analytics ready)
  - [ ] Mobile responsive (stacked on small screens)
- **Testing**:
  - [ ] Click each button, verify navigation
  - [ ] Test on mobile

---

**Task 1.6: Create Onboarding Component**
- **Priority**: 🔥 CRITICAL
- **Estimate**: 2 days
- **Description**: Create onboarding flow component with 3 path intros
- **Files to create**:
  - `src/Calendary.Ng/src/app/pages/onboarding/onboarding.component.ts`
  - `src/Calendary.Ng/src/app/pages/onboarding/onboarding.component.html`
  - `src/Calendary.Ng/src/app/pages/onboarding/onboarding.component.scss`
- **Files to modify**:
  - `src/Calendary.Ng/src/app/app.routes.ts` (add `/onboarding` route)
- **Features**:
  - Welcome screen (if no `?path=` query param)
  - AI path intro (3 slides: Upload Photos → AI Training → Choose Themes)
  - Template path intro (2 slides: Browse Catalog → Customize)
  - Gift path intro (1 slide: Choose Amount → Gift)
  - Progress indicator (dots)
  - Skip button for experienced users
  - LocalStorage: track if user completed onboarding
- **Acceptance Criteria**:
  - [ ] Component generates via `ng generate component pages/onboarding`
  - [ ] Smooth slide transitions (fade or slide animation)
  - [ ] Progress dots indicator
  - [ ] "Skip" button saves to localStorage
  - [ ] "Get Started" button navigates to appropriate next step
  - [ ] Mobile responsive
- **Testing**:
  - [ ] Navigate through all 3 path intros
  - [ ] Skip button works
  - [ ] LocalStorage persists onboarding completion
  - [ ] Back button navigates to previous slide

---

### 📦 PHASE 2: AI Model Creation Wizard (Week 2-3) 🔥 HIGH PRIORITY

**Task 2.1: Refactor Model Wizard Route**
- **Priority**: 🔥 HIGH
- **Estimate**: 2 hours
- **Description**: Rename `/master` to `/create-ai-model` and update all references
- **Files to modify**:
  - `src/Calendary.Ng/src/app/app.routes.ts`
  - `src/Calendary.Ng/src/app/pages/model-wizard/model-wizard.component.ts`
  - Update all navigation calls (`this.router.navigate(['/master'])` → `'/create-ai-model'`)
- **Acceptance Criteria**:
  - [ ] Route `/master` redirects to `/create-ai-model`
  - [ ] All internal links updated
  - [ ] No broken links in app
- **Testing**:
  - [ ] Navigate to old `/master` URL → should redirect
  - [ ] All navigation from other pages works

---

**Task 2.2: Implement 5-Step Wizard UI**
- **Priority**: 🔥 HIGH
- **Estimate**: 3 days
- **Description**: Refactor model wizard into clear 5-step process
- **Files to modify**:
  - `src/Calendary.Ng/src/app/pages/model-wizard/model-wizard.component.ts`
  - `src/Calendary.Ng/src/app/pages/model-wizard/model-wizard.component.html`
  - `src/Calendary.Ng/src/app/pages/model-wizard/model-wizard.component.scss`
- **New Structure**:
  ```typescript
  enum WizardStep {
    UploadPhotos = 1,      // Upload 10-20 photos
    AITraining = 2,        // Processing screen (20-30 min)
    ChooseThemes = 3,      // Select 12 themes (one per month)
    GenerateImages = 4,    // Generate 12 AI images
    ReviewGallery = 5      // Review & customize
  }
  ```
- **Components to create**:
  - `StepIndicator` component (progress bar: 1/5, 2/5, etc.)
  - `PhotoUploader` component (drag & drop)
  - `TrainingProgress` component (progress bar, ETA)
  - `ThemeSelector` component (12 month cards with theme dropdowns)
  - `ImageGallery` component (grid view)
  - `ImageRegenerate` button for each image
- **Acceptance Criteria**:
  - [ ] Clear step indicator at top (1/5 → 5/5)
  - [ ] User-friendly language (no "Flux Model", "Training" terms)
  - [ ] Each step has title, description, and primary action button
  - [ ] Save & Return functionality (store progress in backend)
  - [ ] Mobile responsive
- **Testing**:
  - [ ] Complete full wizard flow
  - [ ] Test Save & Return (close browser, reopen)
  - [ ] Test each step individually

---

**Task 2.3: Email Notification After AI Training**
- **Priority**: 🔥 HIGH
- **Estimate**: 1 day
- **Description**: Send email when AI model training completes
- **Files to modify**:
  - `src/Calendary.Api/Controllers/WebHookController.cs` (Replicate webhook handler)
  - `src/Calendary.Core/Services/EmailService.cs`
  - `src/Calendary.Core/Templates/` (add new email template)
- **Email Template**:
  - Subject: "Ваша AI модель готова! 🎉"
  - Body: "Модель успішно навчена. Час створити календар! [Продовжити]"
  - Link: `https://calendary.com.ua/create-ai-model?modelId={id}&step=3`
- **Backend Changes**:
  - [ ] Detect training completion in webhook
  - [ ] Queue email via RabbitMQ
  - [ ] Handle email sending in Consumer
- **Acceptance Criteria**:
  - [ ] Email sent within 1 minute of training completion
  - [ ] Email contains direct link to continue wizard
  - [ ] Email is mobile-friendly (responsive HTML)
  - [ ] Unsubscribe link included
- **Testing**:
  - [ ] Trigger training completion webhook manually
  - [ ] Verify email received
  - [ ] Click link in email → navigate to correct step

---

**Task 2.4: Progress Saving & Resume**
- **Priority**: 🟡 MEDIUM
- **Estimate**: 2 days
- **Description**: Allow users to save progress and return later
- **Files to modify**:
  - `src/Calendary.Repos/Entities/FluxModel.cs` (add `CurrentStep` field)
  - `src/Calendary.Api/Controllers/ModelController.cs` (add save/resume endpoints)
  - `src/Calendary.Ng/src/app/pages/model-wizard/model-wizard.component.ts`
- **Backend API Endpoints**:
  - `POST /api/models/{id}/save-progress` - Save current step
  - `GET /api/models/{id}/resume` - Load saved progress
- **Frontend Changes**:
  - [ ] "Save & Exit" button on each step
  - [ ] On component init, check if resuming
  - [ ] LocalStorage backup (in case backend fails)
- **Acceptance Criteria**:
  - [ ] User can save at any step
  - [ ] User can resume from saved step
  - [ ] Progress persists across browser sessions
  - [ ] If AI training in progress, show status
- **Testing**:
  - [ ] Save at step 1, close browser, reopen → resume at step 1
  - [ ] Save during AI training → email notification → click link → resume at step 3

---

### 📦 PHASE 3: Catalog & Templates (Week 3-4) 🟡 MEDIUM-HIGH PRIORITY

**Task 3.1: Create Template Database Structure**
- **Priority**: 🟡 MEDIUM
- **Estimate**: 1 day
- **Description**: Create database entities for templates
- **Files to create**:
  - `src/Calendary.Repos/Entities/CalendarTemplate.cs`
  - `src/Calendary.Repos/Entities/TemplateCategory.cs`
- **Entity Structure**:
  ```csharp
  public class CalendarTemplate
  {
      public int Id { get; set; }
      public string Name { get; set; }
      public string Description { get; set; }
      public int CategoryId { get; set; }
      public TemplateCategory Category { get; set; }
      public string Style { get; set; } // Modern, Classic, Fun, Elegant
      public string ColorScheme { get; set; } // Primary colors
      public decimal Price { get; set; }
      public string[] ImageUrls { get; set; } // 12 images
      public bool IsFeatured { get; set; }
      public int DownloadCount { get; set; }
      public DateTime CreatedAt { get; set; }
  }
  ```
- **Migration**:
  - [ ] Create migration: `dotnet ef migrations add AddCalendarTemplates`
  - [ ] Update database: `dotnet ef database update`
- **Acceptance Criteria**:
  - [ ] Entities created with proper relationships
  - [ ] Migration applied successfully
  - [ ] Seed data with 20+ templates
- **Testing**:
  - [ ] Run migration on dev database
  - [ ] Verify tables created
  - [ ] Query templates via SQL

---

**Task 3.2: Implement Catalog Filters**
- **Priority**: 🟡 MEDIUM
- **Estimate**: 2 days
- **Description**: Add filters to catalog page
- **Files to modify**:
  - `src/Calendary.Ng/src/app/pages/catalog/catalog.component.ts`
  - `src/Calendary.Ng/src/app/pages/catalog/catalog.component.html`
  - `src/Calendary.Ng/src/app/pages/catalog/catalog.component.scss`
- **Filters**:
  - Category dropdown (Сім'я, Подорожі, Мінімалізм, Бізнес, Природа)
  - Style dropdown (Modern, Classic, Fun, Elegant)
  - Color scheme (multi-select chips)
  - Price range slider (0 - 1000 грн)
- **Sorting**:
  - Популярні (by `DownloadCount`)
  - Нові (by `CreatedAt DESC`)
  - Ціна ↓ / Ціна ↑
- **Acceptance Criteria**:
  - [ ] Filters work in combination
  - [ ] URL params update on filter change (shareable links)
  - [ ] "Clear Filters" button
  - [ ] Mobile: filters in collapsible panel
  - [ ] Loading state while fetching filtered results
- **Testing**:
  - [ ] Apply each filter individually
  - [ ] Apply multiple filters together
  - [ ] Copy URL with filters → paste in new tab → filters applied

---

**Task 3.3: Template Preview Modal**
- **Priority**: 🟡 MEDIUM
- **Estimate**: 1.5 days
- **Description**: Implement detailed template preview
- **Files to create**:
  - `src/Calendary.Ng/src/app/components/template-preview-modal/template-preview-modal.component.ts`
- **Features**:
  - Hover on template card → quick preview (tooltip with larger image)
  - Click on template card → open modal
  - Modal shows:
    - All 12 month images (carousel or grid)
    - Template name, description
    - Category, style, color scheme
    - Price
    - "Use This Template" button → navigate to `/customize-template/:id`
    - "Add to Favorites" button (optional)
- **Acceptance Criteria**:
  - [ ] Modal opens smoothly (fade animation)
  - [ ] Close on backdrop click or X button
  - [ ] Carousel for 12 images (arrows + dots)
  - [ ] Mobile responsive (full screen on small devices)
- **Testing**:
  - [ ] Open modal, navigate through 12 images
  - [ ] Click "Use This Template" → navigate correctly
  - [ ] Close modal (backdrop, X button, ESC key)

---

**Task 3.4: Quick Customize Page**
- **Priority**: 🟡 MEDIUM
- **Estimate**: 2 days
- **Description**: Create quick customization page for templates
- **Files to create**:
  - `src/Calendary.Ng/src/app/pages/customize-template/customize-template.component.ts`
- **Files to modify**:
  - `src/Calendary.Ng/src/app/app.routes.ts` (add `/customize-template/:id`)
- **Features**:
  - Load template by ID
  - Preview calendar (all 12 months)
  - Quick customization options:
    - Color scheme selector (5-6 presets)
    - Important dates (date picker + label)
    - Language (UK/EN toggle)
    - Week start (Monday/Sunday toggle)
  - Real-time preview update
  - Buttons:
    - "Open in Full Editor" → `/editor?templateId={id}&customizations={json}`
    - "Add to Cart" → create calendar, add to cart, navigate to `/cart`
- **Acceptance Criteria**:
  - [ ] Template loads correctly
  - [ ] Customizations apply in real-time
  - [ ] "Add to Cart" creates calendar in backend
  - [ ] Mobile responsive
- **Testing**:
  - [ ] Load template, change colors → preview updates
  - [ ] Add important dates → appear on preview
  - [ ] Switch language → month names change
  - [ ] "Add to Cart" → cart item created

---

### 📦 PHASE 4: Editor Enhancement (Week 4-5) 🟡 MEDIUM PRIORITY

**Task 4.1: Implement Autosave**
- **Priority**: 🔥 HIGH (UX improvement)
- **Estimate**: 1 day
- **Description**: Auto-save editor changes every 30 seconds
- **Files to modify**:
  - `src/Calendary.Ng/src/app/pages/editor/editor.component.ts`
  - `src/Calendary.Api/Controllers/CalendarController.cs` (add `PATCH /api/calendars/{id}`)
- **Implementation**:
  - Use RxJS `debounceTime(30000)` on calendar changes
  - Show "Saving..." / "Saved" indicator
  - Handle errors gracefully (retry logic)
- **Acceptance Criteria**:
  - [ ] Changes auto-save every 30 seconds
  - [ ] Manual save button still works
  - [ ] "Last saved at HH:MM" timestamp shown
  - [ ] No duplicate saves (debounced)
- **Testing**:
  - [ ] Make change → wait 30s → verify saved to backend
  - [ ] Make multiple changes rapidly → only one save triggered
  - [ ] Simulate network error → see retry

---

**Task 4.2: Undo/Redo Functionality**
- **Priority**: 🟡 MEDIUM
- **Estimate**: 2 days
- **Description**: Implement undo/redo for editor actions
- **Files to modify**:
  - `src/Calendary.Ng/src/app/pages/editor/editor.component.ts`
- **Implementation**:
  - Command pattern: store action history (stack)
  - Ctrl+Z (undo), Ctrl+Shift+Z (redo)
  - UI buttons for undo/redo
  - Max history: 50 actions
- **Acceptance Criteria**:
  - [ ] Undo reverts last action
  - [ ] Redo re-applies undone action
  - [ ] Keyboard shortcuts work (Ctrl+Z, Ctrl+Y)
  - [ ] Buttons disabled when no actions to undo/redo
- **Testing**:
  - [ ] Add text → undo → text removed
  - [ ] Undo → redo → text reappears
  - [ ] Multiple undo/redo cycles

---

**Task 4.3: PDF Export**
- **Priority**: 🟡 MEDIUM
- **Estimate**: 1.5 days
- **Description**: Add "Export PDF" button in editor
- **Files to modify**:
  - `src/Calendary.Ng/src/app/pages/editor/editor.component.ts` (add export button)
  - `src/Calendary.Api/Controllers/CalendarController.cs` (add `GET /api/calendars/{id}/pdf`)
- **Features**:
  - "Export PDF" button in toolbar
  - Backend generates PDF using existing `PdfService`
  - Frontend downloads PDF file
  - Loading state during generation
- **Acceptance Criteria**:
  - [ ] Button triggers PDF generation
  - [ ] PDF downloads automatically
  - [ ] PDF matches editor preview
  - [ ] Loading spinner during generation
- **Testing**:
  - [ ] Click "Export PDF" → PDF downloads
  - [ ] Open PDF → verify layout matches preview
  - [ ] Test with different calendar designs

---

**Task 4.4: Mobile Editor (Simplified)**
- **Priority**: 🟢 LOW (optional)
- **Estimate**: 3 days
- **Description**: Create simplified mobile editor experience
- **Files to modify**:
  - `src/Calendary.Ng/src/app/pages/editor/editor.component.scss`
  - `src/Calendary.Ng/src/app/pages/editor/editor.component.ts`
- **Features**:
  - Simplified toolbar (fewer options)
  - Touch gestures:
    - Pinch to zoom
    - Drag to move elements
    - Tap to select
  - Bottom sheet for element properties (instead of right sidebar)
- **Acceptance Criteria**:
  - [ ] Editor usable on mobile (iPhone, Android)
  - [ ] Touch gestures work smoothly
  - [ ] No horizontal scroll issues
- **Testing**:
  - [ ] Open editor on mobile device
  - [ ] Add text, move elements
  - [ ] Pinch to zoom

---

### 📦 PHASE 5: Checkout & Payment (Week 5-6) 🔥 HIGH PRIORITY

**Task 5.1: Implement Promo Codes**
- **Priority**: 🟡 MEDIUM
- **Estimate**: 2 days
- **Description**: Add promo code system
- **Files to create**:
  - `src/Calendary.Repos/Entities/PromoCode.cs`
  - `src/Calendary.Api/Controllers/PromoCodeController.cs`
- **Entity**:
  ```csharp
  public class PromoCode
  {
      public int Id { get; set; }
      public string Code { get; set; } // e.g., "NEWYEAR2025"
      public decimal DiscountPercent { get; set; }
      public decimal? DiscountAmount { get; set; }
      public DateTime ValidFrom { get; set; }
      public DateTime ValidTo { get; set; }
      public int? MaxUses { get; set; }
      public int CurrentUses { get; set; }
      public bool IsActive { get; set; }
  }
  ```
- **Frontend**:
  - Input field in cart: "Enter promo code"
  - "Apply" button
  - Show discount amount
- **Acceptance Criteria**:
  - [ ] Promo codes validated on backend
  - [ ] Discount applied to cart total
  - [ ] Expired/invalid codes rejected
  - [ ] Max uses enforced
- **Testing**:
  - [ ] Apply valid code → discount applied
  - [ ] Apply expired code → error message
  - [ ] Apply code at max uses → error
  - [ ] Remove code → discount removed

---

**Task 5.2: Streamline Checkout (3 Steps)**
- **Priority**: 🔥 HIGH
- **Estimate**: 2 days
- **Description**: Simplify checkout process to 3 clear steps
- **Files to modify**:
  - `src/Calendary.Ng/src/app/pages/order/order.component.ts`
  - `src/Calendary.Ng/src/app/pages/order/order.component.html`
- **New Structure**:
  - **Step 1**: Shipping Info (Nova Poshta: city, warehouse, contacts)
  - **Step 2**: Payment Method (MonoBank online, Cash on Delivery)
  - **Step 3**: Review & Confirm (summary, terms checkbox, place order)
- **Features**:
  - Progress indicator (Step 1/3 → 3/3)
  - "Back" button to previous step
  - Order summary sidebar (always visible, sticky on scroll)
- **Acceptance Criteria**:
  - [ ] Only 3 steps (combined previous multi-step flow)
  - [ ] Progress indicator clear
  - [ ] All required fields validated
  - [ ] Order summary visible at all times
- **Testing**:
  - [ ] Complete checkout from start to finish
  - [ ] Go back from step 3 to step 1
  - [ ] Required field validation

---

**Task 5.3: Order Confirmation Email**
- **Priority**: 🔥 HIGH
- **Estimate**: 1 day
- **Description**: Send detailed order confirmation email
- **Files to modify**:
  - `src/Calendary.Core/Services/EmailService.cs`
  - `src/Calendary.Core/Templates/` (add order confirmation template)
- **Email Content**:
  - Subject: "Замовлення #12345 підтверджено! 📦"
  - Order number, date
  - Items ordered (calendar preview)
  - Total amount paid
  - Delivery address (Nova Poshta)
  - Estimated delivery date
  - "Track Order" button → `/profile/orders/{id}`
  - "Download PDF" button
- **Acceptance Criteria**:
  - [ ] Email sent immediately after payment confirmation
  - [ ] All order details included
  - [ ] Links work correctly
  - [ ] Mobile-friendly HTML
- **Testing**:
  - [ ] Complete order → verify email received
  - [ ] Click "Track Order" → navigate to order page
  - [ ] Click "Download PDF" → PDF downloads

---

### 📦 PHASE 6: Profile & Dashboard (Week 6-7) 🟢 MEDIUM-LOW PRIORITY

**Task 6.1: User Dashboard**
- **Priority**: 🟢 MEDIUM
- **Estimate**: 2 days
- **Description**: Create user dashboard overview
- **Files to modify**:
  - `src/Calendary.Ng/src/app/pages/profile/profile.component.ts`
  - `src/Calendary.Ng/src/app/pages/profile/profile.component.html`
- **Dashboard Sections**:
  - **Active Orders** (cards with status)
  - **Saved Designs** (3 recent)
  - **AI Models** (status: training / ready)
  - **Quick Actions** (buttons: Create New, Browse Templates, View Orders)
- **Acceptance Criteria**:
  - [ ] Dashboard loads user data
  - [ ] Cards clickable → navigate to details
  - [ ] Loading states for async data
  - [ ] Empty states (no orders, no designs)
- **Testing**:
  - [ ] Login → dashboard shows correct data
  - [ ] Click order card → navigate to order details
  - [ ] Click "Create New" → navigate to onboarding

---

**Task 6.2: My Orders Page**
- **Priority**: 🟢 MEDIUM
- **Estimate**: 1.5 days
- **Description**: List all user orders with tracking
- **Files to create**:
  - `src/Calendary.Ng/src/app/pages/profile/orders/orders.component.ts`
- **Features**:
  - Table/List of orders (order #, date, status, total)
  - Filter by status (All, Processing, Shipped, Delivered)
  - Search by order number
  - Click order → view details modal
  - "Reorder" button (duplicate order)
  - "Download PDF" button
  - "Track Delivery" button (Nova Poshta TTN link)
- **Acceptance Criteria**:
  - [ ] All orders listed
  - [ ] Filters work
  - [ ] Order details modal shows full info
  - [ ] PDF download works
- **Testing**:
  - [ ] View orders list
  - [ ] Filter by status → only matching orders shown
  - [ ] Click order → details modal opens
  - [ ] Click "Download PDF" → PDF downloads

---

**Task 6.3: My Designs Page**
- **Priority**: 🟢 MEDIUM
- **Estimate**: 1.5 days
- **Description**: List saved calendar designs
- **Files to create**:
  - `src/Calendary.Ng/src/app/pages/profile/designs/designs.component.ts`
- **Features**:
  - Grid of saved designs (thumbnail + name)
  - "Edit" button → open in editor
  - "Duplicate" button → create copy
  - "Delete" button (with confirmation)
  - "Add to Cart" button
  - Filter by date created
- **Acceptance Criteria**:
  - [ ] All designs listed with thumbnails
  - [ ] Edit button opens editor with design loaded
  - [ ] Duplicate creates new design copy
  - [ ] Delete removes design (with confirmation)
- **Testing**:
  - [ ] View designs list
  - [ ] Edit design → editor opens
  - [ ] Duplicate → new design created
  - [ ] Delete → design removed

---

### 📦 PHASE 7: Auth & Security (Week 7) 🟡 MEDIUM PRIORITY

**Task 7.1: Enhanced Password Recovery**
- **Priority**: 🟡 MEDIUM
- **Estimate**: 1 day
- **Description**: Improve forgot password flow
- **Files to modify**:
  - `src/Calendary.Ng/src/app/pages/forgot-password/forgot-password.component.ts`
  - `src/Calendary.Api/Controllers/AuthController.cs`
- **Improvements**:
  - Clear instructions
  - "Check your email" success screen
  - "Didn't receive email?" → resend button
  - Email template improvement (branded, clear CTA)
- **Acceptance Criteria**:
  - [ ] User enters email → receives reset link
  - [ ] Reset link expires after 1 hour
  - [ ] Token-based validation
  - [ ] Success/error messages clear
- **Testing**:
  - [ ] Request password reset → email received
  - [ ] Click link → navigate to reset page
  - [ ] Set new password → login with new password
  - [ ] Try expired link → error message

---

**Task 7.2: Social Login (Optional)**
- **Priority**: 🟢 LOW
- **Estimate**: 3 days
- **Description**: Add Google/Facebook login
- **Files to modify**:
  - `src/Calendary.Ng/src/app/pages/login/login.component.ts`
  - `src/Calendary.Api/Controllers/AuthController.cs`
- **Providers**:
  - Google OAuth
  - Facebook OAuth
- **Acceptance Criteria**:
  - [ ] "Login with Google" button
  - [ ] OAuth flow works
  - [ ] User created/linked on first login
  - [ ] JWT token issued after social login
- **Testing**:
  - [ ] Click "Login with Google" → OAuth flow → login success
  - [ ] Login with existing Google account → user data loaded

---

### 📦 PHASE 8: Admin Panel (Week 8) 🟢 LOW PRIORITY

**Task 8.1: Order Management Bulk Actions**
- **Priority**: 🟢 LOW
- **Estimate**: 1.5 days
- **Description**: Add bulk actions for order management
- **Files to modify**:
  - `src/Calendary.Ng/src/app/admin/pages/admin-order/admin-order.component.ts`
- **Bulk Actions**:
  - Select multiple orders (checkboxes)
  - "Mark as Shipped" → update status for all selected
  - "Export to CSV" → download CSV with order data
  - "Print Labels" → generate shipping labels PDF
- **Acceptance Criteria**:
  - [ ] Checkbox column added to table
  - [ ] "Select All" checkbox
  - [ ] Bulk actions dropdown
  - [ ] Actions execute on selected orders only
- **Testing**:
  - [ ] Select 5 orders → Mark as Shipped → all 5 updated
  - [ ] Export to CSV → file downloads with correct data

---

**Task 8.2: Analytics Dashboard**
- **Priority**: 🟢 LOW
- **Estimate**: 3 days
- **Description**: Create admin analytics dashboard
- **Files to create**:
  - `src/Calendary.Ng/src/app/admin/pages/analytics/analytics.component.ts`
- **Metrics**:
  - **Sales**: Total revenue (daily, weekly, monthly)
  - **Conversion Funnel**: Landing → Register → First Order
  - **Popular Templates**: Most used templates
  - **User Growth**: New registrations over time
  - **Order Status**: Pie chart (Processing, Shipped, Delivered)
- **Charts**:
  - Use Chart.js or ng-charts
  - Line chart (revenue over time)
  - Funnel chart (conversion)
  - Bar chart (templates popularity)
- **Acceptance Criteria**:
  - [ ] Dashboard loads with real data
  - [ ] Charts render correctly
  - [ ] Date range filter (last 7 days, 30 days, all time)
  - [ ] Export charts as PNG
- **Testing**:
  - [ ] Open analytics dashboard → charts render
  - [ ] Change date range → data updates
  - [ ] Export chart → PNG downloads

---

## 🚀 Deployment Plan

### Step 1: Merge Current PR
- [ ] Review PR #265 (modern landing page)
- [ ] Merge to `main` branch
- [ ] Verify CI/CD pipeline passes

### Step 2: Backend Deployment
- [ ] Build backend Docker image: `docker build -t calendary-api:latest .`
- [ ] Push to DigitalOcean Container Registry
- [ ] Update docker-compose.yml on production server
- [ ] Run migrations: `dotnet ef database update`
- [ ] Restart API containers: `docker-compose up -d --build`
- [ ] Health check: `curl https://api.calendary.com.ua/health`

### Step 3: Frontend Deployment
- [ ] Build Angular for production: `npm run build -- --configuration production`
- [ ] Build SSR: `npm run build:ssr`
- [ ] Upload `dist/` to server: `/var/www/calendary-ng/`
- [ ] Restart nginx: `sudo systemctl restart nginx`
- [ ] Verify: `https://calendary.com.ua`

### Step 4: Post-Deployment Testing
- [ ] Landing page loads
- [ ] Register new user
- [ ] Login works
- [ ] Create AI model (upload photos)
- [ ] Check email (training notification)
- [ ] Generate calendar
- [ ] Add to cart
- [ ] Checkout (test payment with MonoBank sandbox)
- [ ] Admin panel accessible

### Step 5: Monitoring
- [ ] Check logs: `docker-compose logs -f api`
- [ ] Monitor RabbitMQ queues
- [ ] Check Replicate API usage
- [ ] Monitor error rates (Sentry/Application Insights)

---

## 📊 Success Metrics (Track After Deployment)

### Week 1-2 (After Phase 1)
- [ ] Landing → Register conversion: Target >5% (current unknown)
- [ ] Bounce rate: Target <60%
- [ ] Time on landing page: Target >2 minutes

### Week 3-4 (After Phase 2)
- [ ] Register → Create Model: Target >60%
- [ ] AI training completion rate: Target >85% (current ~85% - 15% drop)
- [ ] Email notification open rate: Target >40%

### Week 5-6 (After Phase 5)
- [ ] Cart abandonment: Target <30% (current 40%)
- [ ] Checkout completion: Target >70%

### Week 8 (Final)
- [ ] Overall conversion (Visit → Purchase): Target >1.75% (current 0.45%)
- [ ] Average order value: Target >500 грн
- [ ] User retention (7-day): Target >30%

---

## 🎯 Quick Wins Priority Order

**Week 1 (Can do immediately):**
1. ✅ Merge PR #265 (Landing Page) - 1 hour
2. 🚀 Task 1.4: Add real images - 1 day
3. 🚀 Task 1.5: Add 3 CTA buttons - 4 hours
4. 🚀 Task 2.3: Email after AI training - 1 day
5. 🚀 Task 4.1: Editor autosave - 1 day

**Total Quick Wins**: 4 days of work, 40% impact on user experience

---

**Status**: ✅ Ready for Implementation
**Next Action**: Review and prioritize tasks, start with Quick Wins
**Estimated Total Timeline**: 8 weeks for full roadmap
**Quick Wins Timeline**: 1 week for immediate impact
