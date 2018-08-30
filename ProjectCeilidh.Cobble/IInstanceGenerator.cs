using System;
using System.Collections.Generic;

namespace ProjectCeilidh.Cobble
{
    public interface IInstanceGenerator
    {
        IEnumerable<Type> Provides { get; }
        IEnumerable<Type> Dependencies { get; }
        object GenerateInstance(object[] args);
    }

    public interface ILateInstanceGenerator : IInstanceGenerator
    {
        IEnumerable<Type> LateDependencies { get; }
    }
}
