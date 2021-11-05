using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PizzaHotOnion.DTOs;
using PizzaHotOnion.Entities;
using PizzaHotOnion.Repositories;
using PizzaHotOnion.Services;
using System.IO;

namespace PizzaHotOnion.Controllers
{
  [Produces("application/json")]
  [Route("api/[controller]")]
  public class OrdersController : Controller
  {
    private readonly IOrderRepository orderRepository;
    private readonly IRoomRepository roomRepository;
    private readonly IUserRepository userRepository;
    private readonly IOrdersApprovalRepository ordersApprovalRepository;
    private readonly IHubContext<MessageHub> messageHubContext;
    private readonly IEmailService emailSerice;

    public OrdersController(
        IOrderRepository orderRepository,
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IOrdersApprovalRepository ordersApprovalRepository,
        IHubContext<MessageHub> messageHubContext,
        IEmailService emailSerice)
    {
      this.orderRepository = orderRepository;
      this.roomRepository = roomRepository;
      this.userRepository = userRepository;
      this.ordersApprovalRepository = ordersApprovalRepository;
      this.messageHubContext = messageHubContext;
      this.emailSerice = emailSerice;
    }

    [HttpGet("{room}", Name = "GetAll")]
    public async Task<IEnumerable<OrderDTO>> GetAll(string room)
    {
      IList<OrderDTO> result = new List<OrderDTO>();
      DateTime orderDay = DateTime.Now.Date;

      var orders = await this.orderRepository.GetAllInRoom(room, orderDay);
      var ordersApproval = await this.ordersApprovalRepository.GetByRoomDayAsync(room, orderDay);

      foreach (var order in orders)
        result.Add(new OrderDTO
        {
          Id = order.Id,
          Day = order.Day,
          Who = order.Who.Email,
          Quantity = order.Quantity,
          Price = order.Price,
          Room = order.Room.Name,
          IsApproved = ordersApproval != null
        });

      return result;
    }

    [HttpGet("details/{id}", Name = "GetOrder")]
    public async Task<IActionResult> GetById(Guid id)
    {
      if (id == Guid.Empty)
        return BadRequest();

      var order = await this.orderRepository.Get(id);
      if (order == null)
        return NotFound();

      OrderDTO orderDTO = new OrderDTO
      {
        Id = order.Id,
        Day = order.Day,
        Quantity = order.Quantity,
        Price = order.Price,
        Who = order.Who.Email,
        Room = order.Room.Name
      };

      return new ObjectResult(orderDTO);
    }

    [HttpPost("{room}")]
    public async Task<IActionResult> Create(string room, [FromBody] MakeOrderDTO orderDTO)
    {
      if (orderDTO == null)
        return BadRequest("Cannot add order because data is empty");

      if (orderDTO.Quantity < 1)
        return BadRequest("Cannot add order because quantity has to be greater or equal 1");

      if (string.IsNullOrEmpty(orderDTO.Room))
        return BadRequest("Cannot add order because room name is required");

      if (string.IsNullOrEmpty(orderDTO.Who))
        return BadRequest("Cannot add order because login is empty");

      DateTime orderDay = DateTime.Now.Date;

      if (await this.ordersApprovalRepository.CheckExistsByRoomDayAsync(room, orderDay))
        return BadRequest("Cannot add order because orders are approved");

      var roomEntity = await this.roomRepository.GetByNameAsync(orderDTO.Room);
      if (roomEntity == null)
        return BadRequest(string.Format("Cannot add order because room '{0}' does not exist", orderDTO.Room));

      var userEntity = await this.userRepository.GetByEmailAsync(orderDTO.Who);
      if (userEntity == null)
        return BadRequest(string.Format("Cannot add order because user '{0}' does not exist", orderDTO.Who));

      bool sendMessage = !await orderRepository.CheckAnyOrderExists(orderDTO.Room, orderDay);

      Order orderEntity = await this.orderRepository.GetOrder(orderDTO.Room, orderDay, orderDTO.Who);
      if (orderEntity == null)
      {
        orderEntity = new Order(Guid.NewGuid());
        orderEntity.Day = orderDay;
        orderEntity.Who = userEntity;
        orderEntity.Room = roomEntity;
        orderEntity.Quantity = orderDTO.Quantity;
        await this.orderRepository.Add(orderEntity);
      }
      else
      {
        orderEntity.Quantity = orderDTO.Quantity;
        await this.orderRepository.Update(orderEntity);
      }

      //Broadcast message to client  
      await this.messageHubContext.Clients.All
        .SendAsync(
          "send",
          new MessageDTO { Operation = OperationType.SliceGrabbed, Context = orderDTO.Room }
        );

      if (sendMessage)
      {
        var users = await this.userRepository.GetAll();
        if (users != null && users.Count() > 0)
        {
          string initialMessage = $"Oops someone is hungry. The pizza has been just opened in {orderDTO.Room} room by {orderDTO.Who}. Can you join me? Let's get some pizza.";
          if (System.IO.File.Exists("InitialMessage.txt"))
          {
            var fileInitialMessage = System.IO.File.ReadAllText("InitialMessage.txt");
            if (!string.IsNullOrWhiteSpace(fileInitialMessage))
              initialMessage = initialMessage + System.Environment.NewLine + fileInitialMessage;
          }
          foreach (var user in users.Where(u => u.EmailNotification))
          {
            this.emailSerice
              .Send(
                user.Email,
                "Hot Onion - someone is hungry",
                initialMessage
              );
          }
        }
      }

      return CreatedAtRoute("GetOrder", new { id = orderEntity.Id }, new { });
    }

