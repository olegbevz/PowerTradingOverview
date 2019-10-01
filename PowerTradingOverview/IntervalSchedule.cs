using System;

namespace PowerTradingOverview
{
    /// <summary>
    /// Class implements interval schedule
    /// </summary>
    public class IntervalSchedule : IJobSchedule
    {
        public IntervalSchedule(DateTime startTime, TimeSpan interval)
        {
            StartTime = startTime;
            Interval = interval;
        }

        /// <summary>
        /// Initial schedule time
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// Schedule interval
        /// </summary>
        public TimeSpan Interval { get; private set; }

        /// <summary>
        /// Get the next scheduled time 
        /// </summary>
        /// <param name="currentTime">Current time</param>
        /// <returns>Next scheduled time</returns>
        public DateTime GetNextTime(DateTime currentTime)
        {
            var timePassed = currentTime - StartTime;
            if (timePassed < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("Current time should not be before start time");

            if (timePassed == TimeSpan.Zero)
                return StartTime + Interval;

            var timeAfterLastSchedule = TimeSpan.FromTicks(timePassed.Ticks % Interval.Ticks);
            if (timeAfterLastSchedule == TimeSpan.Zero)
                return currentTime + Interval;

            return currentTime - timeAfterLastSchedule + Interval; 
        }
    }
}
