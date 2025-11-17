import { Calendar } from "./calendar";

export class OrderItem {
    id: number = 0;

    calendar: Calendar | null = null;

    quantity : number = 0;

    price : number = 0;
  }