    [HttpPatch("{room}/{id}")]
    public async Task<IActionResult> Update(string room, Guid id, [FromBody] OrderDTO orderDTO)
    {
      if (orderDTO == null)
        return BadRequest("Cannot update order because data is empty");

      if (orderDTO.Id == null || orderDTO.Id == Guid.Empty || orderDTO.Id != id)
        return BadRequest("Cannot update order because data is empty");

      if (string.IsNullOrWhiteSpace(orderDTO.Room) || orderDTO.Room != room)
        return BadRequest("Cannot update order because room name is incorrect");

      if (await this.ordersApprovalRepository.CheckExistsByRoomDayAsync(room, DateTime.Now.Date))
        return BadRequest("Cannot update order because orders are approved");

      if (orderDTO.Quantity < 1)
        return BadRequest("Cannot update order because quantity has to be greater or equal 1");

      Order orderEntity = await this.orderRepository.Get(id);
      if (orderEntity == null)
        return BadRequest("Cannot update order because order does not exist");

      if (orderEntity.Room.Name != room)
        return BadRequest("Cannot update order because room name is incorrect");

      orderEntity.Quantity = orderDTO.Quantity;

      await this.orderRepository.Update(orderEntity);

      //Broadcast message to client  
      await this.messageHubContext.Clients.All
        .SendAsync(
          "send",
          new MessageDTO { Operation = OperationType.SliceGrabbed, Context = orderDTO.Room }
        );

      return new NoContentResult();
    }

    [HttpDelete("{room}/{id}")]
    public async Task<IActionResult> Delete(string room, Guid id)
    {
      if (id == Guid.Empty)
        return BadRequest("Cannot delete order because it does not exist");

      if (await this.ordersApprovalRepository.CheckExistsByRoomDayAsync(room, DateTime.Now.Date))
        return BadRequest("Cannot delete order because it is approved");

      var order = await this.orderRepository.Get(id);
      if (order == null)
        return BadRequest("Cannot delete order because it does not exist");

      if (order.Room.Name != room)
        return BadRequest("Cannot delete order because room name is incorrect");

      await this.orderRepository.Remove(id);

      //Broadcast message to client  
      await this.messageHubContext.Clients.All
        .SendAsync(
          "send",
          new MessageDTO { Operation = OperationType.SliceCancelled, Context = room }
        );

      return new NoContentResult();
    }

