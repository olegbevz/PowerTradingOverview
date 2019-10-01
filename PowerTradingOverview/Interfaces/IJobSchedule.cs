using System;

namespace PowerTradingOverview
{
    /// <summary>
    /// Interface implements abstact job schedule.
    /// It can be interval schedule, work hours schedule or any other 
    /// </summary>
    public interface IJobSchedule
    {
        /// <summary>
        /// Get the next scheduled time 
        /// </summary>
        /// <param name="currentTime">Current time</param>
        /// <returns>Next scheduled time</returns>
        DateTime GetNextTime(DateTime currentTime);
    }
}
