namespace Serpent.MessageBus.Examples.Unsubscribe
{
    using System;

    public class UnsubscribeExample
    {
        public static void Unsubscribe()
        {
            // subscribe
            var subscription = Use<int>.Bus.Subscribe(value => Console.Write("Value: " + value));

            // publish
            Use<int>.Bus.Publish(123);

            // unsubscribe
            subscription.Dispose();
        }
    }
}