    [HttpPost("{room}/approve")]
    public async Task<IActionResult> Approve(string room, [FromBody] ApproveOrdersDTO approveOrdersDTO)
    {
      // if (approveOrdersDTO == null)
      //   return BadRequest();

      // if (approveOrdersDTO.PizzaQuantity < 1)
      //   return BadRequest("Pizza quantity has to be greater or equal 1");

      if (string.IsNullOrEmpty(room))
        return BadRequest("Cannot approve orders because room name is incorrect");

      if (approveOrdersDTO == null || approveOrdersDTO.Room != room || string.IsNullOrWhiteSpace(approveOrdersDTO.Approver))
        return BadRequest("Cannot approve orders because approver is missing");

      // if (room != approveOrdersDTO.Room)
      //   return BadRequest("Incorect room");

      var roomEntity = await this.roomRepository.GetByNameAsync(room);
      if (roomEntity == null)
        return BadRequest(string.Format("Cannot approve orders because room '{0}' does not exist", room));

      DateTime orderDay = DateTime.Now.Date;

      if (await this.ordersApprovalRepository.CheckExistsByRoomDayAsync(room, orderDay))
        return BadRequest("Cannot approve orders because orders are approved");

      var orders = await this.orderRepository.GetAllInRoom(room, orderDay);
      int slices = orders.Sum(o => o.Quantity);
      int pizzas = (int)Math.Ceiling((decimal)slices / OrdersApproval.DefaultSlicesPerPizza);

      OrdersApproval ordersApprovalEntity = new OrdersApproval(Guid.NewGuid());
      ordersApprovalEntity.Day = orderDay;
      ordersApprovalEntity.Room = roomEntity;
      ordersApprovalEntity.PizzaQuantity = pizzas;
      ordersApprovalEntity.Who = approveOrdersDTO.Approver;
      ordersApprovalEntity.SlicesPerPizza = OrdersApproval.DefaultSlicesPerPizza;
      await this.ordersApprovalRepository.Add(ordersApprovalEntity);

      //Broadcast message to client  
      await this.messageHubContext.Clients.All
        .SendAsync(
          "send",
          new MessageDTO { Operation = OperationType.OrdersApproved, Context = room }
        );

      return CreatedAtRoute("GetAll", new { room = room }, new { });
    }

    [HttpPost("{room}/cancelApproval")]
    public async Task<IActionResult> CancelApproval(string room, [FromBody] CancelApprovalDTO cancelApprovalDTO)
    {
      if (string.IsNullOrWhiteSpace(room) || cancelApprovalDTO == null || room != cancelApprovalDTO.Room)
        return BadRequest("Cannot cancel approval due to bad request");

      DateTime orderDay = DateTime.Now.Date;

      var approval = await this.ordersApprovalRepository.GetByRoomDayAsync(room, orderDay);
      if (approval == null)
        return BadRequest("Cannot cancel approve orders because it is not aproved yet!");

      if (approval.Who == cancelApprovalDTO.Who)
      {
        await this.ordersApprovalRepository.Remove(approval.Id);
        await this.messageHubContext.Clients.All
          .SendAsync(
          "Send",
          new MessageDTO { Operation = OperationType.ApprovalIsCancelled, Context = room });
        return NoContent();
      }
      else
        return BadRequest("Cannot cancel approval because approver missmatch with the request");
    }

    [HttpPost("{room}/setPrice")]
    public async Task<IActionResult> SetPrice(string room, [FromBody] SetPriceDTO setPriceDTO)
    {
      if (setPriceDTO == null || setPriceDTO.PricePerPizza < 0.0m || string.IsNullOrWhiteSpace(setPriceDTO.Who) || string.IsNullOrWhiteSpace(room) || room != setPriceDTO.Room ||
          setPriceDTO.SlicesPerPizza < 0)
        return BadRequest();

      DateTime orderDay = DateTime.Now.Date;
      var orderApproval = await this.ordersApprovalRepository.GetByRoomDayAsync(room, orderDay).ConfigureAwait(false);
      if (orderApproval == null)
        return BadRequest("Cannot set price because there is no approved orders");

      if (orderApproval.Who != setPriceDTO.Who)
        return BadRequest("Cannot set price because only approver can do this");

      orderApproval.PricePerPizza = setPriceDTO.PricePerPizza;
      orderApproval.SlicesPerPizza = setPriceDTO.SlicesPerPizza;

      await this.ordersApprovalRepository.Update(orderApproval).ConfigureAwait(false);

      var orders = await this.orderRepository.GetAllInRoom(room, orderDay).ConfigureAwait(false);

      foreach (var order in orders)
      {
        order.Price = Math.Round((setPriceDTO.PricePerPizza / (setPriceDTO.SlicesPerPizza * orderApproval.PizzaQuantity)) * order.Quantity, 2, MidpointRounding.AwayFromZero);
        await this.orderRepository.Update(order).ConfigureAwait(false);
      }

      await this.messageHubContext.Clients.All
       .SendAsync(
         "send",
         new MessageDTO { Operation = OperationType.PriceIsSet, Context = room }
       );

      return NoContent();
    }

