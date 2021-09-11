using System;

namespace PizzaHotOnion.Entities
{
  public class OrdersApproval : Entity
  {
    public OrdersApproval(Guid id) : base(id) { }

    public Room Room { get; set; }
    public DateTime Day { get; set; }
    public int PizzaQuantity { get; set; }
    public string Who { get; set; }
    public decimal PricePerSlice { get; set; }
    public bool Arrived { get; internal set; }
  }
}