using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaHotOnion.DTOs
{
  public class SetPriceDTO
  {
    public string Room { get; set; }
    public string Who { get; set; }
    public decimal PricePerPizza { get; set; }
    public decimal SlicesPerPizza { get; set; }    
  }
}
