import { HttpErrorResponse } from '@angular/common/http';

const MAX_PHOTO_MB = 20;

const BY_CODE: Record<string, string> = {
  photo_required: 'Файл не долучився. Оберіть фото ще раз.',
  photo_too_large: `Фото завелике — максимум ${MAX_PHOTO_MB} МБ.`,
  photo_unsupported_format: 'Непідтримуваний формат. Підійдуть JPEG, PNG або WebP.',
};

/// Maps a failed photo upload to a message that says what exactly to do differently.
export function photoUploadErrorMessage(err: unknown): string {
  const fallback = 'Не вдалося завантажити фото. Спробуйте ще раз.';
  if (!(err instanceof HttpErrorResponse)) {
    return fallback;
  }

  switch (err.status) {
    case 0:
      return 'Немає зв’язку із сервером. Перевірте інтернет і спробуйте ще раз.';
    case 400:
      return BY_CODE[errorCode(err)] ?? 'Файл не вдалося прочитати як зображення.';
    case 401:
      return 'Сесія завершилася. Увійдіть ще раз.';
    case 404:
      return 'Замовлення не знайдено.';
    // nginx rejects an oversized body before the API sees it, so this never carries a code.
    case 413:
      return `Фото завелике — максимум ${MAX_PHOTO_MB} МБ.`;
    default:
      return err.status >= 500 ? 'Сервер не зміг зберегти фото. Спробуйте за хвилину.' : fallback;
  }
}

function errorCode(err: HttpErrorResponse): string {
  const body = err.error;
  if (typeof body === 'string') return body;
  if (body && typeof body === 'object' && typeof body.error === 'string') return body.error;
  return '';
}
