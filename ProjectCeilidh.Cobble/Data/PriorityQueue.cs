using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectCeilidh.Cobble.Data
{
    /// <summary>
    /// Implements a priority queue using a min heap.
    /// </summary>
    /// <typeparam name="TKey">The type of the key to use when sorting.</typeparam>
    /// <typeparam name="TValue">The type of the value associated with a key.</typeparam>
    internal class PriorityQueue<TKey, TValue> where TKey : IComparable<TKey>
    {
        /// <summary>
        /// The ammount the internal queue array should grow on reaching the size limit.
        /// </summary>
        private const int GrowSize = 10;

        /// <summary>
        /// The number of items in the priority queue.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// The array used to store queue items.
        /// </summary>
        private (TKey Key, TValue Value)[] _items;

        /// <summary>
        /// Construct an empty <see cref="PriorityQueue{TKey,TValue}"/>.
        /// </summary>
        /// <inheritdoc />
        public PriorityQueue() : this(Enumerable.Empty<(TKey, TValue)>())
        {

        }

        /// <summary>
        /// Construct a <see cref="PriorityQueue{TKey,TValue}"/> from an initial set of key-value pairs.
        /// </summary>
        /// <param name="initialValues">The values to populate the queue with.</param>
        /// <inheritdoc />
        public PriorityQueue(IEnumerable<KeyValuePair<TKey, TValue>> initialValues) : this(
            initialValues.Select(x => (x.Key, x.Value)))
        {

        }

        /// <summary>
        /// Construct a <see cref="PriorityQueue{TKey,TValue}"/> from an initial set of key-value pairs.
        /// </summary>
        /// <param name="initialValues">The values to populate the queue with.</param>
        public PriorityQueue(IEnumerable<(TKey, TValue)> initialValues)
        {
            var data = initialValues.ToList();
            _items = new (TKey, TValue)[data.Count];

            foreach (var (key, value) in data)
                Insert(key, value);
        }

        /// <summary>
        /// Try to remove the minimum element from the queue.
        /// </summary>
        /// <param name="key">The key of the retrieved element.</param>
        /// <param name="value">The value of the retrieved element.</param>
        /// <returns>True if the queue was not empty and an element was removed, false otherwise.</returns>
        public bool TryPop(out TKey key, out TValue value)
        {
            lock(this)
                switch (Count)
                {
                    case 0:
                        key = default;
                        value = default;
                        return false;
                    case 1:
                        Count--;
                        (key, value) = _items[0];
                        return true;
                    default:
                        (key, value) = _items[0];
                        _items[0] = _items[Count - 1];
                        Count--;
                        Heapify(0);

                        return true;
                }
        }

        /// <summary>
        /// Try to view the minimum element from the queue without removing it.
        /// </summary>
        /// <param name="key">The key of the retrieved element.</param>
        /// <param name="value">The value of the retrieved element.</param>
        /// <returns>True if the queue was not empty and an element was returned, false otherwise.</returns>
        public bool TryPeek(out TKey key, out TValue value)
        {
            lock(this)
                switch (Count)
                {
                    case 0:
                        key = default;
                        value = default;
                        return false;
                    default:
                        (key, value) = _items[0];
                        return true;
                }
        }

        /// <summary>
        /// Decrease the key associated with a given value.
        /// </summary>
        /// <param name="value">The value associated with the key to change.</param>
        /// <param name="transform">A function which transforms the key to a new value.</param>
        public void DecreaseKey(TValue value, Func<TKey, TKey> transform)
        {
            lock (this)
            {
                var idx = Array.FindIndex(_items, x => x.Value.Equals(value));
                if (idx < 0) throw new KeyNotFoundException();
                var key = transform(_items[idx].Key);

                if (_items[idx].Key.CompareTo(key) < 0) throw new ArgumentOutOfRangeException(nameof(key));

                _items[idx] = (key, value);
                for (; idx != 0 && _items[Parent(idx)].Key.CompareTo(_items[idx].Key) > 0; idx = Parent(idx))
                    Swap(ref _items[idx], ref _items[Parent(idx)]);
            }
        }

        /// <summary>
        /// Insert a new value into the queue.
        /// </summary>
        /// <param name="key">The key associated with the new value.</param>
        /// <param name="value">The new value to insert.</param>
        public void Insert(TKey key, TValue value)
        {
            lock (this)
            {
                if (Count == _items.Length)
                    Array.Resize(ref _items, _items.Length + GrowSize);

                Count++;
                var i = Count - 1;
                _items[i] = (key, value);

                while (i != 0 && _items[Parent(i)].Key.CompareTo(_items[i].Key) > 0)
                {
                    Swap(ref _items[i], ref _items[Parent(i)]);
                    i = Parent(i);
                }
            }
        }

        private void Heapify(int i)
        {
            lock(this)
                while (true)
                {
                    var l = Left(i);
                    var r = Right(i);
                    var smallest = i;
                    if (l < Count && _items[l].Key.CompareTo(_items[i].Key) < 0) smallest = l;
                    if (r < Count && _items[r].Key.CompareTo(_items[smallest].Key) < 0) smallest = r;

                    if (smallest == i) return;
                    Swap(ref _items[i], ref _items[smallest]);
                    i = smallest;
                }
        }

        private static int Parent(int i) => (i - 1) / 2;
        private static int Left(int i) => 2 * i + 1;
        private static int Right(int i) => 2 * i + 2;

        private static void Swap<T>(ref T a, ref T b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }
    }
}
