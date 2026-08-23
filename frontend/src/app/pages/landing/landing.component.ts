import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink],
  template: `
    <nav class="nav">
      <span class="nav-brand">Calendary</span>
      <a routerLink="/start">Почати</a>
    </nav>

    <div class="page">
      <h1 style="font-size: 48px; max-width: 640px;">Календар на дванадцять місяців — з вашими образами</h1>
      <p class="text-muted" style="font-size: 17px; max-width: 520px; margin-bottom: var(--space-6);">
        Одне фото, обраний напрямок і особисті дати — решту робить генерація. Друк A3, тверда обкладинка,
        доставка Новою поштою.
      </p>
      <a class="btn btn-primary" style="min-height: 50px; font-size: 15px; padding-inline: 28px;" routerLink="/start">
        Почати замовлення
      </a>

      <div class="hr"></div>

      <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: var(--space-4);">
        <div>
          <div class="d" style="font-family: var(--font-heading); font-weight: 600; font-size: 20px; margin-bottom: 8px;">Одне фото</div>
          <p class="card-body">Завантажте одне фото — воно стане основою для образів усіх дванадцяти місяців.</p>
        </div>
        <div>
          <div class="d" style="font-family: var(--font-heading); font-weight: 600; font-size: 20px; margin-bottom: 8px;">Образи й дати</div>
          <p class="card-body">Оберіть напрямок — історія, кіно, пригоди чи професії — і додайте особисті дати.</p>
        </div>
        <div>
          <div class="d" style="font-family: var(--font-heading); font-weight: 600; font-size: 20px; margin-bottom: 8px;">Друк і доставка</div>
          <p class="card-body">Друкуємо на A3 з твердою обкладинкою і надсилаємо Новою поштою у твердому тубусі.</p>
        </div>
      </div>
    </div>
  `,
})
export class LandingComponent {}
