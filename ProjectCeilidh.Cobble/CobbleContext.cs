using System.Collections.Generic;
using System;
using System.Linq;
using ProjectCeilidh.Cobble.Data;
using ProjectCeilidh.Cobble.Generator;

namespace ProjectCeilidh.Cobble
{
    /// <summary>
    /// An application context which allows for dependency graph construction and object creation.
    /// </summary>
    public sealed class CobbleContext
    {
        public delegate object DuplicateResolutionHandler(Type dependencyType, object[] instances);

        public DuplicateResolutionHandler DuplicateResolver { get; set; }

        private bool _firstStage;

        private readonly List<IInstanceGenerator> _instanceGenerators;
        private readonly Dictionary<Type, HashSet<object>> _lateInjectInstances;
        private readonly Dictionary<Type, HashSet<object>> _implementations;

        /// <summary>
        /// Construct a new CobbleContext.
        /// </summary>
        public CobbleContext()
        {
            _instanceGenerators = new List<IInstanceGenerator>();
            _lateInjectInstances = new Dictionary<Type, HashSet<object>>();
            _implementations = new Dictionary<Type, HashSet<object>>();

            AddUnmanaged(this);
        }

        /// <summary>
        /// Attempt to get a single object which can be assigned to the specified type.
        /// </summary>
        /// <returns>True if the singleton was found, false otherwise.</returns>
        /// <param name="single">The produced singleton.</param>
        /// <typeparam name="T">The type to match the singleton against.</typeparam>
        public bool TryGetSingleton<T>(out T single)
        {
            single = default;

            if (!_implementations.TryGetValue(typeof(T), out var set) || set.Count != 1) return false;

            single = (T) set.First();
            return true;
        }

        /// <summary>
        /// Attempt to get every object which can be assigned to the specified type.
        /// </summary>
        /// <returns>True, if matching implementations could be found, false otherwise.</returns>
        /// <param name="implementations">The set of found implementations.</param>
        /// <typeparam name="T">The type to match implementations against.</typeparam>
        public bool TryGetImplementations<T>(out IEnumerable<T> implementations)
        {
            implementations = default;

            if (!_implementations.TryGetValue(typeof(T), out var set)) return false;

            implementations = set.Cast<T>();
            return true;
        }

        /// <summary>
        /// Add a new instance to the graph that will not be constructed by this CobbleContext.
        /// </summary>
        /// <param name="instance">The instance to add.</param>
        public void AddUnmanaged(object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            AddManaged(new BareLateInstanceGenerator(instance));
        }

        /// <summary>
        /// Add an instance to the graph that will be constructed by this CobbleContext.
        /// </summary>
        /// <typeparam name="T">The type to construct.</typeparam>
        /// <seealso cref="AddManaged(Type)"/>
        public void AddManaged<T>() => AddManaged(typeof(T));

        /// <summary>
        /// Add an instance to the graph that will be constructed by this CobbleContext;
        /// </summary>
        /// <param name="type">The type to construct.</param>
        /// <seealso cref="AddManaged{T}"/>
        public void AddManaged(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (type.GetConstructors().Length != 1)
                throw new ArgumentException("Managed types must have exactly one public constructor.", nameof(type));

            AddManaged(new TypeLateInstanceGenerator(type));
        }

        /// <summary>
        /// Add a managed generator to the CobbleContext.
        /// </summary>
        /// <param name="generator">An <see cref="IInstanceGenerator"/> which is responsible for constructing the object.</param>
        public void AddManaged(IInstanceGenerator generator)
        {
            if (generator == null) throw new ArgumentNullException(nameof(generator));

            if (!_firstStage) // If Execute has already been called, create an instance right away and call all UnitLoaded functions.
                _instanceGenerators.Add(generator);
            else
            {
                var inst = CreateInstance(generator, _implementations);
                PushInstanceProvides(generator, inst, _implementations);
                foreach(var prov in generator.Provides)
                {
                    if (!_lateInjectInstances.TryGetValue(prov, out var set)) continue;

                    foreach (var late in set)
                        late.GetType().GetMethod(nameof(ILateInject<object>.UnitLoaded), new[] { prov }).Invoke(late, new []{ inst });
                }
            }
        }

