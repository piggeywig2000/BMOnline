using System;

namespace BMOnline.Common
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan Multiply(this TimeSpan timeSpan, double factor)
        {
            return new TimeSpan((long)(timeSpan.Ticks * factor));
        }

        public static TimeSpan Divide(this TimeSpan timeSpan, double divisor)
        {
            return new TimeSpan((long)(timeSpan.Ticks / divisor));
        }

        public static double Divide(this TimeSpan thisTimeSpan, TimeSpan timeSpan)
        {
            return (double)thisTimeSpan.Ticks / (double)timeSpan.Ticks;
        }
    }
}
