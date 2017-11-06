import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Room } from './room.model';
import { RoomService } from './rooms.service';

@Component({
  selector: 'app-root',
  providers: [
    RoomService
  ],
  templateUrl: './rooms.component.html'
})
export class RoomsComponent implements OnInit {
  
  public room: Room;
  public rooms: Room[] = [];
  public selectedRoom: Room = null;

  constructor(
    public router: Router,
    private roomService: RoomService) {
      this.room = new Room();
  }

  public ngOnInit(): void {
    this.loadRooms();
  }

  private loadRooms(): void {
    this.roomService.getRooms()
      .subscribe(rooms => this.rooms = rooms);
  }

  public addRoom(): void {
    this.roomService.addRoom(this.room)
      .subscribe(result => {
        if(result) {
          this.room.name = '';
          this.loadRooms();
        }
      });
  }

  public selectRoom(room: Room): boolean {
    this.selectedRoom = room;
    return false;
  }

  public removeRoom(): boolean {
    if(!this.selectedRoom)
      return false;

    this.roomService.removeRoom(this.selectedRoom.name)
    .subscribe(result => {
      if(result) {
        this.selectedRoom = null;
        this.loadRooms();
      }
    });
    return false;
  }
}
