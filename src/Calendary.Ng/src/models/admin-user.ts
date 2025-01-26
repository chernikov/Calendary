export abstract class AdminBaseUser {
    userName: string = '';
    email: string = '';
    phoneNumber: string = '';
  }
  
  export class AdminUser extends AdminBaseUser {
    id: number = 0;
    identity: string = ''; 
    created: Date = new Date();
  }
  
  export class AdminCreateUser extends AdminBaseUser {}