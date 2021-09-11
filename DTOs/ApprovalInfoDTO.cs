namespace PizzaHotOnion.Controllers
{
  internal class ApprovalInfoDTO
  {
    public string Approver { get; set; }
    public decimal PricePerSlice { get; set; }
    public bool OrderArrived { get; set; }
  }
}