﻿namespace Microsoft.AspNetCore.SignalR
{
    public interface IHubConnectionContext
    {
        IClientProxy All { get; }

        IClientProxy Client(string connectionId);

        IClientProxy Group(string groupName);

        IClientProxy User(string userId);
    }
}