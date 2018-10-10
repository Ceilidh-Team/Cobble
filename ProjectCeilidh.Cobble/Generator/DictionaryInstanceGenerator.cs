using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ProjectCeilidh.Cobble.Generator
{
    /// <inheritdoc />
    /// <summary>
    /// Generates an instance from a set of delegates.
    /// </summary>
    public class DictionaryInstanceGenerator : IInstanceGenerator
    {
        private static readonly MethodInfo DispatchProxyCreate = typeof(DispatchProxy).GetRuntimeMethod(nameof(DispatchProxy.Create), new Type[0]) ?? throw new Exception("Cannot find DispatchProxy.Create - this should be impossible.");

        public IEnumerable<Type> Provides { get; }
        public IEnumerable<Type> Dependencies { get; }

        private readonly Type _contractType;
        private readonly Delegate _ctor;
        private readonly IReadOnlyDictionary<MethodInfo, Delegate> _implementations;

        /// <summary>
        /// Construct an instance generator given the specified constructor and implementations.
        /// </summary>
        /// <param name="contractType">The base type that should be implemented.</param>
        /// <param name="ctor">The constructor called to inject dependencies. The value returned is passed as the first argument to all implementation delegates.</param>
        /// <param name="implementations">A dictionary containing all implemented functions as delegates.</param>
        public DictionaryInstanceGenerator(Type contractType, Delegate ctor, IReadOnlyDictionary<MethodInfo, Delegate> implementations)
        {
            Dependencies = ctor == null ? new Type[0] : ctor.GetMethodInfo().GetParameters().Select(x => x.ParameterType).ToArray();
            Provides = contractType.GetTypeInfo().ImplementedInterfaces.Concat(new []{ contractType }).Concat(contractType.Unroll(x => x.GetTypeInfo().BaseType == typeof(object) || x.GetTypeInfo().BaseType == null ? new Type[0] : new []{ x.GetTypeInfo().BaseType }));
            _contractType = contractType;
            _ctor = ctor;
            _implementations = implementations;
        }

        public object GenerateInstance(object[] args)
        {
            var proxy = DispatchProxyCreate.MakeGenericMethod(_contractType, typeof(DictionaryProxy)).Invoke(null, new object[0]);
            ((DictionaryProxy) proxy).Implementations = _implementations;
            ((DictionaryProxy) proxy).This = _ctor?.DynamicInvoke(args);
            return proxy;
        }

        public override string ToString() => $"DictionaryInstanceGenerator({_contractType.FullName})";

        /// <summary>
        /// Provides a way to implement the specified contract at runtime.
        /// </summary>
        /// <inheritdoc />
        public class DictionaryProxy : DispatchProxy
        {
            internal object This { get; set; }
            internal IReadOnlyDictionary<MethodInfo, Delegate> Implementations { get; set; }

            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                if (!Implementations.TryGetValue(targetMethod, out var impl)) throw new NotImplementedException();
                return impl.DynamicInvoke(new []{ This }.Concat(args).ToArray());
            }
        }
    }
}
