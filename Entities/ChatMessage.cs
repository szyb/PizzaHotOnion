using System;

namespace PizzaHotOnion.Entities
{
  public class ChatMessage : Entity
  {
    
    public DateTime Day { get; set; }
    public string Message { get; set; }
    public User Who { get; set; }
    public Room Room { get; set; }
    public DateTime Created { get; set; }
    public ChatMessage(Guid id) : base(id)
    {
    }
  }
}
