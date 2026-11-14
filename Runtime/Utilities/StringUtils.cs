using UnityEngine.Networking;

namespace com.Plapamaru.Shared.Utilities
{
    public static class StringUtils
    {
        public static string FormatCompact(long value)
        {
            long abs = value < 0 ? -value : value;
            return abs switch
            {
                >= 1_000_000_000 => (value / 1_000_000_000D).ToString("0.#") + "B",
                >= 1_000_000 => (value / 1_000_000D).ToString("0.#") + "M",
                >= 1_000 => (value / 1_000D).ToString("0.#") + "K",
                _ => value.ToString("N0")
            };
        }

        public static string EscapeURL(string text)
        {
            return UnityWebRequest.EscapeURL(text).Replace("+", "%20");
        }
    }
}
