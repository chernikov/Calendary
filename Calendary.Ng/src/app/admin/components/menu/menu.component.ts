import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [RouterModule,MatSidenavModule],
  templateUrl : "./menu.component.html",
  styleUrl: "./menu.component.scss"
})
export class MenuComponent {}