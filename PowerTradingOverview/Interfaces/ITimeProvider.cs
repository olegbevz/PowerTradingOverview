using System;

namespace PowerTradingOverview
{
    /// <summary>
    /// Interface represents abstraction under current time
    /// Could be used for time dependent unit tests
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// Get current time
        /// </summary>
        /// <returns>Current time</returns>
        DateTime GetCurrentTime();
    }
}
