using System;

namespace PowerTradingOverview
{
    /// <summary>
    /// Implementation of time provider which uses time zone.
    /// In current application time zone is "GMT Standard Time", but it could be any other
    /// </summary>
    public class TimeZoneTimeProvider : ITimeProvider
    {
        private readonly TimeZoneInfo _timeZone;

        public TimeZoneTimeProvider(string timeZone)
        {
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        }

        public DateTime GetCurrentTime()
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZone);
        }
    }
}
