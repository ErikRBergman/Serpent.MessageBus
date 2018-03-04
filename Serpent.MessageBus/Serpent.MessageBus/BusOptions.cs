namespace Serpent.MessageBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     The concurrent message bus options
    /// </summary>
    /// <typeparam name="TMessageType">The message type</typeparam>
    public class BusOptions<TMessageType>
    {
        /// <summary>
        ///     Gets or sets the custom publish method. If CustomPublishFunc is not null, the method is invoked instead of the
        ///     default bus publisher method.
        /// </summary>
        public Func<IEnumerable<Func<TMessageType, CancellationToken, Task>>, TMessageType, CancellationToken, Task> CustomPublishFunc { get; set; }

        internal static BusOptions<TMessageType> Default { get; } = new BusOptions<TMessageType>();
    }
}