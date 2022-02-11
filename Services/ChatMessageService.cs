using PizzaHotOnion.DTOs;
using PizzaHotOnion.Entities;
using PizzaHotOnion.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaHotOnion.Services
{
  public class ChatMessageService : IChatMessageService
  {
    private readonly IChatMessageRepository chatMessageRepository;
    private readonly IRoomRepository roomRepository;
    private readonly IUserRepository userRepository;
    private readonly IOrdersApprovalRepository ordersApprovalRepository;

    public ChatMessageService(IChatMessageRepository chatMessageRepository, IRoomRepository roomRepository, IUserRepository userRepository, IOrdersApprovalRepository ordersApprovalRepository)
    {
      this.chatMessageRepository = chatMessageRepository;
      this.roomRepository = roomRepository;
      this.userRepository = userRepository;
      this.ordersApprovalRepository = ordersApprovalRepository;
    }

    public async Task<IEnumerable<ChatMessageDto>> GetAll(string room, DateTime day)
    {

      var result = await chatMessageRepository.GetAll(room, day);
      var orderApproval = await ordersApprovalRepository.GetByRoomDayAsync(room, day);
      string approversEmail = orderApproval?.Who;
      List<ChatMessageDto> list = new List<ChatMessageDto>();
      foreach (var chatMessage in result)
      {
        list.Add(new ChatMessageDto()
        {
          Who = chatMessage.Who.Email,
          IsApprover = chatMessage.Who.Email == approversEmail,
          Message = chatMessage.Message,
          Date = chatMessage.Created
        });
      }
      return list;
    }


    public async Task<bool> AddMessage(string room, ChatMessageAddRequestDto message)
    {
      var roomEntity = await roomRepository.GetByNameAsync(room);
      if (roomEntity == null)
        throw new BusinessException($"Room {room} not found");

      var userEntity = await userRepository.GetByEmailAsync(message.Who);
      if (userEntity == null)
        throw new BusinessException("User not found");
      ChatMessage chatMessage = new ChatMessage(Guid.NewGuid())
      {
        Room = roomEntity,
        Created = DateTime.Now,
        Day = DateTime.Now.Date,
        Message = message.Message,
        Who = userEntity
      };
      await chatMessageRepository.Add(chatMessage);
      return true;
    }

  }
}
