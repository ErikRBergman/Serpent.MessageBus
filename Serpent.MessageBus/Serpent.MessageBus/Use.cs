// ReSharper disable StyleCop.SA1402
namespace Serpent.MessageBus
{
    /// <summary>
    /// Provides a static singleton message bus for any type
    /// </summary>
    /// <typeparam name="TMessageType">The message used by the message bus</typeparam>
    public static class Use<TMessageType>
    {
        /// <summary>
        /// Gets the static singleton message bus
        /// </summary>
        public static Bus<TMessageType> Bus { get; } = new Bus<TMessageType>();
    }

    /// <summary>
    /// Provides a static singleton message bus for any type
    /// </summary>
    public static class Use
    {
        /// <summary>
        /// Gets the static singleton message bus for  <see cref="System.Object"/>
        /// </summary>
        public static Bus<object> Bus { get; } = new Bus<object>();

        /// <summary>
        /// Gets a static singleton messagebus by generic type
        /// </summary>
        /// <typeparam name="TMessageType"></typeparam>
        /// <returns></returns>
        public static Bus<TMessageType> GetBus<TMessageType>() => Use<TMessageType>.Bus;
    }
}