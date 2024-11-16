import { AdminOrder } from '../admin-order';
import { Order } from '../order';

export class AdminOrderResult {
  public orders: AdminOrder[] = [];
  public total: number = 0;
  public page: number = 0;
  public pageSize: number = 0;
}
