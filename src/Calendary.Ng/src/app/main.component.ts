import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './components/header/header.component';
import { BlockUiService } from '../services/block.ui.service';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-main',
  standalone: true,
  imports: [CommonModule, RouterOutlet, HeaderComponent],
  templateUrl: './main.component.html',
  styleUrl: './main.component.scss'
})
export class MainComponent {

  isBlocked$: Observable<boolean>; 

  constructor(private blockUiService : BlockUiService) {
    this.isBlocked$ = this.blockUiService.isBlocked$;
  } 
 
  
}
