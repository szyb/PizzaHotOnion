import { DecimalPipe } from "@angular/common";

export class ChatMessageItem {
  public who: string;
  public room: string;
  public message: string;
  public date: Date;
}

export class ChatMessageAddRequest {
  public room: string;
  public who: string;
  public message: string;
}
