import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AdminCreateUser } from '../../../../../models/admin-user';
import { CreateUserDialogComponent } from './create-user-dialog.component';

describe('CreateUserDialogComponent', () => {
  let component: CreateUserDialogComponent;
  let fixture: ComponentFixture<CreateUserDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateUserDialogComponent],
      providers: [
        {
          provide: MAT_DIALOG_DATA,
          useValue: { user: new AdminCreateUser() },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CreateUserDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
