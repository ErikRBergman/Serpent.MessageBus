﻿namespace Serpent.Common.MessageBus.MessageHandlerChain.Decorators.Distinct
{
    using System.Linq;
    using System.Reflection;

    using Serpent.Common.MessageBus.MessageHandlerChain.WireUp;

    public class DistinctWireUp : BaseWireUp<DistinctAttribute>
    {
        public const string WireUpExtensionName = "DistinctWireUp";

        public override void WireUp<TMessageType, THandlerType>(
            DistinctAttribute attribute,
            IMessageHandlerChainBuilder<TMessageType> messageHandlerChainBuilder,
            THandlerType handler)
        {
            SelectorSetup<TMessageType, DistinctWireUp>
                .WireUp(
                    attribute.PropertyName,
                    () =>
                        typeof(DistinctExtensions)
                            .GetMethods()
                            .FirstOrDefault(
                                m => m.IsGenericMethodDefinition && m.IsStatic && m.GetCustomAttributes<ExtensionMethodSelectorAttribute>().Any(a => a.Identifier == WireUpExtensionName)),
                    (methodInfo, selector) => methodInfo.Invoke(null, new object[] { messageHandlerChainBuilder, selector }));
        }
    }
}