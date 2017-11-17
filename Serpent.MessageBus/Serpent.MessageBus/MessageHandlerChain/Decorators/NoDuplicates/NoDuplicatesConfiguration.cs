﻿// ReSharper disable CheckNamespace
namespace Serpent.MessageBus
{
    using Serpent.MessageBus.MessageHandlerChain.WireUp;

    /// <summary>
    /// The no duplicates decorator wire up configuration
    /// </summary>
    [WireUpConfigurationName("NoDuplicates")]
    public class NoDuplicatesConfiguration
    {
        /// <summary>
        /// The name of the message property used as a key to identify duplicates
        /// </summary>
        public string PropertyName { get; set; }
    }
}