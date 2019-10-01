using System;
using System.Collections.Generic;
using System.Linq;
using TradingPlatform;

namespace PowerTradingOverview
{
    /// <summary>
    /// Class implements logic of transformation trades from TradingPlatform to report model
    /// </summary>
    public class ReportBuilder : IReportBuilder
    {
        public TradingReport PrepareReport(DateTime currentTime, IEnumerable<Trade> trades)
        {
            if (trades == null) throw new ArgumentNullException(nameof(trades));

            return new TradingReport(currentTime, GetTradeSummaries(trades));
        }

        public IEnumerable<TradingSummary> GetTradeSummaries(IEnumerable<Trade> trades)
        {
            // Here we use dictionary to accumulate trading periods volumes.
            // We could use a simple array to store accunulated volumes,
            // but in that case we need to be sure that TradingPlatform returns 24 periods for all trades every time
            // (of cause it is true for TradingPlatform.dll, but could not be true for production cases)
            var tradingSummary = new Dictionary<int, double>();

            foreach (var trade in trades)
            {
                if (trade.Periods == null) continue;

                foreach (var period in trade.Periods)
                {
                    if (tradingSummary.ContainsKey(period.Period))
                    {
                        tradingSummary[period.Period] += period.Volume;
                    }
                    else
                    {
                        tradingSummary.Add(period.Period, period.Volume);
                    }
                }
            }

            return tradingSummary.Select(x => new TradingSummary(x.Key, x.Value)).ToArray();
        }
    }
}
