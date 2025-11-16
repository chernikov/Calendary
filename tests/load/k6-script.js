import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Trend, Rate, Counter } from 'k6/metrics';

const API_BASE_URL = __ENV.API_BASE_URL || 'http://localhost:5000';
const EDITOR_URL = __ENV.EDITOR_URL || 'http://localhost:4200/editor';
const MODEL_VERSION = __ENV.MODEL_VERSION || 'flux-pro-v1.1';
const IMAGE_PROMPT = __ENV.IMAGE_PROMPT || 'Ukrainian winter village, cinematic lighting, calendar art';
const CALENDAR_ID = __ENV.CALENDAR_ID || 1;
const AUTH_TOKEN = __ENV.API_TOKEN ? `Bearer ${__ENV.API_TOKEN}` : null;
const DEFAULT_SLEEP = Number(__ENV.SLEEP_SECONDS || 1);

const headers = {
  'Content-Type': 'application/json',
  ...(AUTH_TOKEN ? { Authorization: AUTH_TOKEN } : {}),
};

export const responseTime = new Trend('response_time_ms');
export const errorRate = new Rate('error_rate');
export const throughput = new Counter('request_throughput');

export const options = {
  thresholds: {
    http_req_duration: ['p(95)<3000'],
    error_rate: ['rate<0.01'],
  },
  scenarios: {
    image_generation: {
      executor: 'constant-vus',
      vus: 10,
      duration: '3m',
      exec: 'imageGenerationScenario',
    },
    pdf_generation: {
      executor: 'constant-vus',
      vus: 5,
      duration: '3m',
      exec: 'pdfGenerationScenario',
    },
    editor_concurrency: {
      executor: 'constant-vus',
      vus: 100,
      duration: '2m',
      exec: 'editorScenario',
    },
    spike_test: {
      executor: 'ramping-arrival-rate',
      exec: 'spikeScenario',
      startRate: 0,
      timeUnit: '1s',
      preAllocatedVUs: 50,
      maxVUs: 150,
      stages: [
        { target: 20, duration: '30s' },
        { target: 100, duration: '1m' },
        { target: 0, duration: '30s' },
      ],
    },
    soak_test: {
      executor: 'constant-vus',
      vus: 20,
      duration: '60m',
      exec: 'soakScenario',
      gracefulStop: '2m',
    },
  },
};

function trackResult(res, label) {
  responseTime.add(res.timings.duration, { label });
  throughput.add(1, { label });
  const ok = check(res, {
    [`${label} status is 2xx`]: (r) => r.status >= 200 && r.status < 300,
    [`${label} duration < 3s`]: (r) => r.timings.duration < 3000,
  });
  if (!ok) {
    errorRate.add(1, { label });
  }
}

export function imageGenerationScenario() {
  group('image-generation', () => {
    const payload = JSON.stringify({
      prompt: IMAGE_PROMPT,
      seed: Math.floor(Math.random() * 100000),
      modelVersion: MODEL_VERSION,
    });

    const res = http.post(`${API_BASE_URL}/api/synthesis/generate`, payload, {
      headers,
    });
    trackResult(res, 'image');
    sleep(DEFAULT_SLEEP);
  });
}

export function pdfGenerationScenario() {
  group('pdf-generation', () => {
    const res = http.get(`${API_BASE_URL}/api/calendar/generate/${CALENDAR_ID}`, {
      headers: AUTH_TOKEN ? { Authorization: AUTH_TOKEN } : undefined,
    });
    trackResult(res, 'pdf');
    sleep(DEFAULT_SLEEP);
  });
}

export function editorScenario() {
  group('editor-page', () => {
    const res = http.get(EDITOR_URL);
    trackResult(res, 'editor');
    sleep(DEFAULT_SLEEP);
  });
}

export function spikeScenario() {
  // Spike test hits image generation endpoint because it is the most CPU intensive
  const payload = JSON.stringify({
    prompt: `${IMAGE_PROMPT} :: spike`,
    seed: Math.floor(Math.random() * 1000000),
    modelVersion: MODEL_VERSION,
  });
  const res = http.post(`${API_BASE_URL}/api/synthesis/generate`, payload, {
    headers,
  });
  trackResult(res, 'spike');
  sleep(0.5);
}

export function soakScenario() {
  // Soak test keeps the editor open for a prolonged time
  const res = http.get(EDITOR_URL);
  trackResult(res, 'soak');
  sleep(DEFAULT_SLEEP);
}
