using UnityEngine;

namespace com.Plapamaru.Shared.Utilities
{
    public readonly struct PosAndRot
    {
        public readonly Vector3 position;
        public readonly Quaternion rotation;

        public PosAndRot(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public PosAndRot(Transform transform) : this(transform.position, transform.rotation) { }

        public void ApplyTo(Transform transform)
        {
            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
