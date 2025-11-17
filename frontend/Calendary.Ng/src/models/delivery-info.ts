import { NovaPostItem } from "./nova-post-item";

export class DeliveryInfo {
    city : NovaPostItem = new NovaPostItem();
    postOffice : NovaPostItem = new NovaPostItem();
    deliveryCost: number = 0;
}