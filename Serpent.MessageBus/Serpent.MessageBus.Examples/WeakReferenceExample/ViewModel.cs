namespace Serpent.MessageBus.Examples.WeakReferenceExample
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Serpent.Chain;
    using Serpent.Chain.Interfaces;

    public class ViewModel : IMessageHandler<OrdersListUpdatedEvent>
    {
        private readonly bool isWeak;

        public ViewModel(IMessageBusSubscriptions<OrdersListUpdatedEvent> subscriptions, bool weakReference)
        {
            this.Subscription = subscriptions.Subscribe(
                b =>
                    {
                        if (weakReference)
                        {
                            b.WeakReference(this);
                        }
                        else
                        {
                            b.Handler(this);
                        }
                    });

            this.isWeak = weakReference;
        }

        public IChain<OrdersListUpdatedEvent> Subscription { get; }

        public Task HandleMessageAsync(OrdersListUpdatedEvent message, CancellationToken cancellationToken)
        {
            Console.WriteLine("Event handled by " + (this.isWeak ? "weak referenced" : "strong referenced") + " view model");
            return Task.CompletedTask;
        }
    }
}