/**
 * Enum для статусів FluxModel
 * Описує весь життєвий цикл моделі від створення до готовності
 */
export enum ModelStatus {
  /** Модель створена, очікується завантаження фото */
  Created = 'created',

  /** Завантаження фото в процесі */
  Uploading = 'uploading',

  /** Очікується оплата за навчання моделі */
  AwaitingPayment = 'pay_model',

  /** Підготовка до навчання (після оплати) */
  Preparing = 'prepare',

  /** Навчання моделі в процесі */
  Training = 'inprocess',

  /** Навчання завершено, модель готова */
  Trained = 'processed',

  /** Згенеровані приклади зображень */
  ExamplesGenerated = 'exampled',

  /** Генерація основних зображень в процесі */
  GeneratingImages = 'image_generating',

  /** Зображення згенеровані, вибрані для календаря */
  ImagesSelected = 'ready_selected',

  /** Дати додані до календаря */
  DatesAdded = 'dated',

  /** Календар повністю готовий */
  Ready = 'ready',

  /** Помилка в процесі створення/навчання */
  Failed = 'failed',

  /** Модель архівована */
  Archived = 'archived'
}

/**
 * Допоміжні методи для роботи зі статусами
 */
export class ModelStatusHelper {
  /**
   * Перевіряє чи модель в процесі навчання
   */
  static isTraining(status: string): boolean {
    return status === ModelStatus.Preparing ||
           status === ModelStatus.Training;
  }

  /**
   * Перевіряє чи модель готова до генерації зображень
   */
  static isReadyForGeneration(status: string): boolean {
    return status === ModelStatus.Trained ||
           status === ModelStatus.ExamplesGenerated ||
           status === ModelStatus.GeneratingImages ||
           status === ModelStatus.ImagesSelected ||
           status === ModelStatus.DatesAdded ||
           status === ModelStatus.Ready;
  }

  /**
   * Перевіряє чи потрібна оплата
   */
  static requiresPayment(status: string): boolean {
    return status === ModelStatus.AwaitingPayment;
  }

  /**
   * Перевіряє чи модель в фінальному стані
   */
  static isFinalState(status: string): boolean {
    return status === ModelStatus.Ready ||
           status === ModelStatus.Failed ||
           status === ModelStatus.Archived;
  }

  /**
   * Отримує відсоток прогресу на основі статусу
   */
  static getProgress(status: string): number {
    const progressMap: Record<string, number> = {
      [ModelStatus.Created]: 0,
      [ModelStatus.Uploading]: 10,
      [ModelStatus.AwaitingPayment]: 20,
      [ModelStatus.Preparing]: 30,
      [ModelStatus.Training]: 50,
      [ModelStatus.Trained]: 70,
      [ModelStatus.ExamplesGenerated]: 75,
      [ModelStatus.GeneratingImages]: 85,
      [ModelStatus.ImagesSelected]: 90,
      [ModelStatus.DatesAdded]: 95,
      [ModelStatus.Ready]: 100,
      [ModelStatus.Failed]: -1,
      [ModelStatus.Archived]: -1
    };
    return progressMap[status] ?? 0;
  }

  /**
   * Отримує локалізовану назву статусу
   */
  static getDisplayName(status: string): string {
    const displayNames: Record<string, string> = {
      [ModelStatus.Created]: 'Створено',
      [ModelStatus.Uploading]: 'Завантаження фото',
      [ModelStatus.AwaitingPayment]: 'Очікування оплати',
      [ModelStatus.Preparing]: 'Підготовка',
      [ModelStatus.Training]: 'Навчання моделі',
      [ModelStatus.Trained]: 'Навчання завершено',
      [ModelStatus.ExamplesGenerated]: 'Приклади згенеровані',
      [ModelStatus.GeneratingImages]: 'Генерація зображень',
      [ModelStatus.ImagesSelected]: 'Зображення вибрані',
      [ModelStatus.DatesAdded]: 'Дати додані',
      [ModelStatus.Ready]: 'Готово',
      [ModelStatus.Failed]: 'Помилка',
      [ModelStatus.Archived]: 'Архівовано'
    };
    return displayNames[status] ?? status;
  }
}
