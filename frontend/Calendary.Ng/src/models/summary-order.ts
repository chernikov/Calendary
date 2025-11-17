import { Calendar } from './calendar';
import { UserInfo } from './user';

export class SummaryOrder {
  id: number = 0;
  sum: number = 0;
  isPaid: boolean = false;
  deliveryAddress: string = '';
  deliveryCost: number = 0;
  orderItems: OrderItem[] = [];
  user: UserInfo = new UserInfo();
}

export class OrderItem {
  public id: number = 0;
  public quantity: number = 0;
  public price: number = 0;
  public calendar: Calendar = new Calendar();
}
