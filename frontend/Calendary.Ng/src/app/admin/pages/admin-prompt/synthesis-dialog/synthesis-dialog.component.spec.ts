import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { of } from 'rxjs';
import { AdminFluxModelService } from '../../../../../services/admin/flux-model.service';
import { AdminPromptService } from '../../../../../services/admin/prompt.service';
import { AdminSynthesisService } from '../../../../../services/admin/synthesis.service';
import { Prompt } from '../../../../../models/prompt';
import { PromptSeed } from '../../../../../models/promt-seed';
import { SynthesisDialogComponent } from './synthesis-dialog.component';

describe('SynthesisDialogComponent', () => {
  let component: SynthesisDialogComponent;
  let fixture: ComponentFixture<SynthesisDialogComponent>;

  const mockPrompt = new Prompt();
  mockPrompt.id = 1;
  mockPrompt.themeId = 1;
  mockPrompt.categoryId = 2;
  mockPrompt.text = 'Initial prompt';
  mockPrompt.themeName = 'Theme';
  const promptSeed = new PromptSeed();
  promptSeed.id = 1;
  promptSeed.promptId = 1;
  promptSeed.seed = 42;
  mockPrompt.seeds = [promptSeed];

  const fluxModelServiceSpy = jasmine.createSpyObj('AdminFluxModelService', ['getByCategoryId']);
  const synthesisServiceSpy = jasmine.createSpyObj('AdminSynthesisService', ['create', 'runSynthesis']);
  const promptServiceSpy = jasmine.createSpyObj('AdminPromptService', ['update', 'assignSeed', 'getById']);

  beforeEach(async () => {
    fluxModelServiceSpy.getByCategoryId.and.returnValue(of([]));
    synthesisServiceSpy.create.and.returnValue(of({ id: 1 }));
    synthesisServiceSpy.runSynthesis.and.returnValue(of({ imageUrl: 'test', outputSeed: 1 }));
    promptServiceSpy.update.and.returnValue(of(null));
    promptServiceSpy.assignSeed.and.returnValue(of(null));
    promptServiceSpy.getById.and.returnValue(of(mockPrompt));

    await TestBed.configureTestingModule({
      imports: [SynthesisDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: { prompt: mockPrompt } },
        { provide: AdminFluxModelService, useValue: fluxModelServiceSpy },
        { provide: AdminSynthesisService, useValue: synthesisServiceSpy },
        { provide: AdminPromptService, useValue: promptServiceSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SynthesisDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
