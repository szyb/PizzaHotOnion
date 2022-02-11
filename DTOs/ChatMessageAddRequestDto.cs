namespace PizzaHotOnion.DTOs
{
  public class ChatMessageAddRequestDto
  {
    public string Room { get; set; }
    public string Message { get; set; }
    public string Who { get; set; }
  }
}