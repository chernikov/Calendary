import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { AdminTestPromptService } from '../../../../services/admin/test-prompt.service';
import { AdminTestPrompt } from '../../../../models/admin-test-prompt';

@Component({
  selector: 'app-prompt-history',
  standalone: true,
  imports: [CommonModule, MatButtonModule, RouterModule],
  templateUrl: './prompt-history.component.html',
  styleUrl: './prompt-history.component.scss'
})
export class PromptHistoryComponent  implements OnInit {
    testPrompts: AdminTestPrompt[] = [];
    promptId: number = 0;
  
    constructor(
      private route: ActivatedRoute,
      private testPromptService: AdminTestPromptService
    ) {}
  
    ngOnInit(): void {
      // Отримуємо ID промпту з URL
      this.promptId = Number(this.route.snapshot.paramMap.get('id'));
      this.loadHistory();
    }
  
    loadHistory(): void {
      this.testPromptService.getByPromptId(this.promptId).subscribe(
        (testPrompts) => {
          this.testPrompts = testPrompts;
        },
        (error) => {
          console.error('Помилка завантаження історії:', error);
        }
      );
    }
  }