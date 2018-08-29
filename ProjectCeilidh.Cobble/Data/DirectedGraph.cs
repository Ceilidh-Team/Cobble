using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectCeilidh.Cobble.Data
{
    /// <summary>
    /// Represents a directed graph
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    internal class DirectedGraph<TNode>
    {
        private readonly HashSet<TNode> _nodes;
        private readonly Dictionary<TNode, HashSet<TNode>> _incomingEdges, _outgoingEdges;

        public DirectedGraph() : this(Enumerable.Empty<TNode>())
        {

        }

        public DirectedGraph(IEnumerable<TNode> initialNodes)
        {
            _nodes = new HashSet<TNode>(initialNodes);

            _incomingEdges = new Dictionary<TNode, HashSet<TNode>>();
            _outgoingEdges = new Dictionary<TNode, HashSet<TNode>>();
        }

        public void Link(TNode src, TNode dst)
        {
            if (!_incomingEdges.TryGetValue(dst, out var inSet))
                inSet = _incomingEdges[dst] = new HashSet<TNode>();

            inSet.Add(src);

            if (!_outgoingEdges.TryGetValue(src, out var outSet))
                outSet = _outgoingEdges[src] = new HashSet<TNode>();

            outSet.Add(dst);
        }

        public IEnumerable<TNode> TopologicalSort()
        {
            var queue = new PriorityQueue<int, TNode>(_nodes.Select(x => (_incomingEdges.TryGetValue(x, out var set) ? set.Count : 0, x)));

            while (queue.TryPop(out var incoming, out var node))
            {
                if (incoming != 0) throw new Exception("Circular dependency"); // TODO: Real exception data

                if (_outgoingEdges.TryGetValue(node, out var outSet))
                    foreach (var dst in outSet)
                    {
                        queue.DecreaseKey(dst, x => x - 1);

                        if (_incomingEdges.TryGetValue(dst, out var inSet))
                            inSet.Remove(node);
                    }

                yield return node;
            }
        }
    }
}
