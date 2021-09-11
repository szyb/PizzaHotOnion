import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { Observable } from "rxjs/Observable";
import "rxjs/add/operator/do";
import "rxjs/add/operator/map";
import { Config } from "../shared/config";
import { OrderItem } from "./order-item.model";
import { ApprovalInfo, ApproveOrders, Order, SetPrice } from "./order.model";
import { OrdersApproval } from "./orders-approval.model";
import { SERVER_TRANSITION_PROVIDERS } from "@angular/platform-browser/src/browser/server-transition";

@Injectable()
export class OrdersService {

  constructor(private http: HttpClient) { }

  public getOrders(room: string): Observable<OrderItem[]> {
    return this.http.get<OrderItem[]>(
      `${Config.apiUrl}orders/${room}`);
  }

  public makeOrder(order: Order): Observable<boolean> {
    let body = JSON.stringify(order);

    let room = order.room;

    return this.http.post(
      `${Config.apiUrl}orders/${room}`, body, { observe: 'response' }
    ).map(response => response.status == 201);
  }

  public removeOrder(room: string, id: string):  Observable<boolean> {
    return this.http.delete(
      `${Config.apiUrl}orders/${room}/${id}`, { observe: 'response' }
    ).map(response => response.status == 204);
  }

  public approveOrders(room: string, approver: string): Observable<boolean> {
    let approveOrders = new ApproveOrders();
    approveOrders.room = room;
    approveOrders.approver = approver;

    let body = JSON.stringify(approveOrders);

    return this.http.post(
      `${Config.apiUrl}orders/${room}/approve`, body, { observe: 'response' }
    ).map(response => response.status == 201);
  }

  public setPrice(who: string, room: string, price: number): Observable<boolean> {
    let setPrice = new SetPrice();
    setPrice.who = who;
    setPrice.room = room;
    setPrice.pricePerSlice = price;

    let body = JSON.stringify(setPrice);

    return this.http.post(
      `${Config.apiUrl}orders/${room}/setPrice`, body, { observe: 'response' }
    ).map(response => response.status == 201);
  }

  public getApprovalInfo(room: string): Observable<ApprovalInfo> {
    return this.http.get<ApprovalInfo>(
      `${Config.apiUrl}orders/${room}/approvalInfo`);
  }

  public orderArrived(room: string): Observable<boolean> {
    return this.http.post(
      `${Config.apiUrl}orders/${room}/orderArrived`, null, { observe: 'response' }
    ).map(response => response.status == 201);
    
  }

}
