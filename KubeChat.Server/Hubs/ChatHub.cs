using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KubeChat.Server.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly Dictionary<string, string> Usernames = new Dictionary<string, string>();
        private ILogger<ChatHub> logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            this.logger = logger;
        }

        public async Task Register(string username)
        {
            var currentId = Context.ConnectionId;
            if (!Usernames.ContainsKey(currentId))
            {
                Usernames.Add(currentId, username);
                var message = $"{username} joined the chat.";
                await Clients.AllExcept(currentId).SendAsync("ReceiveMessage", username, message);
                logger.LogInformation(message);
            }
        }

        public async Task SendMessage(Guid messageGuid, string message)
        {
            var currentId = Context.ConnectionId;
            if (Usernames.ContainsKey(currentId))
            {
                await Clients.AllExcept(currentId).SendAsync("ReceiveMessage", Usernames[currentId], message);
                await Clients.Caller.SendAsync("MessageConfirmation", messageGuid);
                logger.LogInformation($"{Usernames[currentId]}: {message}");
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var currentId = Context.ConnectionId;
            if (!Usernames.TryGetValue(currentId, out string username))
            {
                username = "[unknown]";
            }

            Usernames.Remove(currentId);
            await Clients.AllExcept(currentId).SendAsync("ReceiveMessage", username, $"{username} has left the chat");
            logger.LogInformation($"{username} disconnected");

            await base.OnDisconnectedAsync(exception);
        }
    }
}
