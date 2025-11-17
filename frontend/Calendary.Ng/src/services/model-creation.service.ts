import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError, timer } from 'rxjs';
import { catchError, retry, shareReplay, tap, switchMap, filter } from 'rxjs/operators';
import { FluxModel } from '../models/flux-model';
import { ModelStatus, ModelStatusHelper } from '../models/model-status.enum';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

/**
 * Інтерфейс для створення моделі
 */
export interface CreateModelRequest {
  categoryId: number;
  name?: string;
}

/**
 * Інтерфейс для завантаження фото
 */
export interface UploadPhotosRequest {
  modelId: number;
  photos: File[];
}

/**
 * Інтерфейс для статусу створення моделі
 */
export interface ModelCreationState {
  model: FluxModel | null;
  status: ModelStatus;
  progress: number;
  error: string | null;
  isLoading: boolean;
}

/**
 * Сервіс для керування створенням та життєвим циклом моделей
 * Замінює старий master flow з правильною архітектурою
 */
@Injectable({
  providedIn: 'root'
})
export class ModelCreationService {
  private readonly apiUrl = '/api/flux-model';
  private readonly http = inject(HttpClient);

  // State management з BehaviorSubject
  private readonly stateSubject = new BehaviorSubject<ModelCreationState>({
    model: null,
    status: ModelStatus.Created,
    progress: 0,
    error: null,
    isLoading: false
  });

  // Публічний observable для компонентів
  public readonly state$ = this.stateSubject.asObservable();

  // SignalR connection для real-time оновлень
  private hubConnection: HubConnection | null = null;
  private readonly modelUpdatesSubject = new BehaviorSubject<FluxModel | null>(null);
  public readonly modelUpdates$ = this.modelUpdatesSubject.asObservable();

  constructor() {
    this.initializeSignalR();
  }

  /**
   * Ініціалізація SignalR з'єднання
   */
  private initializeSignalR(): void {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl('/hubs/model-updates')
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(LogLevel.Information)
      .build();

    this.hubConnection.on('ModelStatusUpdated', (model: FluxModel) => {
      this.handleModelUpdate(model);
    });

    this.hubConnection.on('ModelTrainingProgress', (modelId: number, progress: number) => {
      const currentState = this.stateSubject.value;
      if (currentState.model?.id === modelId) {
        this.updateState({ progress });
      }
    });

    this.hubConnection.on('ModelError', (modelId: number, error: string) => {
      const currentState = this.stateSubject.value;
      if (currentState.model?.id === modelId) {
        this.updateState({
          error,
          status: ModelStatus.Failed,
          isLoading: false
        });
      }
    });

    // Автоматичне підключення
    this.startConnection();
  }

  /**
   * Запуск SignalR з'єднання
   */
  private async startConnection(): Promise<void> {
    if (!this.hubConnection) return;

    try {
      await this.hubConnection.start();
      console.log('SignalR connected for model updates');
    } catch (err) {
      console.error('Error starting SignalR connection:', err);
      // Повторна спроба через 5 секунд
      setTimeout(() => this.startConnection(), 5000);
    }
  }

