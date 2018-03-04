namespace Serpent.MessageBus
{
    using System;

    /// <summary>
    /// ConcurrentMessageBus&lt;TMessageType&gt; is replaced by Bus&lt;TMessageType&gt;
    ///     The message bus. 
    /// </summary>
    /// <typeparam name="TMessageType">The message type</typeparam>
    [Obsolete("ConcurrentMessageBus<TMessageType> is replaced by Bus<TMessageType>")]
    public class ConcurrentMessageBus<TMessageType> : Bus<TMessageType>
    {
        /// <summary>
        /// ConcurrentMessageBus&lt;TMessageType&gt; is replaced by Bus&lt;TMessageType&gt;
        /// Initializes a new instance of the <see cref="Bus{TMessageType}"/> class. 
        /// </summary>
        /// <param name="options">
        /// The options
        /// </param>
        [Obsolete("ConcurrentMessageBus<TMessageType> is replaced by Bus<TMessageType>")]
        public ConcurrentMessageBus(BusOptions<TMessageType> options) : base(options)
        {
        }

        /// <summary>
        /// ConcurrentMessageBus&lt;TMessageType&gt; is replaced by Bus&lt;TMessageType&gt;
        /// Initializes a new instance of the <see cref="Bus{TMessageType}"/> class. 
        /// </summary>
        /// <param name="optionsAction">
        /// A method that configures the message bus options
        /// </param>
        [Obsolete("ConcurrentMessageBus<TMessageType> is replaced by Bus<TMessageType>")]
        public ConcurrentMessageBus(Action<BusOptions<TMessageType>> optionsAction) : base(optionsAction)
        {
        }

        /// <summary>
        /// ConcurrentMessageBus&lt;TMessageType&gt; is replaced by Bus&lt;TMessageType&gt;
        /// Initializes a new instance of the <see cref="Bus{TMessageType}"/> class. 
        /// </summary>
        [Obsolete("ConcurrentMessageBus<TMessageType> is replaced by Bus<TMessageType>")]
        public ConcurrentMessageBus()
        {
        }
    }
}