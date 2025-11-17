import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { AdminSynthesisService } from '../../../../services/admin/synthesis.service';
import { AdminSynthesis } from '../../../../models/admin-synthesis';

@Component({
    standalone: true,
    selector: 'app-prompt-history',
    imports: [CommonModule, MatButtonModule, RouterModule],
    templateUrl: './prompt-history.component.html',
    styleUrl: './prompt-history.component.scss'
})
export class PromptHistoryComponent  implements OnInit {
    synthesises: AdminSynthesis[] = [];
    promptId: number = 0;
  
    constructor(
      private route: ActivatedRoute,
      private synthesisService: AdminSynthesisService
    ) {}
  
    ngOnInit(): void {
      // Отримуємо ID промпту з URL
      this.promptId = Number(this.route.snapshot.paramMap.get('id'));
      this.loadHistory();
    }
  
    loadHistory(): void {
      this.synthesisService.getByPromptId(this.promptId).subscribe(
        (synthesises) => {
          this.synthesises = synthesises;
        },
        (error) => {
          console.error('Помилка завантаження історії:', error);
        }
      );
    }
  }
