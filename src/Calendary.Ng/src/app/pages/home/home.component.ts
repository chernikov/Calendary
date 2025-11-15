import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
    standalone: true,
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrl: './home.component.scss'
})
export class HomeComponent {
  constructor(private router: Router) { }

  goToMaster() {
    this.router.navigate(['/master']).then(() => {
      window.location.reload();
    });
  }
}

