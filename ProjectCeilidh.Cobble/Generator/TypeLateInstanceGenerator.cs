using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ProjectCeilidh.Cobble.Generator
{
    /// <inheritdoc />
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
            Dependencies = target.GetTypeInfo().DeclaredConstructors.Single().GetParameters().Select(x => x.ParameterType).ToArray();
            Provides = target.GetAssignableFrom().ToArray();
            LateDependencies = target.GetTypeInfo().ImplementedInterfaces.Where(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(ILateInject<>)).Select(x => x.GenericTypeArguments[0]).ToArray();
        }

        public object GenerateInstance(object[] args)
        {
            var ctor = _target.GetTypeInfo().DeclaredConstructors.Single();
            return ctor.Invoke(args);
        }

        public override string ToString() => $"TypeLateInstanceGenerator({_target.FullName})";
    }
}
