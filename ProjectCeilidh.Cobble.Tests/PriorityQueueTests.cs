using System;
using System.Linq;
using ProjectCeilidh.Cobble.Data;
using Xunit;

namespace ProjectCeilidh.Cobble.Tests
{
    public class PriorityQueueTests
    {
        [Fact]
        public void Order()
        {
            var rng = new Random();

            var data = Enumerable.Range(0, 100).Select(_ => rng.Next()).ToArray();
            var queue = new PriorityQueue<int, int>(data.Select(x => (x, x)));

            data = data.OrderBy(x => x).ToArray();

            foreach (var t in data)
            {
                Assert.True(queue.TryPop(out var key, out _));
                Assert.Equal(t, key);
            }
        }

        [Fact]
        public void DecreaseKey()
        {
            var queue = new PriorityQueue<int, int>(new []
            {
                (2, 2),
                (15, 15),
                (5, 5),
                (4, 4),
                (45, 45)
            });

            Assert.True(queue.TryPop(out var key, out _) && key == 2);
            Assert.True(queue.TryPeek(out key, out _) && key == 4);
            queue.DecreaseKey(45, _ => 3);
            Assert.True(queue.TryPeek(out key, out _) && key == 3);
        }
    }
}
