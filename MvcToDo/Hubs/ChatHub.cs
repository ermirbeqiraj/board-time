using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using MvcToDo.App_Code;

namespace MvcToDo.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly static ConnectionMapping _connections = new ConnectionMapping();

        public void SendMessage(string displayName ,string to, string message)
        {
            string name = Context.User.Identity.Name;
            DateTime when =  DateTime.Now;
            foreach (var connectionId in _connections.GetConnections(to))
            {
                Clients.Client(connectionId).addNewMessageToPage(displayName, name, message, when.ToString("dd-MMM-yy HH:mm"));
            }
        }

        /// <summary>
        /// Activate a client
        /// </summary>
        /// <param name="connection"></param>
        private void IsActive(string connection, bool connected) 
        {
            Clients.All.clientconnected(connection, connected);
        }

        public override Task OnConnected()
        {
            string name = Context.User.Identity.Name;
            _connections.Add(name, Context.ConnectionId);
            IsActive(name, true);
            /*
             * Send all other online friends to the connected user
             */
            Task.Run(() =>
            {
                var items = _connections.AllKeys();
                foreach (var item in items)
                {
                    Clients.Client(Context.ConnectionId).clientconnected(item, true);
                }
            });

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string name = Context.User.Identity.Name;
            _connections.Remove(name, Context.ConnectionId);
            IsActive(name, false);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            string name = Context.User.Identity.Name;
            if (!_connections.GetConnections(name).Contains(Context.ConnectionId))
            {
                _connections.Add(name, Context.ConnectionId);
            }
            IsActive(name, false);

            return base.OnReconnected();
        }
    }
}