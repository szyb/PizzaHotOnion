using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PizzaHotOnion.DTOs;
using PizzaHotOnion.Entities;
using PizzaHotOnion.Infrastructure;
using PizzaHotOnion.Infrastructure.Security;
using PizzaHotOnion.Repositories;
using PizzaHotOnion.Services;

namespace PizzaHotOnion.Controllers
{
  [BusinessExceptionFilter]
  [Produces("application/json")]
  [Route("api/[controller]")]
  [Authorize]
  public class ChatMessagesController : Controller
  {
    private readonly IAuthenticationService authenticationService;
    private readonly IChatMessageService chatMessageService;
    private readonly IHubContext<MessageHub> messageHubContext;

    public ChatMessagesController(IAuthenticationService authenticationService, IChatMessageService chatMessageService, IHubContext<MessageHub> messageHubContext)
    {
      this.authenticationService = authenticationService;
      this.chatMessageService = chatMessageService;
      this.messageHubContext = messageHubContext;
     
    }
   
    [HttpGet("{room}")]
    public async Task<IActionResult> GetMessages(string room)
    {
      if (string.IsNullOrWhiteSpace(room))
        return BadRequest();
      DateTime day = DateTime.Now.Date;
      var result = await chatMessageService.GetAll(room, day);

      return Ok(result);
    }

    [HttpPost("{room}")]
    public async Task<IActionResult> AddMessage(string room, [FromBody] ChatMessageAddRequestDto message)
    {
      if (string.IsNullOrWhiteSpace(room) || message == null || room != message.Room)
        return BadRequest();
      bool result = await chatMessageService.AddMessage(room, message);
      await messageHubContext.Clients.All.SendAsync(
         "send",
          new MessageDTO { Operation = OperationType.NewChatMessage, Context = room });

      if (result)
        return NoContent();
      else
        return BadRequest();


    }

  }
}
