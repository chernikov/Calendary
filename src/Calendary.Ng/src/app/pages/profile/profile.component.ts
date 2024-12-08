import { Component } from '@angular/core';
import { SettingService } from '../../../services/setting.service';
import { SettingsComponent } from '../../components/settings/settings.component';
import { ProfileOrdersComponent } from '../../components/profile-orders/profile-orders.component';
import { ChangePasswordComponent } from '../../components/change-password/change-password.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [SettingsComponent, ProfileOrdersComponent, ChangePasswordComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent {

  constructor(private settingService: SettingService) {

  }
}
