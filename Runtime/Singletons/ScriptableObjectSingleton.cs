using UnityEngine;

namespace com.Plapamaru.Shared.Singletons
{
    public abstract class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObjectSingleton<T>
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<T>(typeof(T).Name);
                    if (instance == null)
                    {
                        Debug.LogError($"No {typeof(T).Name} asset found. Create one named \"{typeof(T).Name}\" inside a Resources folder.");
                        return null;
                    }
                    instance.OnInitialize();
                }
                return instance;
            }
        }

        protected virtual void OnInitialize() { }
    }
}
