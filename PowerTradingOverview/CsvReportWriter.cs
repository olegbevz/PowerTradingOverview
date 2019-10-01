using log4net;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PowerTradingOverview
{
    /// <summary>
    /// Class implements csv reports writing logic
    /// </summary>
    public class CsvReportWriter : IReportWriter
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(JobScheduler));
        private static readonly string _defaultDirectory = Path.GetDirectoryName(typeof(CsvReportWriter).Assembly.Location);

        private readonly string _fileNamePattern;
        private readonly string _directory;

        public CsvReportWriter(string fileNamePattern, string directory)
        {
            _fileNamePattern = fileNamePattern ?? throw new ArgumentNullException(nameof(fileNamePattern));
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _directory = Path.Combine(_defaultDirectory, _directory);
        }

        public async Task WriteReportAsync(TradingReport report)
        {
            if (!Directory.Exists(_directory)) Directory.CreateDirectory(_directory);

            var fileName = Path.Combine(_directory, string.Format(_fileNamePattern, report.ReportTime));

            _logger.Debug($"Writing trading report to {fileName}...");

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Date,Periods,Volume");

            var firstRow = true;
            foreach (var summary in report.Trades)
            {
                if (firstRow) stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0:dd-MMM-yyyy}", report.ReportTime);
                var volume = summary.Volume.ToString(CultureInfo.InvariantCulture);
                stringBuilder.AppendLine($",{summary.Period},{volume}");
                firstRow = false;
            }

            var fileContent = stringBuilder.ToString();

            using (var fileStream = File.Open(fileName, FileMode.CreateNew, FileAccess.Write))
            using (var streamWriter = new StreamWriter(fileStream))
            {
                await streamWriter.WriteAsync(fileContent);
                await streamWriter.FlushAsync();
            }

            _logger.Debug($"Trading report was written.");
        }
    }
}
