using System;

namespace PizzaHotOnion.Services
{
  public class ChatMessageDto
  {
    public string Who { get; set; }
    public bool IsApprover { get; set; }
    public string Message { get; set; }
    public DateTime Date { get; set; }
  }
}