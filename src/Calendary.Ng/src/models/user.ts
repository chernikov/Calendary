export class User {
  id: number = 0;
  email: string = '';
  token: string = '';
}

export class UserLogin {
  email: string = '';
  password: string = '';
}

export class UserRegister {
  email: string = '';
  password: string = '';
  confirmPassword: string = '';
}

export class UserInfo {
  userName : string = '';
  email : string = '';
  phoneNumber : string = '';
  isEmailConfirmed : boolean = false;
  isPhoneNumberConfirmed : boolean = false;
}