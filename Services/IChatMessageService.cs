using PizzaHotOnion.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaHotOnion.Services
{
  public interface IChatMessageService
  {
    Task<IEnumerable<ChatMessageDto>> GetAll(string room, DateTime day);
    Task<bool> AddMessage(string room, ChatMessageAddRequestDto message);
  }
}