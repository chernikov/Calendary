import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';

export interface ProgressUpdate {
  progress: number;
  status: string;
  estimatedTime?: number | null;
  error?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class ImageGenerationSignalRService {
  private connection: signalR.HubConnection | null = null;
  private progressSubject = new BehaviorSubject<ProgressUpdate | null>(null);
  public progress$: Observable<ProgressUpdate | null> = this.progressSubject.asObservable();

  private isConnected = false;
  private currentTaskId: string | null = null;

  constructor() {}

  /**
   * Підключення до SignalR Hub
   */
  async connect(): Promise<void> {
    if (this.isConnected) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/image-generation')
      .withAutomaticReconnect()
      .build();

    // Обробник оновлень прогресу
    this.connection.on('ProgressUpdate', (update: ProgressUpdate) => {
      console.log('Progress update received:', update);
      this.progressSubject.next(update);
    });

    try {
      await this.connection.start();
      this.isConnected = true;
      console.log('SignalR connection established');
    } catch (err) {
      console.error('Error connecting to SignalR:', err);
      throw err;
    }
  }

  /**
   * Відключення від SignalR Hub
   */
  async disconnect(): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        if (this.currentTaskId) {
          await this.leaveTaskGroup(this.currentTaskId);
        }
        await this.connection.stop();
        this.isConnected = false;
        this.connection = null;
        console.log('SignalR connection closed');
      } catch (err) {
        console.error('Error disconnecting from SignalR:', err);
      }
    }
  }

  /**
   * Приєднання до групи задачі для отримання оновлень прогресу
   */
  async joinTaskGroup(taskId: string): Promise<void> {
    if (!this.connection || !this.isConnected) {
      await this.connect();
    }

    if (this.currentTaskId && this.currentTaskId !== taskId) {
      await this.leaveTaskGroup(this.currentTaskId);
    }

    try {
      await this.connection!.invoke('JoinTaskGroup', taskId);
      this.currentTaskId = taskId;
      console.log(`Joined task group: ${taskId}`);

      // Скинути прогрес перед початком нової задачі
      this.progressSubject.next(null);
    } catch (err) {
      console.error('Error joining task group:', err);
      throw err;
    }
  }

  /**
   * Вийти з групи задачі
   */
  async leaveTaskGroup(taskId: string): Promise<void> {
    if (!this.connection || !this.isConnected) {
      return;
    }

    try {
      await this.connection.invoke('LeaveTaskGroup', taskId);
      if (this.currentTaskId === taskId) {
        this.currentTaskId = null;
      }
      console.log(`Left task group: ${taskId}`);
    } catch (err) {
      console.error('Error leaving task group:', err);
    }
  }

  /**
   * Скинути прогрес
   */
  resetProgress(): void {
    this.progressSubject.next(null);
  }

  /**
   * Отримати поточний стан підключення
   */
  get connected(): boolean {
    return this.isConnected;
  }
}
