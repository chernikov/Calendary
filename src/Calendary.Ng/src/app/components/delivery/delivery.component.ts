import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { Component, EventEmitter, Output, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { debounceTime, filter, switchMap, tap } from 'rxjs';
import { VerificationDialogComponent } from '../verification-dialog/verification-dialog.component';
import { UserService } from '../../../services/user.service';
import { VerificationService } from '../../../services/verification.service';
import { NovaPostService } from '../../../services/novapost.service';
import { UserInfo } from '../../../models/user';
import { NovaPostItem } from '../../../models/nova-post-item';
import { CartService } from '../../../services/cart.service';
import { DeliveryInfo } from '../../../models/delivery-info';

@Component({
    selector: 'app-delivery',
    imports: [CommonModule, FormsModule, ReactiveFormsModule],
    templateUrl: './delivery.component.html',
    styleUrls: ['./delivery.component.scss']
})
export class DeliveryComponent implements OnInit {
  deliveryForm: FormGroup;
  isEmailValid = false;
  isPhoneValid = false;
  isEmailConfirmed = false;
  isPhoneConfirmed = false;
  errorMessage: string = '';
  cityOptions: NovaPostItem[] = [];
  postOfficeOptions: NovaPostItem[] = [];
  selectedCity: string = '';
  lastSelectedCity: string = ''; // Змінна для порівняння
  lastSelectedPostOffice: string = ''; // Змінна для порівняння
  highlightedCityIndex: number = -1; // Індекс для навігації по містах
  highlightedPostOfficeIndex: number = -1; // Індекс для навігації по відділеннях

  deliveryInfo: DeliveryInfo = new DeliveryInfo();

  constructor(
    private formBuilder: FormBuilder,
    private userService: UserService,
    private VerificationService: VerificationService,
    private novaPostService: NovaPostService,
    private cartService: CartService,
    public dialog: MatDialog
  ) {
    this.deliveryForm = this.formBuilder.group({
      city: ['', Validators.required],
      postOffice: ['', Validators.required],
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: [
        '',
        [
          Validators.required,
          Validators.pattern(/^(?:\+380|380|0)\s?(\d\s?){9}$/),
        ],
      ],
    });

   
  }

  ngOnInit(): void {
    this.initUserInfo();
    this.initDeliveryInfo();
  }

  private setupAutocomplete() {
    // Автокомпліт для міста
    this.deliveryForm.controls['city'].valueChanges
      .pipe(
        debounceTime(300),
        filter((query) => query !== this.lastSelectedCity), // Фільтруємо, якщо значення не змінилось
        tap(() => {
          this.clearPostOffice(); // Очищуємо відділення при зміні міста
          this.highlightedCityIndex = -1; // Скидаємо індекс навігації
        }),
        switchMap((query) => this.novaPostService.searchCity(query))
      )
      .subscribe((options) => (this.cityOptions = options));

    // Автокомпліт для відділення, залежить від вибраного міста
    this.deliveryForm.controls['postOffice'].valueChanges
      .pipe(
        debounceTime(300),
        filter((query) => query !== this.lastSelectedPostOffice), // Фільтруємо, якщо значення не змінилось
        switchMap((query) =>
          this.novaPostService.searchWarehouse(this.selectedCity, query)
        )
      )
      .subscribe((options) => (this.postOfficeOptions = options));
  }

  selectCity(city: NovaPostItem) {
    this.selectedCity = city.description;
    this.deliveryForm.controls['city'].setValue(city.description);
    this.cityOptions = [];
    this.lastSelectedCity = city.description; // Зберігаємо останнє обране значення
    this.deliveryForm.controls['postOffice'].setValue('');
    this.loadPostOfficesForCity(city.description); // Завантажуємо відділення для вибраного міста
    this.highlightedCityIndex = -1; // Скидаємо індекс навігації
    this.deliveryInfo.city.description = city.description;
    this.deliveryInfo.city.ref = city.ref;
  }

  selectPostOffice(postOffice: NovaPostItem) {
    this.deliveryForm.controls['postOffice'].setValue(postOffice.description);
    this.postOfficeOptions = [];
    this.lastSelectedPostOffice = postOffice.description; // Зберігаємо останнє обране значення
    this.highlightedPostOfficeIndex = -1; // Скидаємо індекс навігації
    this.deliveryInfo.postOffice.description = postOffice.description;
    this.deliveryInfo.postOffice.ref = postOffice.ref;
    this.updateDeliveryInfo();
  }

  private clearPostOffice() {
    this.lastSelectedPostOffice = ''; // Скидаємо останнє вибране значення
    this.deliveryForm.controls['postOffice'].setValue(''); // Очищуємо значення
    this.postOfficeOptions = []; // Очищуємо опції
  }

  private loadPostOfficesForCity(cityName: string) {
    this.novaPostService
      .searchWarehouse(cityName, '') // Порожній параметр пошуку для отримання всіх відділень у місті
      .subscribe((options) => (this.postOfficeOptions = options));
  }

  // Обробник натискань на клавіші для навігації по списку міст
  onCityKeyDown(event: KeyboardEvent) {
    if (this.cityOptions.length) {
      if (event.key === 'ArrowDown') {
        this.highlightedCityIndex =
          (this.highlightedCityIndex + 1) % this.cityOptions.length;
        event.preventDefault();
      } else if (event.key === 'ArrowUp') {
        this.highlightedCityIndex =
          (this.highlightedCityIndex - 1 + this.cityOptions.length) %
          this.cityOptions.length;
        event.preventDefault();
      } else if (event.key === 'Enter' && this.highlightedCityIndex >= 0) {
        this.selectCity(this.cityOptions[this.highlightedCityIndex]);
        event.preventDefault();
      }
    }
  }

  // Обробник натискань на клавіші для навігації по списку відділень
  onPostOfficeKeyDown(event: KeyboardEvent) {
    if (this.postOfficeOptions.length) {
      if (event.key === 'ArrowDown') {
        this.highlightedPostOfficeIndex =
          (this.highlightedPostOfficeIndex + 1) % this.postOfficeOptions.length;
        event.preventDefault();
      } else if (event.key === 'ArrowUp') {
        this.highlightedPostOfficeIndex =
          (this.highlightedPostOfficeIndex -
            1 +
            this.postOfficeOptions.length) %
          this.postOfficeOptions.length;
        event.preventDefault();
      } else if (
        event.key === 'Enter' &&
        this.highlightedPostOfficeIndex >= 0
      ) {
        this.selectPostOffice(
          this.postOfficeOptions[this.highlightedPostOfficeIndex]
        );
        event.preventDefault();
      }
    }
  }

  initUserInfo() {
    this.userService.getInfo().subscribe(
      (data) => {
        if (data) {
          this.deliveryForm.patchValue({
            name: data.userName,
            email: data.email,
            phone: data.phoneNumber,
          });
          this.validateEmail();
          this.validatePhone();
          this.isEmailConfirmed = data.isEmailConfirmed;
          if (this.isEmailConfirmed) {
            this.deliveryForm.controls['email'].disable();
          }
          this.isPhoneConfirmed = data.isPhoneNumberConfirmed;
          if (this.isPhoneConfirmed) {
            this.deliveryForm.controls['phone'].disable();
          }
        }
      },
      (error) => {
        console.log(error);
      }
    );
  }

  initDeliveryInfo() {
    this.cartService.getDeliveryInfo().subscribe(
      (data) => {
        if (data) {
        this.deliveryInfo = data;
          this.deliveryForm.patchValue({
            city: data.city.description,
            postOffice: data.postOffice.description,
          });
        }
        this.setupAutocomplete();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  validateEmail() {
    this.updateInfo();
    this.isEmailValid = this.deliveryForm.controls['email'].valid;
  }

  validatePhone() {
    this.updateInfo();
    this.isPhoneValid = this.deliveryForm.controls['phone'].valid;
  }

  updateInfo() {
    const userInfo = new UserInfo();
    userInfo.userName = this.deliveryForm.controls['name'].value;
    userInfo.email = this.deliveryForm.controls['email'].value;
    userInfo.phoneNumber = this.deliveryForm.controls['phone'].value;

    this.errorMessage = '';
    this.userService.updateInfo(userInfo).subscribe(
      (data) => {},
      (error) => {
        this.errorMessage =
          error.error.message || 'Помилка при оновленні інформації';
      }
    );
  }

  updateDeliveryInfo() {
    this.errorMessage = '';
    this.cartService.updateDeliveryInfo(this.deliveryInfo).subscribe(
      (data) => {},
      (error) => {
        this.errorMessage =
          error.error.message || 'Помилка при оновленні інформації';
      }
    );
  }

  showModalEmailCode() {
    this.VerificationService.sendEmailVerification().subscribe(
      (data) => {
        console.log(data);
        this.modalVerifyEmailCode();
        // Логіка для відкриття модального вікна підтвердження email
      },
      (error) => {
        console.log(error);
      }
    );
  }

  modalVerifyEmailCode() {
    const dialogRef = this.dialog.open(VerificationDialogComponent, {
      width: '400px',
      panelClass: 'custom-dialog-container',
      data: { type: 'email' },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.initUserInfo();
      }
    });
  }

  showModalPhoneCode() {
    // Логіка для відкриття модального вікна підтвердження телефону
    this.VerificationService.sendPhoneVerification().subscribe(
      (data) => {
        console.log(data);
        this.modalVerifyPhoneCode();
        // Логіка для відкриття модального вікна підтвердження телефону
      },
      (error) => {
        console.log(error);
      }
    );
  }

  modalVerifyPhoneCode() {
    const dialogRef = this.dialog.open(VerificationDialogComponent, {
      width: '400px',
      panelClass: 'custom-dialog-container',
      data: { type: 'phone' },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.initUserInfo();
      }
    });
  }
}
