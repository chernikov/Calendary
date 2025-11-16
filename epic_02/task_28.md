# Task 28: Performance optimization

**Epic**: [Epic 02 - Customer Portal](../epic_02.md)
**Статус**: TODO
**Пріоритет**: P1 (Високий)
**Складність**: Середня
**Час**: 5-6 годин
**Відповідальний AI**: Claude
**Залежить від**: Всі попередні

## Опис задачі

Оптимізувати performance Customer Portal: Lighthouse audit, lazy loading, code splitting, caching, CDN.

## Проблема

Потрібно забезпечити швидке завантаження та smooth UX для всіх користувачів, особливо на мобільних пристроях.

## Що треба зробити

1. **Lighthouse Audit**
   - Run Lighthouse для всіх ключових сторінок
   - Target: Score > 90 (Performance, Accessibility, Best Practices, SEO)
   - Зафіксувати baseline metrics
   - Ідентифікувати bottlenecks

2. **Image Optimization**
   - Next.js Image component (вже використовується)
   - Responsive images (srcset)
   - WebP format
   - Lazy loading
   - Blur placeholders
   - CDN для images

3. **Code Splitting**
   - Dynamic imports для великих компонентів
   - Route-based code splitting (Next.js автоматично)
   - Component-level splitting
   - Bundle size analysis

4. **Lazy Loading**
   - Below-the-fold content
   - Modal компоненти
   - Heavy components (Canvas editor)
   - Intersection Observer

5. **Caching Strategy**
   - Browser caching headers
   - Service Worker (Next.js PWA)
   - API response caching (SWR або React Query)
   - Static generation де можливо

6. **CDN Configuration**
   - Static assets на CDN
   - Image CDN (Cloudflare, Cloudinary)
   - Font optimization
   - Preconnect до external domains

7. **Bundle Size Reduction**
   - Tree shaking
   - Remove unused dependencies
   - Analyze bundle з webpack-bundle-analyzer
   - Split vendor chunks

8. **Performance Monitoring**
   - Google Analytics Core Web Vitals
   - Error tracking (Sentry)
   - Performance metrics dashboard

## Файли для створення/модифікації

- `next.config.js` - optimization config
- `src/components/LazyLoad.tsx` - wrapper component
- `.github/workflows/lighthouse-ci.yml`
- `package.json` - remove unused deps
- `src/lib/analytics.ts` - performance tracking

## Критерії успіху

- [ ] Lighthouse Performance Score > 90
- [ ] Time to Interactive < 3s
- [ ] First Contentful Paint < 1.5s
- [ ] Largest Contentful Paint < 2.5s
- [ ] Cumulative Layout Shift < 0.1
- [ ] Total bundle size < 500KB (initial)
- [ ] Images optimized (WebP, lazy load)

## Залежності

- Всі features повинні бути імплементовані

## Технічні деталі

### next.config.js Optimization
```javascript
/** @type {import('next').NextConfig} */
const nextConfig = {
  images: {
    formats: ['image/webp', 'image/avif'],
    domains: ['storage.googleapis.com', 'cdn.calendary.com'],
    deviceSizes: [640, 750, 828, 1080, 1200, 1920, 2048, 3840],
    imageSizes: [16, 32, 48, 64, 96, 128, 256, 384],
  },

  // Compiler optimizations
  compiler: {
    removeConsole: process.env.NODE_ENV === 'production',
  },

  // Strict mode for performance
  reactStrictMode: true,

  // Minification
  swcMinify: true,

  // Headers for caching
  async headers() {
    return [
      {
        source: '/:all*(svg|jpg|png|webp)',
        headers: [
          {
            key: 'Cache-Control',
            value: 'public, max-age=31536000, immutable',
          },
        ],
      },
    ]
  },
}

module.exports = nextConfig
```

### Dynamic Import для Heavy Components
```typescript
// Instead of:
// import CanvasEditor from '@/components/features/editor/CanvasEditor'

// Use:
import dynamic from 'next/dynamic'

const CanvasEditor = dynamic(
  () => import('@/components/features/editor/CanvasEditor'),
  {
    loading: () => <div>Завантаження редактора...</div>,
    ssr: false, // Canvas works only on client
  }
)
```

### Lazy Load Component
```typescript
'use client'

import { useEffect, useRef, useState } from 'react'

interface LazyLoadProps {
  children: React.ReactNode
  height?: number
}

export default function LazyLoad({ children, height = 300 }: LazyLoadProps) {
  const ref = useRef<HTMLDivElement>(null)
  const [isVisible, setIsVisible] = useState(false)

  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsVisible(true)
          observer.disconnect()
        }
      },
      { rootMargin: '100px' }
    )

    if (ref.current) {
      observer.observe(ref.current)
    }

    return () => observer.disconnect()
  }, [])

  return (
    <div ref={ref} style={{ minHeight: height }}>
      {isVisible ? children : null}
    </div>
  )
}
```

### Bundle Analysis
```json
// package.json
{
  "scripts": {
    "analyze": "ANALYZE=true next build"
  },
  "devDependencies": {
    "@next/bundle-analyzer": "^14.0.0"
  }
}
```

```javascript
// next.config.js
const withBundleAnalyzer = require('@next/bundle-analyzer')({
  enabled: process.env.ANALYZE === 'true',
})

module.exports = withBundleAnalyzer(nextConfig)
```

### SWR for API Caching
```typescript
import useSWR from 'swr'

export function useTemplates() {
  const { data, error, isLoading } = useSWR(
    '/api/templates',
    fetcher,
    {
      revalidateOnFocus: false,
      revalidateOnReconnect: false,
      dedupingInterval: 60000, // 1 min
    }
  )

  return {
    templates: data,
    isLoading,
    error,
  }
}
```

### Lighthouse CI
```yaml
# .github/workflows/lighthouse-ci.yml
name: Lighthouse CI

on:
  pull_request:
    branches: [main]

jobs:
  lighthouse:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Run Lighthouse CI
        uses: treosh/lighthouse-ci-action@v9
        with:
          urls: |
            http://localhost:3000/
            http://localhost:3000/catalog
            http://localhost:3000/editor/new
          budgetPath: ./budget.json
          uploadArtifacts: true
```

### budget.json
```json
[
  {
    "path": "/*",
    "resourceSizes": [
      {
        "resourceType": "script",
        "budget": 300
      },
      {
        "resourceType": "total",
        "budget": 500
      }
    ],
    "timings": [
      {
        "metric": "interactive",
        "budget": 3000
      },
      {
        "metric": "first-contentful-paint",
        "budget": 1500
      }
    ]
  }
]
```

## Примітки

- Next.js має багато вбудованих оптимізацій
- Image optimization критична для performance
- Code splitting зменшує initial bundle
- Caching зменшує API calls
- Lighthouse CI запобігає performance regression

## Чому Claude?

Performance optimization задача:
- Performance profiling та analysis
- Optimization strategies
- Bundle analysis interpretation
- Caching strategies
- Monitoring setup

---

**Створено**: 2025-11-16
**Оновлено**: 2025-11-16
**Виконано**: -