    [HttpGet("{room}/ApprovalInfo")]
    public async Task<IActionResult> GetApprovalInfo(string room)
    {
      if (string.IsNullOrWhiteSpace(room))
        return BadRequest("Room is not provided");

      var orderDay = DateTime.Now.Date;
      var orderApproval = await this.ordersApprovalRepository.GetByRoomDayAsync(room, orderDay).ConfigureAwait(false);
      if (orderApproval != null)
        return new ObjectResult(new ApprovalInfoDTO()
        {
          Approver = orderApproval.Who,
          PricePerPizza = orderApproval.PricePerPizza,
          SlicesPerPizza = orderApproval.SlicesPerPizza,
          OrderArrived = orderApproval.Arrived
        });
      else return Ok();
    }

    [HttpPost("{room}/orderArrived")]
    public async Task<IActionResult> OrderArrived(string room)
    {
      if (string.IsNullOrWhiteSpace(room))
        return BadRequest("Room is not provided");

      var orderDay = DateTime.Now.Date;
      var orderApproval = await this.ordersApprovalRepository.GetByRoomDayAsync(room, orderDay).ConfigureAwait(false);
      if (orderApproval != null)
      {
        if (orderApproval.SlicesPerPizza == 0 || orderApproval.PizzaQuantity == 0)
          return BadRequest("SlicesPerPizza is 0 or PizzaQuantity = 0");

        orderApproval.Arrived = true;
        await this.ordersApprovalRepository.Update(orderApproval).ConfigureAwait(false);
        var orders = await this.orderRepository.GetAllInRoom(room, orderDay).ConfigureAwait(false);
        var approver = await this.userRepository.GetByEmailAsync(orderApproval.Who).ConfigureAwait(false);
        StringBuilder approverSummary = new StringBuilder();
        string bodyTemplate =
              @"Pizza arrived!
              You ordered {0} slice(s) for {1} PLN in total ({2} PLN per slice)
              Below is the approver's ({3}) message.

              {4}
              ";

        var pricePerSlice = Math.Round((orderApproval.PricePerPizza / (orderApproval.SlicesPerPizza * orderApproval.PizzaQuantity)), 2, MidpointRounding.AwayFromZero);

        foreach (var order in orders)
        {
          //var cost = Math.Round(order.Quantity * orderApproval.PricePerPizza, 2, MidpointRounding.AwayFromZero);
          var cost = order.Price;

          approverSummary.AppendLine($"{order.Who.Email}: {order.Quantity} ({cost})");
          this.emailSerice.Send(order.Who.Email,
            "Hot Onion - Pizza arrived!",
            string.Format(bodyTemplate,
              order.Quantity,
              cost,
              pricePerSlice,
              approver.Email,
              approver.ApproversMessage));
        }

        this.emailSerice.Send(approver.Email, "Hot Onion - Pizza summary", "Below is an order summary" + Environment.NewLine + approverSummary.ToString());

        await this.messageHubContext.Clients.All
         .SendAsync(
           "send",
           new MessageDTO { Operation = OperationType.OrderArrived, Context = room }
       );

        return NoContent();

      }
      else
        return BadRequest("Cannot set order as arrived");
    }

  }
}
