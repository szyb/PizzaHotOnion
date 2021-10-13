export class Order {
  public quantity: number;
  public who: string;
  public room: string;
}

export class ApproveOrders {
  public room: string;
  public approver: string;
}

export class SetPrice {
  public who: string;
  public room: string;
  public pricePerPizza: number;
  public slicesPerPizza: number;
}

export class ApprovalInfo {
  public approver: string;
  public pricePerPizza: number;
  public slicesPerPizza: number;
  public orderArrived: boolean;
}
