﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Sockets;
using Microsoft.Extensions.DependencyInjection;
using SocketsSample.EndPoints.Hubs;
using SocketsSample.Hubs;

namespace SocketsSample
{
    public class HubEndPoint<THub> : RpcEndpoint, IHubConnectionContext where THub : Hub
    {
        protected readonly IServiceProvider _serviceProvider;
        private readonly AllClientProxy<THub> _all;
        private readonly HubLifetimeManager<THub> _lifetimeManager;

        public HubEndPoint(HubLifetimeManager<THub> lifetimeManager, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _lifetimeManager = lifetimeManager;
            _serviceProvider = serviceProvider;
            _all = new AllClientProxy<THub>(_lifetimeManager);
        }

        public IHubConnectionContext Clients => this;

        public virtual IClientProxy All => _all;

        public virtual IClientProxy Client(string connectionId)
        {
            return new SingleClientProxy<THub>(_lifetimeManager, connectionId);
        }

        public virtual IClientProxy Group(string groupName)
        {
            return new GroupProxy<THub>(_lifetimeManager, groupName);
        }

        public virtual IClientProxy User(string userId)
        {
            return new UserProxy<THub>(_lifetimeManager, userId);
        }

        public override async Task OnConnected(Connection connection)
        {
            var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();

            try
            {
                await _lifetimeManager.OnConnectedAsync(connection);

                using (var scope = scopeFactory.CreateScope())
                {
                    var value = scope.ServiceProvider.GetService<THub>() ?? Activator.CreateInstance<THub>();
                    Initialize(connection, value);
                    await value.OnConnectedAsync();
                }

                await base.OnConnected(connection);
            }
            finally
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var value = scope.ServiceProvider.GetService<THub>() ?? Activator.CreateInstance<THub>();
                    Initialize(connection, value);
                    await value.OnDisconnectedAsync();
                }

                await _lifetimeManager.OnDisconnectedAsync(connection);
            }
        }

        protected override void BeforeInvoke(Connection connection, object endpoint)
        {
            Initialize(connection, endpoint);
        }

        private void Initialize(Connection connection, object endpoint)
        {
            var hub = (THub)endpoint;
            hub.Clients = this;
            hub.Context = new HubCallerContext(connection);
            hub.Groups = new GroupManager<THub>(connection, _lifetimeManager);
        }

        protected override void AfterInvoke(Connection connection, object endpoint)
        {
            // Poison the hub make sure it can't be used after invocation
        }

        protected override void DiscoverEndpoints()
        {
            RegisterRPCEndPoint(typeof(THub));
        }
    }
}