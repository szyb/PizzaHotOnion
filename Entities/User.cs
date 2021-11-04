using System;

namespace PizzaHotOnion.Entities
{
  public class User : Entity
  {
    public User(Guid id) : base(id) { }
    public string Email { get; set; }
    public string Passwd { get; set; }
    public bool EmailNotification { get; set; }
    public string ApproversMessage { get; set; }
    public string ResetCode { get; set; }
    public DateTime? ResetCodeValidTo { get; set; }
  }
}
