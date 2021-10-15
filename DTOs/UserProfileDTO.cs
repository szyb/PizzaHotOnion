using System;

namespace PizzaHotOnion.DTOs
{
  public class UserProfileDTO
  {
    public string Email { get; set; }

    public bool EmailNotification { get; set; }

    public string ApproversMessage { get; set; }
  }
}