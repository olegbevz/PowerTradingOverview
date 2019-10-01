namespace PowerTradingOverview
{
    /// <summary>
    /// Trade period summary
    /// </summary>
    public class TradingSummary
    {
        public TradingSummary(int period, double volume)
        {
            Period = period;
            Volume = volume;
        }

        /// <summary>
        /// Number of trading period
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Aggragated volume for trading period
        /// </summary>
        public double Volume { get; set; }
    }
}
