using System;
using UnityEngine;

namespace com.Plapamaru.Shared.Utilities
{
    public class Timer : MonoBehaviour
    {
        private float _interval;
        private float _elapsed;
        private Action _onTick;

        public static Timer Create(GameObject owner, float interval, Action onTick, bool tickImmediately = true)
        {
            Timer timer = owner.AddComponent<Timer>();
            timer._interval = interval;
            timer._onTick = onTick;
            if (tickImmediately)
                onTick();
            return timer;
        }

        public void Stop()
        {
            Destroy(this);
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed < _interval)
                return;

            _elapsed -= _interval;
            _onTick();
        }
    }
}
