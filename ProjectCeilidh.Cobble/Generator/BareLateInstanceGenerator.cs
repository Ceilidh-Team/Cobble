using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectCeilidh.Cobble.Generator
{
    /// <inheritdoc />
    /// <summary>
    /// Can provide an instance and dependency list from a pre-created instance.
    /// </summary>
    public class BareLateInstanceGenerator : ILateInstanceGenerator
    {
        public IEnumerable<Type> LateDependencies { get; }
        public IEnumerable<Type> Provides { get; }
        public IEnumerable<Type> Dependencies => Enumerable.Empty<Type>();

        private readonly object _instance;

        public BareLateInstanceGenerator(object instance)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));

            var type = _instance.GetType();
            LateDependencies = type.GetInterfaces().Where(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(ILateInject<>)).Select(x => x.GetGenericArguments()[0]).ToArray();
            Provides = type.GetAssignableFrom().ToArray();
        }

        public object GenerateInstance(object[] args) => _instance;
    }
}
