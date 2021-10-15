import { DecimalPipe } from "@angular/common";

export class OrderItem {
  public id: string;
  public day: Date;
  public quantity: number;
  public price: number;
  public who: string;
  public room: string;
  public isApproved: boolean;
}