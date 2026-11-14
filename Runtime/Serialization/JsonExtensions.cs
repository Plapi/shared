using System.Globalization;
using Newtonsoft.Json;

namespace com.Plapamaru.Shared.Serialization
{
    public static class JsonExtensions
    {
        public static T AsModel<T>(this string json, params JsonConverter[] converters)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings(Formatting.None, converters));
        }

        public static string AsJson(this object value, params JsonConverter[] converters)
        {
            return JsonConvert.SerializeObject(value, Settings(Formatting.None, converters));
        }

        public static string AsPrettyJson(this object value, params JsonConverter[] converters)
        {
            return JsonConvert.SerializeObject(value, Settings(Formatting.Indented, converters));
        }

        private static JsonSerializerSettings Settings(Formatting formatting, JsonConverter[] converters)
        {
            return new JsonSerializerSettings
            {
                Converters = converters,
                Formatting = formatting,
                Culture = CultureInfo.InvariantCulture
            };
        }
    }
}
