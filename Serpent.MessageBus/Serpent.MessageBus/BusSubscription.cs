namespace Serpent.MessageBus
{
    using System;

    using Serpent.MessageBus.Helpers;

    internal class BusSubscription : IMessageBusSubscription
    {
        private readonly object lockObject = new object();
        private Action unsubscribeAction;

        public BusSubscription(Action unsubscribeAction)
        {
            this.unsubscribeAction = unsubscribeAction;
        }

        public void Dispose()
        {
            this.Unsubscribe();
        }

        private void Unsubscribe()
        {
            lock (this.lockObject)
            {
                var action = this.unsubscribeAction;
                this.unsubscribeAction = ActionHelpers.NoAction;
                action.Invoke();
            }
        }
    }
}