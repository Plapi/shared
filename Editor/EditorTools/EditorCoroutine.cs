using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.Plapamaru.Shared.EditorTools
{
    public class EditorCoroutine
    {
        private readonly Stack<IEnumerator> _stack = new Stack<IEnumerator>();

        public bool IsRunning { get; private set; }

        public static EditorCoroutine Start(IEnumerator routine)
        {
            var coroutine = new EditorCoroutine(routine);
            coroutine.IsRunning = true;
            EditorApplication.update += coroutine.Update;
            return coroutine;
        }

        public static IEnumerator WaitForSeconds(double seconds)
        {
            double end = EditorApplication.timeSinceStartup + seconds;
            while (EditorApplication.timeSinceStartup < end)
                yield return null;
        }

        private EditorCoroutine(IEnumerator routine)
        {
            _stack.Push(routine);
        }

        public void Stop()
        {
            if (!IsRunning)
                return;
            IsRunning = false;
            EditorApplication.update -= Update;
        }

        private void Update()
        {
            try
            {
                IEnumerator current = _stack.Peek();
                if (!current.MoveNext())
                {
                    _stack.Pop();
                    if (_stack.Count == 0)
                        Stop();
                    return;
                }

                if (current.Current is IEnumerator nested)
                    _stack.Push(nested);
            }
            catch (Exception e)
            {
                Stop();
                Debug.LogException(e);
            }
        }
    }
}
