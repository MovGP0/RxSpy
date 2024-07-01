using Google.Protobuf.WellKnownTypes;

namespace RxSpy.Models;

public static class TimestampExtensions
{
    public static TimeSpan ToTimeSpan(this Timestamp timestamp)
    {
        long seconds = timestamp.Seconds;
        long nanoSeconds = timestamp.Nanos;
        long milliseconds = (seconds * 1000) + (nanoSeconds / 1_000);
        return TimeSpan.FromMilliseconds(milliseconds);
    }
}