  /**
   * Підписка на оновлення конкретної моделі
   */
  private async subscribeToModel(modelId: number): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      await this.hubConnection.invoke('SubscribeToModel', modelId);
    }
  }

  /**
   * Відписка від оновлень моделі
   */
  private async unsubscribeFromModel(modelId: number): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      await this.hubConnection.invoke('UnsubscribeFromModel', modelId);
    }
  }

  /**
   * Обробка оновлення моделі від SignalR
   */
  private handleModelUpdate(model: FluxModel): void {
    const status = model.status as ModelStatus;
    const progress = ModelStatusHelper.getProgress(model.status);

    this.updateState({
      model,
      status,
      progress,
      isLoading: !ModelStatusHelper.isFinalState(model.status)
    });

    this.modelUpdatesSubject.next(model);
  }

  /**
   * Оновлення стану
   */
  private updateState(partial: Partial<ModelCreationState>): void {
    const currentState = this.stateSubject.value;
    this.stateSubject.next({ ...currentState, ...partial });
  }

  /**
   * Створення нової моделі
   */
  createModel(request: CreateModelRequest): Observable<FluxModel> {
    this.updateState({
      isLoading: true,
      error: null,
      status: ModelStatus.Created,
      progress: 0
    });

    return this.http.post<FluxModel>(this.apiUrl, request).pipe(
      retry({
        count: 3,
        delay: (error, retryCount) => {
          // Експоненційна затримка: 1s, 2s, 4s
          return timer(Math.pow(2, retryCount - 1) * 1000);
        }
      }),
      tap(model => {
        this.updateState({
          model,
          status: ModelStatus.Created,
          progress: ModelStatusHelper.getProgress(ModelStatus.Created),
          isLoading: false
        });

        // Підписатися на оновлення моделі
        this.subscribeToModel(model.id);
      }),
      catchError(error => {
        this.updateState({
          error: error.message || 'Помилка створення моделі',
          isLoading: false,
          status: ModelStatus.Failed
        });
        return throwError(() => error);
      }),
      shareReplay(1)
    );
  }

  /**
   * Завантаження фото для моделі
   */
  uploadPhotos(modelId: number, photos: File[]): Observable<any> {
    this.updateState({
      isLoading: true,
      error: null,
      status: ModelStatus.Uploading
    });

    const formData = new FormData();
    photos.forEach((photo, index) => {
      formData.append(`photos`, photo, photo.name);
    });

    return this.http.post(`${this.apiUrl}/${modelId}/photos`, formData, {
      reportProgress: true,
      observe: 'events'
    }).pipe(
      tap(() => {
        this.updateState({
          status: ModelStatus.AwaitingPayment,
          progress: ModelStatusHelper.getProgress(ModelStatus.AwaitingPayment),
          isLoading: false
        });
      }),
      catchError(error => {
        this.updateState({
          error: error.message || 'Помилка завантаження фото',
          isLoading: false
        });
        return throwError(() => error);
      })
    );
  }

  /**
   * Початок навчання моделі (після оплати)
   */
  startTraining(modelId: number): Observable<FluxModel> {
    this.updateState({
      isLoading: true,
      error: null,
      status: ModelStatus.Preparing
    });

    return this.http.post<FluxModel>(`${this.apiUrl}/${modelId}/train`, {}).pipe(
      tap(model => {
        this.updateState({
          model,
          status: ModelStatus.Training,
          progress: ModelStatusHelper.getProgress(ModelStatus.Training),
          isLoading: true // Залишається true поки тренується
        });
      }),
      catchError(error => {
        this.updateState({
          error: error.message || 'Помилка початку навчання',
          isLoading: false,
          status: ModelStatus.Failed
        });
        return throwError(() => error);
      })
    );
  }

  /**
   * Генерація прикладів зображень
   */
  generateExamples(modelId: number): Observable<FluxModel> {
    this.updateState({
      isLoading: true,
      error: null
    });

    return this.http.post<FluxModel>(`${this.apiUrl}/${modelId}/generate-examples`, {}).pipe(
      tap(model => {
        this.updateState({
          model,
          status: ModelStatus.ExamplesGenerated,
          progress: ModelStatusHelper.getProgress(ModelStatus.ExamplesGenerated),
          isLoading: false
        });
      }),
      catchError(error => {
        this.updateState({
          error: error.message || 'Помилка генерації прикладів',
          isLoading: false
        });
        return throwError(() => error);
      })
    );
  }

  /**
   * Генерація основних зображень для календаря
   */
  generateImages(modelId: number, promptThemeId?: number): Observable<FluxModel> {
    this.updateState({
      isLoading: true,
      error: null,
      status: ModelStatus.GeneratingImages
    });

    const url = promptThemeId
      ? `${this.apiUrl}/${modelId}/generate?promptThemeId=${promptThemeId}`
      : `${this.apiUrl}/${modelId}/generate`;

    return this.http.post<FluxModel>(url, {}).pipe(
      tap(model => {
        this.updateState({
          model,
          status: ModelStatus.GeneratingImages,
          progress: ModelStatusHelper.getProgress(ModelStatus.GeneratingImages)
        });
      }),
      catchError(error => {
        this.updateState({
          error: error.message || 'Помилка генерації зображень',
          isLoading: false
        });
        return throwError(() => error);
      })
    );
  }

  /**
   * Отримання поточної моделі користувача
   */
  getCurrentModel(): Observable<FluxModel> {
    return this.http.get<FluxModel>(this.apiUrl).pipe(
      tap(model => {
        const status = model.status as ModelStatus;
        this.updateState({
          model,
          status,
          progress: ModelStatusHelper.getProgress(model.status),
          isLoading: !ModelStatusHelper.isFinalState(model.status)
        });

        // Підписатися на оновлення
        if (model.id) {
          this.subscribeToModel(model.id);
        }
      }),
      catchError(error => {
        this.updateState({
          error: error.message || 'Помилка отримання моделі'
        });
        return throwError(() => error);
      })
    );
  }

  /**
   * Отримання моделі за ID
   */
  getModelById(id: number): Observable<FluxModel> {
    return this.http.get<FluxModel>(`${this.apiUrl}/${id}`).pipe(
      tap(model => {
        const status = model.status as ModelStatus;
        this.updateState({
          model,
          status,
          progress: ModelStatusHelper.getProgress(model.status),
          isLoading: !ModelStatusHelper.isFinalState(model.status)
        });

        this.subscribeToModel(model.id);
      }),
      shareReplay(1)
    );
  }

  /**
   * Отримання списку всіх моделей користувача
   */
  getModelsList(): Observable<FluxModel[]> {
    return this.http.get<FluxModel[]>(`${this.apiUrl}/list`).pipe(
      shareReplay(1)
    );
  }

  /**
   * Встановлення активної моделі
   */
  setActiveModel(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/set-active`, {}).pipe(
      switchMap(() => this.getModelById(id)),
      switchMap(() => [])
    );
  }

  /**
   * Перейменування моделі
   */
  renameModel(id: number, name: string): Observable<FluxModel> {
    return this.http.put<FluxModel>(`${this.apiUrl}/${id}/name`, { name }).pipe(
      tap(model => {
        const currentState = this.stateSubject.value;
        if (currentState.model?.id === id) {
          this.updateState({ model });
        }
      })
    );
  }

  /**
   * Видалення моделі
   */
  deleteModel(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        const currentState = this.stateSubject.value;
        if (currentState.model?.id === id) {
          this.unsubscribeFromModel(id);
          this.updateState({
            model: null,
            status: ModelStatus.Created,
            progress: 0,
            error: null,
            isLoading: false
          });
        }
      })
    );
  }

  /**
   * Архівація моделі
   */
  archiveModel(id: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/archive/${id}`, null).pipe(
      tap(() => {
        const currentState = this.stateSubject.value;
        if (currentState.model?.id === id) {
          this.updateState({
            status: ModelStatus.Archived,
            progress: -1
          });
        }
      })
    );
  }

  /**
   * Очищення стану
   */
  clearState(): void {
    const currentModel = this.stateSubject.value.model;
    if (currentModel) {
      this.unsubscribeFromModel(currentModel.id);
    }

    this.stateSubject.next({
      model: null,
      status: ModelStatus.Created,
      progress: 0,
      error: null,
      isLoading: false
    });
  }

  /**
   * Знищення сервісу
   */
  ngOnDestroy(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
}
