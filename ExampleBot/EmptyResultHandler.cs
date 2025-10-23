using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services;
using NetCord.Services.ComponentInteractions;

namespace ExampleBot
{
    public class EmptyResultHandler<TContext> : IComponentInteractionResultHandler<TContext> where TContext : IComponentInteractionContext
    {
        /// <inheritdoc />
        public ValueTask HandleResultAsync(IExecutionResult result, TContext context, GatewayClient? client, ILogger logger, IServiceProvider services)
            => ValueTask.CompletedTask;
    }
}