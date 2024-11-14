import { Calendar } from "./calendar";
import { UserInfo } from "./user";

export class SummaryOrder {
    orderItems: OrderItem[] = [];
    sum: number = 0;
    deliveryAddress: string = '';
    user: UserInfo = new UserInfo();

}

export class OrderItem {
    public id: number = 0;
    public quantity: number= 0; 
    public price: number= 0; 
    public calendar: Calendar = new Calendar();
}
