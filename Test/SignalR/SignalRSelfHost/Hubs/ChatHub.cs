using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SignalRSelfHost.Hubs
{
    [HubName("chat")]
    public class ChatHub : Hub
    {
        static readonly HashSet<string> Rooms = new HashSet<string>();

        public string Login(string name)
        {
            Clients.Caller.rooms(Rooms.ToArray());
            return name;
        }

        public void Logout(string name)
        {

        }

        public void JoinRoom(string name)
        {
            if (!Rooms.Contains(name)) return;
            Groups.Add(Context.ConnectionId, name);
            Clients.Caller.@join(name);
        }

        public void CreateRoom(string name)
        {
            if (Rooms.Contains(name)) return;
            Rooms.Add(name);
            JoinRoom(name);
            Clients.All.rooms(new[] { name });
        }

        public void Send(string room, string message, string user)
        {
            Clients.Group(room).message(room, new { message, sender = user });

            Clients.Group(room).message(room, new { message, sender = user });
        }
    }
}
