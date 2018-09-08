using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task ParallelTopologicalSort()
        {
            var graph = new DirectedGraph<int>(Enumerable.Range(0, 5));
            graph.Link(0, 1);
            graph.Link(2, 1);
            graph.Link(1, 3);
            graph.Link(4, 3);

            var queue = new ConcurrentQueue<int>();

            await graph.ParallelTopologicalSort(x => queue.Enqueue(x));

            Assert.Collection(queue, value => Assert.True(value == 0 || value == 2 || value == 4), value => Assert.True(value == 0 || value == 2 || value == 4), value => Assert.True(value == 0 || value == 2 || value == 4 || value == 1), value => Assert.True(value == 4 || value == 1), x => Assert.Equal(3, x));
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
        public async Task ParallelCircularDependency()
        {
            var graph = new DirectedGraph<int>(Enumerable.Range(0, 5));
            graph.Link(0, 1);
            graph.Link(2, 1);
            graph.Link(1, 3);
            graph.Link(4, 3);
            graph.Link(3, 0);

            await Assert.ThrowsAsync<DirectedGraph<int>.CyclicGraphException>(async () => await graph.ParallelTopologicalSort(_ => { }));
        }
    }
}
