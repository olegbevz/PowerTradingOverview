using System;
using System.Collections.Generic;
using TradingPlatform;

namespace PowerTradingOverview
{
    /// <summary>
    /// Interface represents logic of converting trades from TradingPlatform to TradingReport
    /// </summary>
    public interface IReportBuilder
    {
        /// <summary>
        /// Get TradingReport model
        /// </summary>
        /// <param name="currentTime">Time of report extraction</param>
        /// <param name="trades">Trades returned from TradingPlatform</param>
        /// <returns>Trading Report model</returns>
        TradingReport PrepareReport(DateTime currentTime, IEnumerable<Trade> trades);
    }
}