        /// <summary>
        /// Construct all managed objects, resolving dependencies. Circular dependencies existing in this stage will cause an exception.
        /// </summary>
        public void Execute()
        {
            if (_firstStage) throw new Exception("You cannot execute a CobbleContext twice.");

            _firstStage = true;

            // Create a lookup which maps provided type to the set off all generators that provide it.
            var implMap = _instanceGenerators
                .SelectMany(x => x.Provides.Select(y => (Type: y, Generator: x)))
                .ToLookup(x => x.Type, x => x.Generator);

            var graph = new DirectedGraph<IInstanceGenerator>(_instanceGenerators);

            foreach(var gen in _instanceGenerators) // Create links in the DirectedGraph between dependencies and the generators which provide them
            {
                foreach(var dep in gen.Dependencies)
                {
                    var depType = dep;

                    if (dep.IsConstructedGenericType && dep.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        depType = dep.GetGenericArguments()[0];

                    foreach (var impl in implMap[depType])
                        graph.Link(impl, gen);
                }
            }

            try
            {
                foreach (var gen in graph.TopologicalSort()) // Sort the dependency graph topologically - all dependencies should be satisfied by the time we get to each unit
                {
                    var inst = CreateInstance(gen, _implementations);

                    PushInstanceProvides(gen, inst, _implementations);

                    if (gen is ILateInstanceGenerator late) // If the generator supports late injection, we need to add it to our list
                    {
                        foreach (var lateDep in late.LateDependencies)
                        {
                            if (_lateInjectInstances.TryGetValue(lateDep, out var lateSet))
                                lateSet.Add(inst);
                            else
                                _lateInjectInstances[lateDep] = new HashSet<object>(new[] { inst });
                        }
                    }
                }
            }
            catch (DirectedGraph<IInstanceGenerator>.CyclicGraphException) {
                throw new CircularDependencyException();
            }
        }

        /// <summary>
        /// Given a generator and an instance, push it into the relevant provider dictionaries
        /// </summary>
        /// <param name="gen">The instance generator that produced the instance.</param>
        /// <param name="instance">The instance that was produced.</param>
        /// <param name="instances">A dictionary mapping provided types to a set of instances.</param>
        private static void PushInstanceProvides(IInstanceGenerator gen, object instance, Dictionary<Type, HashSet<object>> instances)
        {
            foreach (var prov in gen.Provides)
                if (instances.TryGetValue(prov, out var set))
                    set.Add(instance);
                else
                    instances[prov] = new HashSet<object>(new[] { instance });
        }

        /// <summary>
        /// Given a generator and the instance map, generate an object.
        /// </summary>
        /// <returns>The created object.</returns>
        /// <param name="gen">The generator instance.</param>
        /// <param name="instances">A dictionary mapping provided types to a set of instances.</param>
        private object CreateInstance(IInstanceGenerator gen, Dictionary<Type, HashSet<object>> instances)
        {
            var args = new object[gen.Dependencies.Count()];
            var i = 0;
            foreach (var dep in gen.Dependencies)
            {
                if (dep.IsConstructedGenericType && dep.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var depType = dep.GetGenericArguments()[0];

                    if (instances.TryGetValue(depType, out var set))
                    {
                        var arr = Array.CreateInstance(depType, set.Count);
                        Array.Copy(set.ToArray(), arr, set.Count);
                        args[i] = arr;
                    }
                    else
                        args[i] = Array.CreateInstance(depType, 0);
                }
                else
                {
                    if (instances[dep].Count <= 1)
                        args[i] = instances[dep].FirstOrDefault();
                    else
                    {
                        if (DuplicateResolver != null)
                            args[i] = DuplicateResolver(dep, instances[dep].ToArray());
                        else
                            throw new AmbiguousDependencyException(dep);
                    }
                }

                i++;
            }

            return gen.GenerateInstance(args);
        }
    }
}
