using NUnit.Framework;
using PowerTradingOverview.Host;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace PowerTradingOverview.IntegrationTests
{
    [TestFixture]
    public class WindowsServiceTests
    {
        private readonly string _currentDirectory;
        private readonly string _outputDirectory;
        private readonly string _hostFileName;
        private readonly string _logFileName;
        private readonly string _serviceName;
        private readonly TopshelfServiceRunner _serviceRunner;
        private readonly ReportOptions _reportOptions;
        private readonly Regex _csvFileNameRegex;
        private readonly string _csvContentRegex;
        private readonly TimeSpan _reportTimeDeviation;
        private readonly TimeSpan _reportInterval;
        private readonly string _timeZone;

        public WindowsServiceTests()
        {
            _currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _outputDirectory = Path.Combine(_currentDirectory, "output");
            _hostFileName = Path.Combine(_currentDirectory, "PowerTradingOverview.Host.exe");
            _logFileName = Path.Combine(_currentDirectory, "PowerTradingOverview.log");
            _serviceName = "PowerTradingOverview$test";
            _serviceRunner = new TopshelfServiceRunner(_hostFileName, "test");
            _timeZone = "GMT Standard Time";
            _reportInterval = TimeSpan.FromSeconds(10);
            _reportOptions = new ReportOptions(
                _timeZone,
                "output",
                "PowerPosition_{0:yyyyMMdd_HHmmss}.csv",
                _reportInterval);
            _csvFileNameRegex = new Regex(@"PowerPosition_(\d{8}_\d{6}).csv");
            _reportTimeDeviation = TimeSpan.FromSeconds(5);
            _csvContentRegex = @"^Date,Periods,Volume(\n|\r|\r\n){0:dd-MMM-yyyy}(,\d+,\d+.\d+(\n|\r|\r\n))+$";
        }

        [TestCase]
        public void ShouldInstallService()
        {
            try
            {
                _serviceRunner.InstallService();

                var services = ServiceController.GetServices();
                var service = services.FirstOrDefault(x => x.ServiceName == _serviceName);

                Assert.NotNull(service);
                Assert.AreEqual(service.StartType, ServiceStartMode.Automatic);
                Assert.AreEqual(service.Status, ServiceControllerStatus.Stopped);
            }
            finally
            {
                _serviceRunner.UninstallService();
            }
        }

        [TestCase]
        public void ShouldUninstallService()
        {
            _serviceRunner.InstallService();
            _serviceRunner.UninstallService();

            var services = ServiceController.GetServices();
            var service = services.FirstOrDefault(x => x.ServiceName == _serviceName);

            Assert.IsNull(service);
        }

        [TestCase]
        public void ShouldWriteCsvReports()
        {
            try
            {
                ReportOptions.WriteToConfig(_reportOptions, _hostFileName);
                if (Directory.Exists(_outputDirectory)) Directory.Delete(_outputDirectory, true);
                if (File.Exists(_logFileName)) File.Delete(_logFileName);

                _serviceRunner.InstallService();

                var services = ServiceController.GetServices();
                var nextTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(_timeZone));

                var service = services.FirstOrDefault(x => x.ServiceName == _serviceName);                
                service.Start();
                Thread.Sleep(TimeSpan.FromSeconds(120));
                service.Stop();

                foreach (var filePath in Directory.Exists(_outputDirectory) ? Directory.EnumerateFiles(_outputDirectory) : new string[0])
                {
                    var fileName = Path.GetFileName(filePath);
                    var fileNameMatch = _csvFileNameRegex.Match(fileName);
                    Assert.IsTrue(fileNameMatch.Success, $"File name {fileName} is incorrect");

                    var reportDate = DateTime.ParseExact(fileNameMatch.Groups[1].Value, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);

                    while ((reportDate - nextTime).Duration() > _reportTimeDeviation)
                    {
                        nextTime += _reportInterval;

                        if (nextTime > reportDate + _reportInterval + _reportTimeDeviation)
                            Assert.Fail($"Report {fileName} has incorrect time.");
                    }

                    var regex = new Regex(string.Format(_csvContentRegex, reportDate));
                    var fileContent = File.ReadAllText(filePath);
                    Assert.IsTrue(regex.IsMatch(fileContent), $"File content {fileName} is incorrect");                    
                }

                Assert.IsTrue(File.Exists(_logFileName), "Log file was not written");

                using (var logReader = new Log4NetReader(_logFileName))
                {
                    var errorMessages = new StringBuilder();
                    foreach (var error in logReader.GetErrors())
                        if (!error.EndsWith("TradingPlatform.TradingServiceException."))
                            errorMessages.AppendLine(error);
                    if (errorMessages.Length > 0) Assert.Fail(errorMessages.ToString());
                }
            }
            finally
            {          
                _serviceRunner.UninstallService();
            }
        }
    }
}
