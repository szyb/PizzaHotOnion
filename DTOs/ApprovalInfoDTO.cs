namespace PizzaHotOnion.Controllers
{
  internal class ApprovalInfoDTO
  {
    public string Approver { get; set; }
    public decimal PricePerPizza { get; set; }
    public decimal SlicesPerPizza { get; set; }
    public bool OrderArrived { get; set; }
  }
}