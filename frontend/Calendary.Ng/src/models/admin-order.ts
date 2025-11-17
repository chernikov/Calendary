import { OrderItem } from "./order-item";
import { UserInfo } from "./user";

export class AdminOrder {
    id: number = 0;
    orderDate: Date = new Date();
    status: string = '';
    isPaid: boolean = false;
    comment : string | null = null;
    items: OrderItem[] = [];
    delieveryAddress: string = '';
    user : UserInfo = new UserInfo();
  }