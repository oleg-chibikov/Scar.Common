using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Scar.Common
{
    public static class EnumerableExtensions
    {
        public static void RunByBlocks<T>([NotNull] this IEnumerable<T> items, int maxBlockSize, [NotNull] Func<T[], int, int, bool> action)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var objArray = items as T[] ?? items.ToArray();
            if (objArray.Length == 0)
            {
                return;
            }

            if (maxBlockSize <= 0)
            {
                maxBlockSize = 100;
            }

            var num = objArray.Length / maxBlockSize;
            if (objArray.Length % maxBlockSize > 0)
            {
                ++num;
            }

            for (var index = 0; index < num; ++index)
            {
                var array = objArray.Skip(index * maxBlockSize).Take(maxBlockSize).ToArray();
                if (array.Length == 0 || !action(array, index, num))
                {
                    break;
                }
            }
        }

        public static async Task RunByBlocksAsync<T>([NotNull] this IEnumerable<T> items, int maxBlockSize, [NotNull] Func<T[], int, int, Task<bool>> action)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var objArray = items as T[] ?? items.ToArray();
            if (objArray.Length == 0)
            {
                return;
            }

            if (maxBlockSize <= 0)
            {
                maxBlockSize = 100;
            }

            var num = objArray.Length / maxBlockSize;
            if (objArray.Length % maxBlockSize > 0)
            {
                ++num;
            }

            for (var index = 0; index < num; ++index)
            {
                var array = objArray.Skip(index * maxBlockSize).Take(maxBlockSize).ToArray();
                if (array.Length == 0 || !await action(array, index, num))
                {
                    break;
                }
            }
        }

        [ItemCanBeNull]
        [NotNull]
        public static IEnumerable<T> WithoutLast<T>([NotNull] this IEnumerable<T> source)
        {
            using (var e = source.GetEnumerator())
            {
                if (!e.MoveNext())
                {
                    yield break;
                }

                for (var value = e.Current; e.MoveNext(); value = e.Current)
                {
                    yield return value;
                }
            }
        }
    }
}