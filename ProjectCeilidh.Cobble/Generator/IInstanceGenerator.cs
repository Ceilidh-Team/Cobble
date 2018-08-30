using System;
using System.Collections.Generic;

namespace ProjectCeilidh.Cobble.Generator
{
    /// <summary>
    /// Describes a strategy for generating an instance that implements certain types and asks for certain types.
    /// </summary>
    public interface IInstanceGenerator
    {
        /// <summary>
        /// All the types that this instance will provide. A cast of the instance to any one of these must be possible.
        /// </summary>
        IEnumerable<Type> Provides { get; }
        /// <summary>
        /// All the dependency types this instance needs to be constructed.
        /// </summary>
        IEnumerable<Type> Dependencies { get; }
        /// <summary>
        /// Create a new instance given arguments that satisfy the dependencies.
        /// </summary>
        /// <param name="args">The arguments to be used when constructing.</param>
        /// <returns>The newly created instance.</returns>
        object GenerateInstance(object[] args);
    }
}