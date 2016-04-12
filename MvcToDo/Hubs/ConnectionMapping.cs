using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcToDo.Hubs
{
    /// <summary>
    /// Keep tracking users, not all messages will be for all users :)
    /// </summary>
    /// <typeparam name="T">something</typeparam>
    public class ConnectionMapping
    {
        private readonly List<SingleConnection> _connections = new List<SingleConnection>();

        /// <summary>
        /// Count all connections in a specific moment
        /// </summary>
        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        /// <summary>
        /// Add a user to the list of chatters :P
        /// </summary>
        /// <param name="key"></param>
        /// <param name="connectionId"></param>
        public void Add(string key, string connectionId)
        {
            lock (_connections)
            {
                var sn = _connections.Where(x => x.Id == key).FirstOrDefault();
                if (sn != null) // there is a connection with this key
                {
                    _connections.Find(x => x.Id == key).ConnectionId.Add(connectionId);
                }
                else
                {
                    _connections.Add(new SingleConnection { Id = key, ConnectionId = new List<string> { connectionId } });
                }
            }
        }

        public List<string> GetConnections(string id)
        {
            var con = _connections.Find(x => x.Id == id);
            return con != null ?  con.ConnectionId : new List<string>();
        }

        public List<string> AllConnectionIds() 
        {
            List<string> results = new List<string>();
             var allItems = _connections.Where(x => x.ConnectionId.Count > 0).ToList();
             foreach (var item in allItems)
             {
                 results.AddRange(item.ConnectionId);
             }
             return results;
        }

        /// <summary>
        /// return al list of strings containing all keys connected
        /// </summary>
        /// <returns></returns>
        public List<string> AllKeys() 
        {
            return _connections.Select(x => x.Id).ToList();
        }

        public void Remove(string key, string connectionId)
        {
            lock (_connections)
            {
                var item = _connections.Find(x => x.Id == key);
                if (_connections.Find(x => x.Id == key) != null)
                {
                    _connections.Find(x => x.Id == key).ConnectionId.Remove(connectionId);
                    if (_connections.Find(x => x.Id == key).ConnectionId.Count == 0)
                    {
                        _connections.Remove(item);
                    }
                }
            }
        }
    }

    public class SingleConnection 
    {
        public SingleConnection()
        {
            ConnectionId = new List<string>();
        }
        public string Id { get; set; }
        public List<string> ConnectionId { get; set; }
    }
}