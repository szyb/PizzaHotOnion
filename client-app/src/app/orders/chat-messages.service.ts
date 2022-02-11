import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { Observable } from "rxjs/Observable";
import "rxjs/add/operator/do";
import "rxjs/add/operator/map";
import { Config } from "../shared/config";
import { SERVER_TRANSITION_PROVIDERS } from "@angular/platform-browser/src/browser/server-transition";
import { ChatMessageAddRequest, ChatMessageItem } from "./chat-message-item.model";
import { Console } from "@angular/core/src/console";

@Injectable()
export class ChatMessagesService {

  constructor(private http: HttpClient) { }

  public getAllMessages(room: string): Observable<ChatMessageItem[]> {
    console.log("before get all messages");
    return this.http.get<ChatMessageItem[]>(
      `${Config.apiUrl}chatMessages/${room}`);
  }

  public addMessage(request: ChatMessageAddRequest): Observable<boolean> {
    let body = JSON.stringify(request);

    let room = request.room;
    console.log("before add");
    return this.http.post(
      `${Config.apiUrl}chatMessages/${room}`, body, { observe: 'response' }
    ).map(response => response.status == 204);
  }
}
