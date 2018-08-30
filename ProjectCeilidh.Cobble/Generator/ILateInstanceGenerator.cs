using System;
using System.Collections.Generic;

namespace ProjectCeilidh.Cobble.Generator
{
    /// <summary>
    /// Describes an instance generator which supports <see cref="ILateInject{T}"/>
    /// </summary>
    /// <inheritdoc />
    public interface ILateInstanceGenerator : IInstanceGenerator
    {
        /// <summary>
        /// All the dependencies that can be late injected.
        /// </summary>
        IEnumerable<Type> LateDependencies { get; }
    }
}
