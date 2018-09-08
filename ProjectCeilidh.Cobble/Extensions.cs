using System;
using System.Collections.Generic;

namespace ProjectCeilidh.Cobble
{
    internal static class Extensions
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key,
            out TValue value)
        {
            key = pair.Key;
            value = pair.Value;
        }

        /// <summary>
        /// Produce a sequence by recursively calling the specified function.
        /// </summary>
        /// <returns>The union of all sets produced.</returns>
        /// <param name="value">The base value.</param>
        /// <param name="transform">A function which performs one unrolling step.</param>
        /// <typeparam name="T">The type of the object to unroll.</typeparam>
        public static IEnumerable<T> Unroll<T>(this T value, Func<T, IEnumerable<T>> transform)
        {
            if (transform == null) throw new ArgumentNullException(nameof(transform));

            var stack = new Stack<T>(new[] { value });
            while (stack.Count > 0)
            {
                foreach(var i in transform(stack.Pop()))
                {
                    stack.Push(i);
                    yield return i;
                }
            }
        }

        public static IEnumerable<Type> GetAssignableFrom(this Type type)
        {
            foreach (var intf in type.GetInterfaces())
                yield return intf;

            yield return type;

            var b = type.BaseType;

            while (b != null && b != typeof(object))
            {
                b = b.BaseType;
                yield return b;
            }
        }
    }
}
