﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectCeilidh.Cobble.Data
{
    /// <summary>
    /// Represents a directed graph
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    internal class DirectedGraph<TNode>
    {
        private readonly HashSet<TNode> _initialNodes;
        private readonly Dictionary<TNode, int> _nodes;
        private readonly Dictionary<TNode, HashSet<TNode>> _outgoingEdges;

        /// <summary>
        /// Construct a directed graph with no nodes.
        /// </summary>
        public DirectedGraph() : this(Enumerable.Empty<TNode>())
        {
        }

        /// <summary>
        /// Construct a directed graph with the specified initial nodes.
        /// </summary>
        /// <param name="initialNodes">Initial nodes.</param>
        public DirectedGraph(IEnumerable<TNode> initialNodes)
        {
            _initialNodes = new HashSet<TNode>(initialNodes);
            _nodes = _initialNodes.ToDictionary(x => x, x => 0);
            _outgoingEdges = new Dictionary<TNode, HashSet<TNode>>();
        }

        /// <summary>
        /// Add a new node to the graph.
        /// </summary>
        /// <returns>True if the node was added, false otherwise.</returns>
        /// <param name="node">The node to add.</param>
        public void Add(TNode node)
        {
            _nodes[node] = 0;
            _initialNodes.Add(node);
        }

        /// <summary>
        /// Add a link from <paramref name="src"/> to <paramref name="dst"/>.
        /// </summary>
        /// <param name="src">The start of the link.</param>
        /// <param name="dst">The end of the link.</param>
        public void Link(TNode src, TNode dst)
        {
            if (!_outgoingEdges.TryGetValue(src, out var outSet))
                outSet = _outgoingEdges[src] = new HashSet<TNode>();

            outSet.Add(dst);
            _nodes[dst]++;
            _initialNodes.Remove(dst);
        }

        /// <summary>
        /// Topologically sort the graph, excepting if a cycle is detected.
        /// </summary>
        /// <returns>A sequence of nodes in topological order.</returns>
        /// <remarks>
        /// Implements Kahn's algorithm for topological sorting, without removing edges.
        /// This saves a copy operation and allows the sort to be executed multiple times.
        /// </remarks>
        public IEnumerable<TNode> TopologicalSort()
        {
            var refDict = new Dictionary<TNode, int>(_nodes);

            var list = new LinkedList<TNode>(_initialNodes);

            while (list.Count > 0)
            {
                var node = list.First.Value;
                list.RemoveFirst();

                yield return node;

                foreach (var target in _outgoingEdges.TryGetValue(node, out var set) ? set : Enumerable.Empty<TNode>())
                {
                    var con = --refDict[target];
                    if (con == 0) list.AddLast(target);
                }
            }

            var remaining = refDict.Where(x => x.Value > 0).ToList();

            if (remaining.Count > 0)
                throw new CyclicGraphException(remaining.Select(x => x.Key));
        }

        /// <summary>
        /// Topologically sort the graph, invoking the callback in parallel where possible
        /// </summary>
        /// <param name="callback">The callback to invoke</param>
        /// <returns>A task following the parallel sort</returns>
        public async Task ParallelTopologicalSort(Action<TNode> callback)
        {
            var refDict = new ConcurrentDictionary<TNode, int>(_nodes);

            await Task.WhenAll(_initialNodes.Select(HandleItem));

            var remaining = refDict.Where(x => x.Value != 0).ToList();

            if (remaining.Count > 0)
                throw new CyclicGraphException(remaining.Select(x => x.Key));

            async Task HandleItem(TNode node)
            {
                await Task.Run(() => callback(node));

                if (_outgoingEdges.TryGetValue(node, out var set))
                    await Task.WhenAll(set.Select(async target =>
                    {
                        var con = refDict.AddOrUpdate(target, 0, (a, b) => b - 1);

                        if (con == 0) await HandleItem(target);
                    }));
            }
        }

        public class CyclicGraphException : Exception
        {
            public readonly IReadOnlyList<TNode> ExtraNodes;

            public CyclicGraphException(IEnumerable<TNode> extraNodes) : base("Encountered a cycle while attempting to topologically sort a directed graph.")
            {
                ExtraNodes = extraNodes.ToList();
            }
        }
    }
}
