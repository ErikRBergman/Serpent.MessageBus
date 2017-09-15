﻿namespace Serpent.Common.MessageBus.SubscriptionTypes
{
    using System;
    using System.Threading.Tasks;

    public abstract class BusSubscription<T>
    {
        public abstract Task HandleMessageAsync(T message);
    }
}