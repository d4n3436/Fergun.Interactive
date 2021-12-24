using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Fergun.Interactive
{
    /// <summary>
    /// Represents a wrapper for REST and WebSocket component interactions.
    /// </summary>
    internal class GenericMessageComponent : GenericInteraction, IComponentInteraction
    {
        private readonly SocketMessageComponent? _socketComponent;
        private readonly RestMessageComponent? _restComponent;
        private readonly IComponentInteraction _component;
        private new readonly Func<string, Task>? _restCallback;

        [MemberNotNullWhen(true, nameof(_restComponent), nameof(_restCallback))]
        [MemberNotNullWhen(false, nameof(_socketComponent))]
        public override bool IsRestInteraction => _restComponent != null;

        public GenericMessageComponent(IComponentInteraction component, Func<string, Task> restCallback)
        : base(component, restCallback)
        {
            _component = component;
            _socketComponent = _socketInteraction as SocketMessageComponent;
            _restComponent = _restInteraction as RestMessageComponent;
            _restCallback = base._restCallback;
        }

        /// <inheritdoc cref="SocketMessageComponent.UpdateAsync(Action{MessageProperties}, RequestOptions)"/>
        public async Task UpdateAsync(Action<MessageProperties> func, RequestOptions? options = null)
        {
            if (IsRestInteraction)
                await _restCallback(_restComponent.Update(func, options)).ConfigureAwait(false);
            else
                await _socketComponent.UpdateAsync(func, options).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public IComponentInteractionData Data => _component.Data;

        /// <inheritdoc />
        public IUserMessage Message => _component.Message;
    }
}