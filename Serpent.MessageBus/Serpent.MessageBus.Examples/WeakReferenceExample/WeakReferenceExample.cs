namespace Serpent.MessageBus.Examples.WeakReferenceExample
{
    using System;

    using Serpent.MessageBus.Examples.Extensions;

    public class WeakReferenceExample
    {

        private static void CreateViewModel(bool weakReference)
        {
            var viewModel = new ViewModel(Use<OrdersListUpdatedEvent>.Bus, weakReference);
        }

        public static void WeakReferenceExampleMethod()
        {
            Console.WriteLine();
            ColorConsole.WriteLine(ConsoleColor.White, "** WeakReference example **");

            var bus = Use<OrdersListUpdatedEvent>.Bus;

            Console.WriteLine($"Number of subscriptions {bus.SubscriberCount}. Should be 0.");

            Console.WriteLine($"Creating 2 view models (which in turn subscribes)");

            CreateViewModel(false);
            CreateViewModel(true);

            // Publish an event. This reaches both view models
            bus.Publish(new OrdersListUpdatedEvent());

            Console.WriteLine($"Number of subscriptions {bus.SubscriberCount}. Should be 2.");

            Console.WriteLine($"Collecting garbage");

            GC.Collect(2, GCCollectionMode.Forced);

            Console.WriteLine($"Number of subscriptions {bus.SubscriberCount}. Should be 2 - since the subscription has not yet been removed.");

            // Publish an event. This reaches the strong referenced view model and unsubscribes the weak one
            bus.Publish(new OrdersListUpdatedEvent());

            Console.WriteLine($"Number of subscriptions {bus.SubscriberCount}. Should be 1.");

            Console.WriteLine();
            ColorConsole.WriteLine(ConsoleColor.White, "** End of WeakReference example **");
        }
    }
}