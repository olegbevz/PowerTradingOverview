using System;
using System.Collections.Generic;

namespace PowerTradingOverview
{
    /// <summary>
    /// Trading report model
    /// </summary>
    public class TradingReport
    {
        public TradingReport(DateTime reportTime, IEnumerable<TradingSummary> trades)
        {
            ReportTime = reportTime;
            Trades = trades ?? throw new ArgumentNullException(nameof(trades));
        }        

        /// <summary>
        /// Report extraction time
        /// </summary>
        public DateTime ReportTime { get; set; }

        /// <summary>
        /// List of report trades summaries 
        /// </summary>
        public IEnumerable<TradingSummary> Trades { get; set; }
    }
}
