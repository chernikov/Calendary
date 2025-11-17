import { OrderItem } from "./order-item";

export class Order {
    id: number = 0;
    orderDate: Date = new Date();
    status: string = '';
    isPaid: boolean = false;
    comment : string | null = null;
    items: OrderItem[] = [];
  }