using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace com.Plapamaru.Shared.Async
{
    public static class UniTaskRace
    {
        public static async UniTask<T> Race<T>(
            IReadOnlyList<T> entries,
            Func<T, CancellationToken, UniTask> wait,
            CancellationToken cancellationToken)
        {
            using var raceCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var tasks = new UniTask[entries.Count];
            try
            {
                for (int i = 0; i < tasks.Length; i++)
                    tasks[i] = wait(entries[i], raceCts.Token);

                int winner = await UniTask.WhenAny(tasks);
                return entries[winner];
            }
            finally
            {
                raceCts.Cancel();
            }
        }
    }
}
