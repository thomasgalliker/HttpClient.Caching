using System;

namespace WpfSample.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToSecondsString(this TimeSpan timeSpan)
        {
            return $"{timeSpan.TotalSeconds:0.0000}s";
        }
    }
}