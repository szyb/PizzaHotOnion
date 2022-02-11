using PizzaHotOnion.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaHotOnion.Repositories
{
  public interface IChatMessageRepository : ICrudRepository<ChatMessage>
  {
    Task<IEnumerable<ChatMessage>> GetAll(string room, DateTime orderDay);
  }
}