using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectCeilidh.Cobble.Generator
{
    /// <summary>
    /// Can produce an instance and set of dependencies given a type.
    /// </summary>
    public class TypeLateInstanceGenerator : ILateInstanceGenerator
    {
        public IEnumerable<Type> Provides { get; }
        public IEnumerable<Type> Dependencies { get; }
        public IEnumerable<Type> LateDependencies { get; }

        private readonly Type _target;

        public TypeLateInstanceGenerator(Type target)
        {
            _target = target;
            Dependencies = target.GetConstructors().Single().GetParameters().Select(x => x.ParameterType).ToArray();
            Provides = target.GetInterfaces().Concat(new[] { target }).Concat(target.Unroll(x => x.BaseType == typeof(object) || x.BaseType == null ? new Type[0] : new []{ x.BaseType })).ToArray();
            LateDependencies = target.GetInterfaces().Where(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(ILateInject<>)).Select(x => x.GetGenericArguments()[0]).ToArray();
        }

        public object GenerateInstance(object[] args)
        {
            var ctor = _target.GetConstructors().Single();
            return ctor.Invoke(args);
        }
    }
}
