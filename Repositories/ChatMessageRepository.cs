
using Microsoft.Extensions.Options;
using PizzaHotOnion.Configuration;
using PizzaHotOnion.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using System;

namespace PizzaHotOnion.Repositories
{
  public class ChatMessageRepository : MongoCrudRepository<ChatMessage>, IChatMessageRepository
  {
    public ChatMessageRepository(IOptions<Settings> settings) : base(settings) { }


    public async Task<IEnumerable<ChatMessage>> GetAll(string room, DateTime orderDay)
    {
      var filter = Builders<ChatMessage>.Filter.Eq(nameof(ChatMessage.Room) + '.' + nameof(Room.Name), room);
      filter = filter & (Builders<ChatMessage>.Filter.Eq(nameof(ChatMessage.Day), orderDay));
      //filter = filter & (Builders<ChatMessage>.Sort.Descending(f => f.Created));
      return await this.GetMongoCollection()
          .Find(filter)
          .SortByDescending(f => f.Created)
          .ToListAsync();
    }

  }
}