﻿// ReSharper disable once CheckNamespace
namespace Serpent.Common.MessageBus
{
    using Serpent.Common.MessageBus.MessageHandlerChain.WireUp;

    /// <summary>
    /// The NoDuplicates attribute
    /// </summary>
    public class NoDuplicatesAttribute : WireUpAttribute
    {
        /// <summary>
        /// Initializes an attribute where the property named propertyName is used as key to eliminate duplicates
        /// </summary>
        /// <param name="propertyName">The property name</param>
        public NoDuplicatesAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Initializes a new instance of the no duplicates decorator attribute
        /// </summary>
        public NoDuplicatesAttribute()
        {
        }

        /// <summary>
        /// The name of the property used as a key to eliminate duplicates
        /// </summary>
        public string PropertyName { get; }
    }
}