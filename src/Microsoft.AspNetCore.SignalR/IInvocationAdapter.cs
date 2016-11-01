﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SignalR
{
    public interface IInvocationAdapter
    {
        Task<InvocationDescriptor> ReadInvocationDescriptor(Stream stream, Func<string, Type[]> getParams);

        Task WriteInvocationResult(InvocationResultDescriptor resultDescriptor, Stream stream);

        Task WriteInvocationDescriptor(InvocationDescriptor invocationDescriptor, Stream stream);
    }
}