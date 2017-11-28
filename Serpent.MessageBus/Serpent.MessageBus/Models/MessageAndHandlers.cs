namespace Serpent.MessageBus.Models
{
    using System.Collections.Generic;

    using Serpent.Chain.Interfaces;

    /// <summary>
    /// The message and handlers type
    /// </summary>
    /// <typeparam name="TMessageType">The message type</typeparam>
    public struct MessageAndHandlers<TMessageType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageAndHandlers{TMessageType}"/> struct. 
        /// </summary>
        /// <param name="message">
        /// The message
        /// </param>
        /// <param name="handlers">
        /// The message handlers
        /// </param>
        public MessageAndHandlers(TMessageType message, IEnumerable<IMessageHandler<TMessageType>> handlers)
        {
            this.Message = message;
            this.Handlers = handlers;
        }

        /// <summary>
        /// The message
        /// </summary>
        public TMessageType Message { get; }

        /// <summary>
        /// The message handlers
        /// </summary>
        public IEnumerable<IMessageHandler<TMessageType>> Handlers { get; }
    }
}