using System;

namespace com.Plapamaru.Shared.Utilities
{
    public static class EnumUtils
    {
        public static T[] GetValues<T>() where T : struct, Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static void ForEach<T>(Action<T> action) where T : struct, Enum
        {
            foreach (T value in GetValues<T>())
                action(value);
        }

        public static void ForEach<T>(Action<T, int> action) where T : struct, Enum
        {
            T[] values = GetValues<T>();
            for (int i = 0; i < values.Length; i++)
                action(values[i], i);
        }
    }
}
