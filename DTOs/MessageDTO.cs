using System;

namespace PizzaHotOnion.DTOs
{
  public class MessageDTO
  {
    public OperationType Operation { get; set; }

    public string Context { get; set; }
  }

  public enum OperationType {
    RoomCreated = 1,
    RoomDeleted = 2,
    SliceGrabbed = 3,
    SliceCancelled = 4,
    OrdersApproved = 5,
    PriceIsSet = 6,
    OrderArrived = 7,
    ApprovalIsCancelled = 8,
    NewChatMessage = 9
  }
}
