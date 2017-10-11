﻿// ReSharper disable once CheckNamespace
namespace Serpent.Common.MessageBus
{
    using System;
    using System.Collections.Generic;

    using Serpent.Common.MessageBus.MessageHandlerChain.Decorators.NoDuplicates;
    using Serpent.Common.MessageBus.MessageHandlerChain.WireUp;

    public static class NoDuplicatesExtensions
    {
        [ExtensionMethodSelector(NoDuplicatesWireUp.WireUpExtensionName)]
        public static IMessageHandlerChainBuilder<TMessageType> NoDuplicates<TMessageType, TKeyType>(this IMessageHandlerChainBuilder<TMessageType> messageHandlerChainBuilder, Func<TMessageType, TKeyType> keySelector)
        {
            return messageHandlerChainBuilder.Add(currentHandler => new NoDuplicatesDecorator<TMessageType, TKeyType>(currentHandler, keySelector));
        }

        public static IMessageHandlerChainBuilder<TMessageType> NoDuplicates<TMessageType, TKeyType>(this IMessageHandlerChainBuilder<TMessageType> messageHandlerChainBuilder, Func<TMessageType, TKeyType> keySelector, IEqualityComparer<TKeyType> equalityComparer)
        {
            return messageHandlerChainBuilder.Add(currentHandler => new NoDuplicatesDecorator<TMessageType, TKeyType>(currentHandler, keySelector, equalityComparer));
        }
    }
}