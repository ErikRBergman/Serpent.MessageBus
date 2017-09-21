﻿namespace Serpent.Common.MessageBus.SubscriptionTypes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class TaskSchedulerSubscription<TMessageType> : BusSubscription<TMessageType>
    {
        private readonly Func<TMessageType, Task> handlerFunc;

        private readonly TaskScheduler taskScheduler;

        public TaskSchedulerSubscription(Func<TMessageType, Task> handlerFunc, TaskScheduler taskScheduler)
        {
            this.handlerFunc = handlerFunc;
            this.taskScheduler = taskScheduler;
        }

        public TaskSchedulerSubscription(BusSubscription<TMessageType> innerSubscription, TaskScheduler taskScheduler)
        {
            this.taskScheduler = taskScheduler;
            this.handlerFunc = innerSubscription.HandleMessageAsync;
        }

        public override Task HandleMessageAsync(TMessageType message)
        {
            var task = Task.Factory.StartNew(
                () => this.handlerFunc(message), 
                CancellationToken.None,
                TaskCreationOptions.None, 
                this.taskScheduler).Unwrap();
            return task;
        }
    }
}