using System.Threading.Tasks;
using TradingPlatform;

namespace PowerTradingOverview
{
    /// <summary>
    /// Class represents report extraction job
    /// </summary>
    public class ReportExtractJob : IJob
    {
        private readonly TradingService _tradingService;
        private readonly IReportBuilder _reportBuilder;
        private readonly IReportWriter _reportService;
        private readonly ITimeProvider _timeProvider;

        public ReportExtractJob(
            TradingService tradingService,
            IReportBuilder reportBuilder,
            IReportWriter reportService,
            ITimeProvider timeProvider)
        {
            _tradingService = tradingService;
            _reportService = reportService;
            _timeProvider = timeProvider;
            _reportBuilder = reportBuilder;
        }

        public async Task ExecuteAsync()
        {
            // Get current time (report extraction time)
            var currentTime = _timeProvider.GetCurrentTime();

            // Request trades from trading platform asynchronously (to not block scheduler thread)
            var trades = await _tradingService.GetTradesAsync(currentTime);

            // Prepare report model from received trades (in separate thread)
            var tradeReport = _reportBuilder.PrepareReport(currentTime, trades);

            // Write report to csv file
            await _reportService.WriteReportAsync(tradeReport);
        }
    }
}
