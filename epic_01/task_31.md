# Task 31: Навантажувальне тестування

**Epic**: [Epic 01 - Перший реліз](../epic_01.md)
**Статус**: TODO
**Пріоритет**: P2 (Середній)
**Складність**: Висока
**Час**: 4-6 годин
**Відповідальний AI**: QA / DevOps

## Опис задачі

Перевірити як система веде себе під навантаженням.

## Що треба зробити

1. **Setup Load Testing Tools**:
   - k6, Artillery, або JMeter
   - Metrics collection (Prometheus + Grafana)

2. **Load Test Scenarios**:
   - 10 одночасних генерацій зображень
   - 5 одночасних генерацій PDF
   - 100 concurrent users в /editor
   - Spike test (різкий наплив користувачів)
   - Soak test (тривале навантаження 1 година)

3. **Metrics to Track**:
   - Response time (p50, p95, p99)
   - Throughput (requests/second)
   - Error rate
   - CPU usage
   - Memory usage
   - Database connections

4. **Acceptance Criteria**:
   - p95 response time <3 seconds
   - Error rate <1%
   - System stable під навантаженням 1 година
   - No memory leaks

## Файли для створення

- `tests/load/k6-script.js`
- `tests/load/scenarios.yml`

## Приклад k6 script

```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '1m', target: 10 },
    { duration: '3m', target: 50 },
    { duration: '1m', target: 0 },
  ],
};

export default function () {
  const res = http.post('http://localhost:5000/api/synthesis/generate', {
    prompt: 'Test prompt',
    modelId: 'test-model-id',
  });

  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 3000ms': (r) => r.timings.duration < 3000,
  });

  sleep(1);
}
```

## Що тестувати

- [ ] Load tests успішні
- [ ] Metrics в межах норми
- [ ] No crashes під навантаженням
- [ ] Auto-scaling працює (якщо налаштовано)

---

**Створено**: 2025-11-15
