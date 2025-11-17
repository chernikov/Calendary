import { Order } from '../order';

export class OrderResult {
  public orders: Order[] = [];
  public total: number = 0;
  public page: number = 0;
  public pageSize: number = 0;
}
