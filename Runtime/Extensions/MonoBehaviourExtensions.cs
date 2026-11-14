using System;
using System.Collections;
using UnityEngine;

namespace com.Plapamaru.Shared.Extensions
{
    public static class MonoBehaviourExtensions
    {
        public static Coroutine Wait(this MonoBehaviour behaviour, float delay, Action onComplete)
        {
            return Start(behaviour, WaitRoutine(delay, onComplete));
        }

        public static Coroutine EndOfFrame(this MonoBehaviour behaviour, Action onComplete)
        {
            return Start(behaviour, EndOfFrameRoutine(onComplete));
        }

        public static Coroutine WaitForFrames(this MonoBehaviour behaviour, int frames, Action onComplete)
        {
            return Start(behaviour, WaitForFramesRoutine(frames, onComplete));
        }

        public static Coroutine ExecuteForNextFrames(this MonoBehaviour behaviour, int frames, Action action, Action onComplete = null)
        {
            return Start(behaviour, ExecuteForNextFramesRoutine(frames, action, onComplete));
        }

        public static Coroutine WaitUntil(this MonoBehaviour behaviour, Func<bool> predicate, Action onSuccess, Action onFail = null, float timeout = 10f)
        {
            return Start(behaviour, WaitUntilRoutine(predicate, onSuccess, onFail, timeout));
        }

        public static Coroutine LoopAction(this MonoBehaviour behaviour, Action action, float interval = 1f)
        {
            return Start(behaviour, LoopActionRoutine(action, interval));
        }

        public static Coroutine ExecuteUntil(this MonoBehaviour behaviour, Func<bool> condition, Action action, Action onComplete = null)
        {
            return Start(behaviour, ExecuteUntilRoutine(condition, action, onComplete));
        }

        private static Coroutine Start(MonoBehaviour behaviour, IEnumerator routine)
        {
            return behaviour != null ? behaviour.StartCoroutine(routine) : null;
        }

        private static IEnumerator WaitRoutine(float delay, Action onComplete)
        {
            if (delay <= 0f)
                yield return null;
            else
                yield return new WaitForSeconds(delay);
            onComplete?.Invoke();
        }

        private static IEnumerator EndOfFrameRoutine(Action onComplete)
        {
            yield return new WaitForEndOfFrame();
            onComplete?.Invoke();
        }

        private static IEnumerator WaitForFramesRoutine(int frames, Action onComplete)
        {
            for (int i = 0; i < frames; i++)
                yield return null;
            onComplete?.Invoke();
        }

        private static IEnumerator ExecuteForNextFramesRoutine(int frames, Action action, Action onComplete)
        {
            for (int i = 0; i < frames; i++)
            {
                action?.Invoke();
                yield return null;
            }
            onComplete?.Invoke();
        }

        private static IEnumerator WaitUntilRoutine(Func<bool> predicate, Action onSuccess, Action onFail, float timeout)
        {
            while (!predicate())
            {
                yield return null;
                timeout -= Time.deltaTime;
                if (timeout < 0f)
                {
                    onFail?.Invoke();
                    yield break;
                }
            }
            onSuccess?.Invoke();
        }

        private static IEnumerator LoopActionRoutine(Action action, float interval)
        {
            var wait = new WaitForSeconds(interval);
            while (true)
            {
                action?.Invoke();
                yield return wait;
            }
        }

        private static IEnumerator ExecuteUntilRoutine(Func<bool> condition, Action action, Action onComplete)
        {
            while (!condition())
            {
                action?.Invoke();
                yield return null;
            }
            onComplete?.Invoke();
        }
    }
}
