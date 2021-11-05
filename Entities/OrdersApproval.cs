using System;

namespace PizzaHotOnion.Entities
{
  public class OrdersApproval : Entity
  {
    public static int DefaultSlicesPerPizza = 8;
    public OrdersApproval(Guid id) : base(id) { }

    public Room Room { get; set; }
    public DateTime Day { get; set; }
    public int PizzaQuantity { get; set; }
    public string Who { get; set; }
    public decimal PricePerPizza { get; set; }
    public decimal SlicesPerPizza { get; set; }
    public bool Arrived { get; internal set; }
  }
}
