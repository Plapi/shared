using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace com.Plapamaru.Shared.Async
{
    public static class UniTaskRace
    {
        public static async UniTask<THandler> Race<THandler>(
            IEnumerable<KeyValuePair<Func<CancellationToken, UniTask>, THandler>> raceTasks,
            CancellationToken cancellationToken)
        {
            using var raceCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            List<(UniTask task, THandler handler)> tasks = CreateRaceTasks(raceTasks, raceCts.Token);
            return await WhenFirst(tasks, raceCts);
        }

        public static List<(UniTask task, THandler handler)> CreateRaceTasks<THandler>(
            IEnumerable<KeyValuePair<Func<CancellationToken, UniTask>, THandler>> raceTasks,
            CancellationToken cancellationToken)
        {
            var tasks = new List<(UniTask task, THandler handler)>();
            foreach (KeyValuePair<Func<CancellationToken, UniTask>, THandler> pair in raceTasks)
                tasks.Add((pair.Key(cancellationToken), pair.Value));
            return tasks;
        }

        public static async UniTask<THandler> WhenFirst<THandler>(
            IReadOnlyList<(UniTask task, THandler handler)> tasks,
            CancellationTokenSource raceCancellationTokenSource)
        {
            var array = new UniTask[tasks.Count];
            for (int i = 0; i < array.Length; i++)
                array[i] = tasks[i].task;

            try
            {
                int winner = await UniTask.WhenAny(array);
                return tasks[winner].handler;
            }
            finally
            {
                raceCancellationTokenSource.Cancel();
            }
        }
    }
}
