using System;
using System.Linq;
using Xunit;
using ProjectCeilidh.Cobble.Data;

namespace ProjectCeilidh.Cobble.Tests
{
    public class DirectedGraphTests
    {
        [Fact]
        public void TopologicalSort()
        {
            var graph = new DirectedGraph<int>(Enumerable.Range(0, 5));
            graph.Link(0, 1);
            graph.Link(2, 1);
            graph.Link(1, 3);
            graph.Link(4, 3);

            Assert.Collection(graph.TopologicalSort(), InitialInspector, InitialInspector, InitialInspector, x => Assert.Equal(1, x), x => Assert.Equal(3, x));

            void InitialInspector(int value)
            {
                Assert.True(value == 0 || value == 2 || value == 4);
            }
        }

        [Fact]
        public void ParallelTopologicalSort()
        {
            var graph = new DirectedGraph<int>(Enumerable.Range(0, 5));
            graph.Link(0, 1);
            graph.Link(2, 1);
            graph.Link(1, 3);
            graph.Link(4, 3);

            Assert.Collection(graph.ParallelTopologicalSort(), InitialInspector, x => Assert.Equal(new []{ 1 }, x), x => Assert.Equal(new []{ 3 }, x));

            void InitialInspector(int[] value)
            {
                Array.Sort(value);

                Assert.Equal(new []{ 0, 2, 4 }, value);
            }
        }

        [Fact]
        public void CircularDependency()
        {
            var graph = new DirectedGraph<int>(Enumerable.Range(0, 5));
            graph.Link(0, 1);
            graph.Link(2, 1);
            graph.Link(1, 3);
            graph.Link(4, 3);
            graph.Link(3, 0);
            
            Assert.Throws<DirectedGraph<int>.CyclicGraphException>(() => graph.TopologicalSort().ToList());
        }

        [Fact]
        public void ParallelCircularDependency()
        {
            var graph = new DirectedGraph<int>(Enumerable.Range(0, 5));
            graph.Link(0, 1);
            graph.Link(2, 1);
            graph.Link(1, 3);
            graph.Link(4, 3);
            graph.Link(3, 0);

            Assert.Throws<DirectedGraph<int>.CyclicGraphException>(() => graph.ParallelTopologicalSort().ToList());
        }
    }
}
