﻿using System.Security.Claims;
using Microsoft.AspNetCore.Sockets;

namespace Microsoft.AspNetCore.SignalR
{
    public class HubCallerContext
    {
        public HubCallerContext(Connection connection)
        {
            ConnectionId = connection.ConnectionId;
            User = connection.User;
            Connection = connection;
        }

        public Connection Connection { get; }

        public ClaimsPrincipal User { get; }

        public string ConnectionId { get; }
    }
}