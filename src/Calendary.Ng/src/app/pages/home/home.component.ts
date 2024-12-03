import { Component } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { HeaderComponent } from '../../components/header/header.component';
import { TempUserService } from '../../../services/temp-user.service';
import { TokenService } from '../../../services/token.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  constructor(private router: Router, 
    private tempUserService: TempUserService,
    private tokenService: TokenService,
  ) { }

  goToCreateCalendar() {
    this.tempUserService.init().subscribe({
      next: (response) => {
        if (response) {
          this.tokenService.saveToken(response!.token);
        }
        this.router.navigate(['/master']).then(() => {
          window.location.reload();
        });
      },
      error: (error) => {
        console.error('Error while initializing user', error);
      }
    });
  }
}

