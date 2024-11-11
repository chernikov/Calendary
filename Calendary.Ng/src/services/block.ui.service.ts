import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BlockUiService {
  private _isBlocked = new BehaviorSubject<boolean>(false);
  isBlocked$ = this._isBlocked.asObservable();

  show() {
    setTimeout(() => {
      this._isBlocked.next(true);
    });
  }

  hide() {
    setTimeout(() => {
      this._isBlocked.next(false);
    });
  }
}