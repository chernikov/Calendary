import { Component } from '@angular/core';
import { SettingService } from '../../../services/setting.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent {

  constructor(private settingService: SettingService) {

  }

  onClick() {
    this.settingService.error().subscribe({
      next : (data) => {
        console.log("Error");
      }
    });
  }
}
