using System;
using System.Globalization;

namespace com.Plapamaru.Shared.Utilities
{
    [Serializable]
    public class SerializedDateTime
    {
        public long timestamp;

        public SerializedDateTime() { }

        public SerializedDateTime(DateTime date)
        {
            Date = date;
        }

        public DateTime Date
        {
            get => new(timestamp);
            set => timestamp = value.Ticks;
        }

        public override string ToString()
        {
            return Date.ToString(CultureInfo.InvariantCulture);
        }
    }
}
