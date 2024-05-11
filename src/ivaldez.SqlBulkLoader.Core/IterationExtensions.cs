using System;
using System.Collections.Generic;
using System.Linq;

namespace ivaldez.Sql.SqlBulkLoader.Core
{
    public static class IterationExtensions
    {
        public static bool IsOver = false; 

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> enumerable, int batchSize)
        {
            IEnumerator<T> enumerator = enumerable.GetEnumerator();

            var start = true;
            while (true)
            {
                var batchAux = BatchAux<T>(enumerator, batchSize);

                if (IsOver) break;
                if (enumerator.Current == null && start == false) break;
                if (start) start = false;

                yield return batchAux;
            }
        }

        private static IEnumerable<T> BatchAux<T>(IEnumerator<T> enumerable, int batchSize)
        {
            var batchCount = batchSize;
            while (true)
            {
                var hasNextItem = enumerable.MoveNext();

                batchCount--;

                if (!hasNextItem)
                {
                    IsOver = true;
                    break;
                }

                yield return enumerable.Current;

                if (batchCount <= 0) break;
            }
        }
    }
}
