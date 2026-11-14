using System;
using Cysharp.Threading.Tasks;

namespace com.Plapamaru.Shared.UI
{
    public class UILoadingOverlay : UIView
    {
        private int _count;

        public int Count => _count;

        public IDisposable Begin()
        {
            _count++;
            if (_count == 1)
                Show().Forget();

            return new Scope(this);
        }

        protected internal override void OnBack()
        {
        }

        private void End()
        {
            if (_count == 0)
                return;

            _count--;
            if (_count == 0)
                Hide().Forget();
        }

        private sealed class Scope : IDisposable
        {
            private UILoadingOverlay _owner;

            public Scope(UILoadingOverlay owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                if (_owner == null)
                    return;

                var owner = _owner;
                _owner = null;
                if (owner != null)
                    owner.End();
            }
        }
    }
}
