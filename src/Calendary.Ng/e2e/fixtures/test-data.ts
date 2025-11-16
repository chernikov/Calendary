/**
 * Test data and fixtures for E2E tests
 */

/**
 * Test user credentials
 */
export const testUsers = {
  regular: {
    email: 'test@example.com',
    password: 'Test123!@#',
  },
  admin: {
    email: 'admin@example.com',
    password: 'Admin123!@#',
  },
};

/**
 * Sample prompts for image generation
 */
export const samplePrompts = [
  'A beautiful sunset over mountains',
  'A cozy winter cabin in the snow',
  'Spring flowers in a garden',
  'Summer beach with palm trees',
  'Autumn forest with colorful leaves',
  'A starry night sky',
  'A peaceful lake with reflection',
  'A vibrant city skyline at night',
  'A cute puppy playing in grass',
  'A majestic eagle in flight',
  'A vintage car on a country road',
  'A modern architecture building',
];

/**
 * Month names (Ukrainian)
 */
export const monthNames = [
  'Січень',
  'Лютий',
  'Березень',
  'Квітень',
  'Травень',
  'Червень',
  'Липень',
  'Серпень',
  'Вересень',
  'Жовтень',
  'Листопад',
  'Грудень',
];

/**
 * Get a random prompt from sample prompts
 */
export function getRandomPrompt(): string {
  return samplePrompts[Math.floor(Math.random() * samplePrompts.length)];
}

/**
 * Get prompts for all 12 months
 */
export function getMonthlyPrompts(): string[] {
  // Return 12 unique prompts
  return samplePrompts.slice(0, 12);
}

/**
 * Test model data
 */
export const testModel = {
  name: 'Test Model',
  description: 'Model for E2E testing',
};

/**
 * Calendar test data
 */
export const calendarTestData = {
  year: new Date().getFullYear() + 1,
  title: 'Test Calendar',
};

/**
 * API endpoints (for mocking or direct API calls)
 */
export const apiEndpoints = {
  login: '/api/auth/login',
  logout: '/api/auth/logout',
  models: '/api/flux-models',
  generateImage: '/api/jobs',
  calendar: '/api/calendar',
};

/**
 * Timeouts (in milliseconds)
 */
export const timeouts = {
  imageGeneration: 60000, // 60 seconds
  pageLoad: 10000, // 10 seconds
  apiResponse: 5000, // 5 seconds
  pdfGeneration: 30000, // 30 seconds
};

/**
 * Test selectors (data-testid values)
 * TODO: Add these data-testid attributes to components
 */
export const selectors = {
  // Login page
  loginEmail: '[data-testid="login-email"]',
  loginPassword: '[data-testid="login-password"]',
  loginButton: '[data-testid="login-button"]',

  // Editor page
  modelsPanel: '[data-testid="models-panel"]',
  modelItem: '[data-testid="model-item"]',
  activeModel: '[data-testid="active-model"]',

  // Sidebar
  generateButton: '[data-testid="generate-button"]',
  uploadButton: '[data-testid="upload-button"]',
  saveButton: '[data-testid="save-button"]',
  exportButton: '[data-testid="export-button"]',

  // Generate modal
  promptInput: '[data-testid="prompt-input"]',
  generateSubmit: '[data-testid="generate-submit"]',
  generateCancel: '[data-testid="generate-cancel"]',

  // Image gallery
  imageGallery: '[data-testid="image-gallery"]',
  imageItem: '[data-testid="image-item"]',
  addToCalendarButton: '[data-testid="add-to-calendar"]',
  deleteImageButton: '[data-testid="delete-image"]',

  // Month selector
  monthSelector: '[data-testid="month-selector"]',
  monthOption: '[data-testid="month-option"]',
  monthConfirm: '[data-testid="month-confirm"]',
  monthCancel: '[data-testid="month-cancel"]',

  // Calendar preview
  calendarPreview: '[data-testid="calendar-preview"]',
  monthSlot: '[data-testid="month-slot"]',
  clearMonth: '[data-testid="clear-month"]',
  clearAll: '[data-testid="clear-all"]',
  generatePDF: '[data-testid="generate-pdf"]',

  // Canvas
  imageCanvas: '[data-testid="image-canvas"]',

  // Properties panel
  propertiesPanel: '[data-testid="properties-panel"]',
};
