import { Component, OnInit, ViewChild, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';
import { RoomService } from '../rooms/rooms.service';
import { Room } from '../rooms/room.model';
import { Order } from './order.model';
import { AuthenticationService } from '../shared/auth/authentication.service';
import { OrderItem } from './order-item.model';
import { OrdersService } from './orders.service';
import { BaseChartDirective } from 'ng2-charts';
import { OrdersApproval } from './orders-approval.model';
import { ErrorHelper } from '../shared/error-helper';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';
import { Message, OperationType } from '../shared/message.model';
import { Config } from '../shared/config';

@Component({
  selector: 'app-root',
  providers: [
    RoomService,
    OrdersService,
    AuthenticationService
  ],
  templateUrl: './orders.component.html'
})
export class OrdersComponent implements OnInit {

  public rooms: Room[] = [];
  public order: Order;
  public orderItems: OrderItem[] = [];
  public selectedRoomName: string;
  public slices: number = 0;
  public pizzas: number = 0;
  public slicesToGet: number = 0;
  public isApproved: boolean = false;
  public isApprover: boolean = false;
  public pricePerSlice: number;
  public orderPrice: string = null;
  public myQuantity: number = 0;
  public isOrderArrived: boolean = false;



  @ViewChild(BaseChartDirective) chart: BaseChartDirective;

  // Pie
  public pieChartLabels: string[] = [];
  public pieChartData: number[] = [];
  //public pieChartColours: any[] = [{ backgroundColor: ["#FFA1B5", "#7B68EE", "#87CEFA", "#B22222", "#FFE29A", "#D2B48C", "#90EE90", "#FF69B4", "#EE82EE", "#6A5ACD", "#b8436d", "#9ACD32", "#00d9f9", "#800080", "#FF6347", "#DDA0DD", "#a4c73c", "#a4add3", "#008000", "#DAA520", "#00BFFF", "#2F4F4F", "#FF8C00", "#A9A9A9", "#FFB6C1", "#00FFFF", "#6495ED", "#7FFFD4", "#F0F8FF", "#7FFF00", "#008B8B", "#9932CC", "#E9967A", "#8FBC8F", "#483D8B", "#D3D3D3", "#ADD8E6"] }];
  //public pieChartColours: any[] = [{ backgroundColor: ["#FF0000", "#FF6A00", "#FFD800", "#B6FF00", "#4CFF00", "#00FF21", "#00FF90", "#00FFFF", "#0094FF", "#0026FF", "#4800FF", "#B200FF", "#FF00DC", "#7F0000", "#7F3300", "#7F6A00", "#5B7F00", "#267F00", "#007F0E", "#007F46", "#007F7F", "#004A7F", "#00137F", "#21007F", "#57007F", "#7F006E", "#7F0037", "#DAFF7F", "#A5FF7F", "#7FFFFF", "#7FC9FF", "#7F92FF", "#A17FFF", "#D67FFF", "#FF7FB6", "#7F3F5B", "#DAFF7F"] }];
  //public pieChartColours: any[] = [{ backgroundColor: ["#A5FF7F", "#7F3F5B", "#A17FFF", "#7F6A00", "#21007F", "#7F0000", "#00FF21", "#DAFF7F", "#0094FF", "#B200FF", "#7F92FF", "#7F3300", "#007F46", "#00137F", "#FF6A00", "#00FF90", "#FF0000", "#7FFFFF", "#7FC9FF", "#004A7F", "#4CFF00", "#FF00DC", "#57007F", "#FFD800", "#DAFF7F", "#B6FF00", "#D67FFF", "#007F0E", "#00FFFF", "#267F00", "#0026FF", "#4800FF", "#5B7F00", "#FF7FB6", "#007F7F", "#7F006E", "#7F0037"] }];
  public pieChartColours: any[] = [{ backgroundColor: ["#007F0E", "#B200FF", "#7F0000", "#7F92FF", "#FFD800", "#FF6A00", "#7F0037", "#7FFFFF", "#DAFF7F", "#21007F", "#7FC9FF", "#5B7F00", "#4CFF00", "#B6FF00", "#57007F", "#00FFFF", "#7F3F5B", "#FF0000", "#FF00DC", "#FF7FB6", "#007F46", "#7F3300", "#007F7F", "#A5FF7F", "#D67FFF", "#00137F", "#0094FF", "#DAFF7F", "#7F006E", "#00FF90", "#7F6A00", "#A17FFF", "#004A7F", "#0026FF", "#267F00", "#00FF21", "#4800FF"] }];
  public pieChartType: string = 'pie';

  constructor(
    public router: Router,
    private roomService: RoomService,
    private ordersService: OrdersService,
    private authenticationService: AuthenticationService) {
    this.order = new Order();
    this.order.who = this.authenticationService.getLoggedUser();
  }

  public ngOnInit(): void {
    this.registerSignalR();
    this.loadRooms();
  }

  private registerSignalR() {
    const connection = new HubConnectionBuilder().withUrl(`${Config.baseUrl}message`).build();
    connection.on('send', data => {
      //console.log(data);
      const message: Message = <Message>data;
      if(message) {
        switch(message.operation) {
          case OperationType.RoomCreated:
          case OperationType.RoomDeleted:
            this.loadRooms();
          break;
          case OperationType.SliceGrabbed:
          case OperationType.SliceCancelled:
          case OperationType.OrdersApproved:
         
            if (message.context == this.selectedRoomName) {
              this.loadOrdersInRoom(this.selectedRoomName);
              this.playSound_drop();
            }
            break;
          case OperationType.PriceIsSet:            
            if (message.context == this.selectedRoomName)
              this.loadApprovalInfo(this.selectedRoomName);            
            break;
          case OperationType.OrderArrived:
            if (message.context == this.selectedRoomName) {              
              this.loadApprovalInfo(this.selectedRoomName);
              this.playSound_fanfare();
            }
            break;
          case OperationType.ApprovalIsCancelled:
            if (message.context == this.selectedRoomName) {
              console.log("Approval is cancelled");
              this.pricePerSlice = null;
              this.loadOrdersInRoom(this.selectedRoomName);
              this.playSound_drop();
            }
            break;
        }
      }
    });

    connection.start()
      .then(() => {
         //console.log('MessageHub Connected');
      });

  }

  private loadRooms(): void {
    this.roomService.getRooms()
      .subscribe(rooms => this.onLoadRooms(rooms));
  }

  private onLoadRooms(rooms: Room[]): void {
    this.rooms = rooms;
    if (rooms.length > 0) {
      if (this.selectedRoomName && this.rooms.some(r => r.name == this.selectedRoomName))
        this.selectRoom(this.selectedRoomName);
      else
        this.selectRoom(rooms[0].name);
    }
  }

  public selectRoom(roomName: string): boolean {
    this.rooms.forEach((r) => {
      r.isActive = r.name == roomName;
    });

    this.selectedRoomName = roomName;
    this.order.room = roomName;
    this.isApprover = false;
    this.pricePerSlice = null;
    this.orderPrice = null;
    this.isOrderArrived = false;

    this.loadOrdersInRoom(this.selectedRoomName);
    this.onPriceChange();

    return false;
  }

  private loadOrdersInRoom(roomName: string) {
    this.ordersService.getOrders(roomName)
      .subscribe(
      orderItems => this.onLoadOrderItems(orderItems),
      error => alert(ErrorHelper.getErrorMessage(error))
      );
  }

  private loadApprovalInfo(roomName: string) {
    //this.isApprover = false;
    //this.pricePerSlice = null;
    this.ordersService.getApprovalInfo(roomName)
      .subscribe(
        info => {
          if (info.approver == this.authenticationService.getLoggedUser())
            this.isApprover = true;
          this.pricePerSlice = info.pricePerSlice;
          if (info.orderArrived)
            this.isOrderArrived = true;
          this.onPriceChange();          
        },
        error => { });
  }

  private onLoadOrderItems(orderItems: OrderItem[]): void {
    this.orderItems = orderItems;
    this.slices = 0;
    this.pizzas = 0;
    this.setNumberOfSlices();
    this.checkIsApproved();
    this.preparePizzaChart();
    this.loadApprovalInfo(this.selectedRoomName);
  }

  private onPriceChange(): void {
    if (this.order != null && this.order.quantity > 0) {
      const tmpPrice = this.order.quantity * this.pricePerSlice;
      this.orderPrice = tmpPrice.toFixed(2);
    }
  }

  private checkIsApproved(): void {
    this.isApproved = this.orderItems.some(item => item.isApproved);
  }

  private setNumberOfSlices(): void {
    let currentUserEmail = this.authenticationService.getLoggedUser();

    this.orderItems.forEach((o) => {
      if (o.who == currentUserEmail) {
        this.order.quantity = o.quantity;
      }
    });
  }

  private preparePizzaChart(): void {
    let pieChartLabels: string[] = [];
    let pieChartData: number[] = [];

    this.orderItems.forEach((o) => {
      this.slices += o.quantity;
      pieChartLabels.push(o.who);
      pieChartData.push(o.quantity);
    });

    this.pizzas = Math.ceil(this.slices / 8);

    if (this.pizzas == 0)
      return;

    this.slicesToGet = (this.pizzas * 8) - this.slices;
    if (this.slicesToGet > 0) {
      pieChartLabels.push('FREE');
      pieChartData.push(this.slicesToGet);
    }

    this.pieChartLabels = pieChartLabels;
    this.pieChartData = pieChartData;

    setTimeout(() => {
      if (this.chart && this.chart.chart && this.chart.chart.config) {
        this.chart.chart.config.data.labels = this.pieChartLabels;
        //this.chart.chart.config.data.datasets = this.pieChartData;
        this.chart.chart.config.data.colors = this.pieChartColours;
        this.chart.chart.update();
      }
    });
  }

  public makeOrder(): boolean {    
    this.ordersService.makeOrder(this.order)
      .subscribe(result => {
        if (result)
          this.loadOrdersInRoom(this.selectedRoomName);

      },
        error => alert(ErrorHelper.getErrorMessage(error))
      );    
    return false;
  }

  public setPrice(): boolean {
    this.ordersService.setPrice(this.authenticationService.getLoggedUser(), this.order.room, this.pricePerSlice)
      .subscribe(
        result => {

      },
        error => alert(ErrorHelper.getErrorMessage(error))
      );
    return true;
  }

  public orderArrived(): boolean {
    this.ordersService.orderArrived(this.order.room)
      .subscribe(
        result => { },
        error => alert(ErrorHelper.getErrorMessage(error)));
    return true;
  }

  public cancel(): boolean {
    let orderId: string = this.getOrderId();

    this.ordersService.removeOrder(this.selectedRoomName, orderId)
      .subscribe(result => {
        if (result)
          this.loadOrdersInRoom(this.selectedRoomName);
      },
      error => alert(ErrorHelper.getErrorMessage(error))
      );
    return false;
  }

  private getOrderId(): string {
    let orderId: string;
    this.orderItems.forEach((o) => {
      if (o.who == this.authenticationService.getLoggedUser()) {
        orderId = o.id;
      }
    });
    return orderId;
  }

  public refresh(): boolean {
    this.loadOrdersInRoom(this.selectedRoomName);
    return false;
  }

  public approveOrders(): void {
    if (confirm("Are you sure you want to approve all orders? ")) {
      this.ordersService.approveOrders(this.selectedRoomName, this.authenticationService.getLoggedUser())
        .subscribe(
          result => {
            this.refresh();
            this.isApprover = true;
          },
          error => alert(ErrorHelper.getErrorMessage(error))
        );
    }
  }

  public cancelApproval(): void {
    if (confirm("Are you sure you want to cancel approval?")) {
      this.ordersService.cancelApproval(this.selectedRoomName, this.authenticationService.getLoggedUser())
        .subscribe(
          result => {
            this.refresh();
            this.isApprover = false;
            this.isApproved = false;
          },
          error => alert(ErrorHelper.getErrorMessage(error))
        );
    }
  }

  playSound_fanfare() {
    const audio = new Audio("assets/Pizza_fanfare.mp3");
    audio.play();
  }

  playSound_drop() {
    const audio = new Audio("assets/water_drop.wav");
    audio.play();
  }

  // events
  public chartClicked(e: any): void {
    //console.log(e);
  }

  public chartHovered(e: any): void {
    //console.log(e);
  }
}
