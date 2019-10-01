using System.Threading.Tasks;

namespace PowerTradingOverview
{
    /// <summary>
    /// Interface represents abstract report writer. It could be csv, pdf, xls or any other.
    /// </summary>
    public interface IReportWriter
    {
        Task WriteReportAsync(TradingReport report);
    }
}
