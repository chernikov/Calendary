import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface PromptHistoryItem {
  id: string;
  text: string;
  timestamp: number;
  isPinned: boolean;
}

const STORAGE_KEY = 'calendary_prompt_history';
const MAX_HISTORY_ITEMS = 20;

@Injectable({
  providedIn: 'root'
})
export class PromptHistoryService {
  private historySubject = new BehaviorSubject<PromptHistoryItem[]>([]);
  public history$: Observable<PromptHistoryItem[]> = this.historySubject.asObservable();

  constructor() {
    this.loadHistory();
  }

  /**
   * Завантажити історію з LocalStorage
   */
  private loadHistory(): void {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) {
        const history = JSON.parse(stored) as PromptHistoryItem[];
        this.historySubject.next(history);
      }
    } catch (error) {
      console.error('Помилка завантаження історії промптів:', error);
      this.historySubject.next([]);
    }
  }

  /**
   * Зберегти історію в LocalStorage
   */
  private saveHistory(history: PromptHistoryItem[]): void {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(history));
      this.historySubject.next(history);
    } catch (error) {
      console.error('Помилка збереження історії промптів:', error);
    }
  }

  /**
   * Додати новий промпт до історії
   */
  addPrompt(text: string): void {
    if (!text || text.trim().length < 3) {
      return; // Не зберігаємо дуже короткі промпти
    }

    const trimmedText = text.trim();
    const currentHistory = this.historySubject.value;

    // Перевірити чи такий промпт вже існує
    const existingIndex = currentHistory.findIndex(
      item => item.text === trimmedText && !item.isPinned
    );

    if (existingIndex !== -1) {
      // Якщо існує, оновлюємо timestamp і переміщуємо на початок
      const existingItem = currentHistory[existingIndex];
      const updatedHistory = [
        { ...existingItem, timestamp: Date.now() },
        ...currentHistory.filter((_, i) => i !== existingIndex)
      ];
      this.saveHistory(updatedHistory);
      return;
    }

    // Створити новий елемент історії
    const newItem: PromptHistoryItem = {
      id: this.generateId(),
      text: trimmedText,
      timestamp: Date.now(),
      isPinned: false
    };

    // Додати до початку списку
    let updatedHistory = [newItem, ...currentHistory];

    // Відфільтрувати закріплені промпти
    const pinnedItems = updatedHistory.filter(item => item.isPinned);
    const unpinnedItems = updatedHistory.filter(item => !item.isPinned);

    // Обмежити кількість незакріплених елементів
    const limitedUnpinned = unpinnedItems.slice(0, MAX_HISTORY_ITEMS);

    // Об'єднати закріплені та незакріплені (закріплені завжди на початку)
    updatedHistory = [...pinnedItems, ...limitedUnpinned];

    this.saveHistory(updatedHistory);
  }

  /**
   * Закріпити промпт
   */
  togglePin(id: string): void {
    const currentHistory = this.historySubject.value;
    const updatedHistory = currentHistory.map(item =>
      item.id === id ? { ...item, isPinned: !item.isPinned } : item
    );

    // Відсортувати: закріплені на початку, потім за timestamp
    const sorted = this.sortHistory(updatedHistory);
    this.saveHistory(sorted);
  }

  /**
   * Видалити промпт з історії
   */
  removePrompt(id: string): void {
    const currentHistory = this.historySubject.value;
    const updatedHistory = currentHistory.filter(item => item.id !== id);
    this.saveHistory(updatedHistory);
  }

  /**
   * Очистити всю історію (крім закріплених)
   */
  clearHistory(): void {
    const currentHistory = this.historySubject.value;
    const pinnedOnly = currentHistory.filter(item => item.isPinned);
    this.saveHistory(pinnedOnly);
  }

  /**
   * Очистити всю історію (включно з закріпленими)
   */
  clearAllHistory(): void {
    this.saveHistory([]);
  }

  /**
   * Отримати поточну історію
   */
  getHistory(): PromptHistoryItem[] {
    return this.historySubject.value;
  }

  /**
   * Пошук в історії
   */
  searchHistory(query: string): PromptHistoryItem[] {
    if (!query || query.trim().length === 0) {
      return this.historySubject.value;
    }

    const lowerQuery = query.toLowerCase();
    return this.historySubject.value.filter(item =>
      item.text.toLowerCase().includes(lowerQuery)
    );
  }

  /**
   * Відсортувати історію: закріплені на початку, потім за timestamp
   */
  private sortHistory(history: PromptHistoryItem[]): PromptHistoryItem[] {
    const pinned = history.filter(item => item.isPinned)
      .sort((a, b) => b.timestamp - a.timestamp);
    const unpinned = history.filter(item => !item.isPinned)
      .sort((a, b) => b.timestamp - a.timestamp);
    return [...pinned, ...unpinned];
  }

  /**
   * Генерувати унікальний ID
   */
  private generateId(): string {
    return `${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }
}
