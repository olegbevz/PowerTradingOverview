using System.Threading.Tasks;

namespace PowerTradingOverview
{
    /// <summary>
    /// Interface represent a single work item for job scheduler
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Execute job asynchronously
        /// </summary>
        Task ExecuteAsync();
    }
}
