import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { MonthAssignment } from '../models/calendar-assignment.model';

@Injectable({
  providedIn: 'root',
})
export class CalendarBuilderService {
  private readonly storageKey = 'calendar-assignments';
  private readonly assignmentsSubject = new BehaviorSubject<MonthAssignment[]>(this.loadFromStorage());
  readonly assignments$ = this.assignmentsSubject.asObservable();

  assignImageToMonth(month: number, imageId: string, imageUrl: string): void {
    if (!month || month < 1 || month > 12) {
      return;
    }

    const normalizedId = imageId.toString();
    const current = this.assignmentsSubject.value.filter((assignment) => assignment.month !== month);
    const updated: MonthAssignment = { month, imageId: normalizedId, imageUrl };

    this.setAssignments([...current, updated]);
  }

  removeAssignment(month: number): void {
    this.setAssignments(this.assignmentsSubject.value.filter((assignment) => assignment.month !== month));
  }

  removeAssignmentsByImageId(imageId: string): void {
    const normalizedId = imageId.toString();
    this.setAssignments(this.assignmentsSubject.value.filter((assignment) => assignment.imageId !== normalizedId));
  }

  clear(): void {
    this.setAssignments([]);
  }

  getAssignment(month: number): MonthAssignment | undefined {
    return this.assignmentsSubject.value.find((assignment) => assignment.month === month);
  }

  getAssignmentByImageId(imageId: string): MonthAssignment | undefined {
    const normalizedId = imageId.toString();
    return this.assignmentsSubject.value.find((assignment) => assignment.imageId === normalizedId);
  }

  isComplete(): boolean {
    return this.assignmentsSubject.value.length === 12;
  }

  getDuplicateImageIds(): string[] {
    const occurrences = this.assignmentsSubject.value.reduce<Map<string, number>>((map, assignment) => {
      const key = assignment.imageId;
      map.set(key, (map.get(key) || 0) + 1);
      return map;
    }, new Map<string, number>());

    return Array.from(occurrences.entries())
      .filter(([, count]) => count > 1)
      .map(([id]) => id);
  }

  private loadFromStorage(): MonthAssignment[] {
    if (typeof localStorage === 'undefined') {
      return [];
    }

    try {
      const saved = localStorage.getItem(this.storageKey);
      return saved ? (JSON.parse(saved) as MonthAssignment[]) : [];
    } catch {
      return [];
    }
  }

  private persist(assignments: MonthAssignment[]): void {
    if (typeof localStorage === 'undefined') {
      return;
    }

    try {
      localStorage.setItem(this.storageKey, JSON.stringify(assignments));
    } catch {
      // Swallow storage errors silently to avoid breaking UX.
    }
  }

  private setAssignments(assignments: MonthAssignment[]): void {
    this.assignmentsSubject.next(assignments);
    this.persist(assignments);
  }
}
