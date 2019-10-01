using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradingPlatform;

namespace PowerTradingOverview.UnitTests
{
    [TestFixture]
    public class ReportExtractJobTests
    {
        private ITimeProvider _timeProvider;
        private IReportBuilder _reportBuilder;
        private IReportWriter _reportWriter;
        private TradingService _tradingService;
        private IJob _job;

        [SetUp]
        public void SetUp()
        {
            _timeProvider = Mock.Of<ITimeProvider>();
            _reportBuilder = Mock.Of<IReportBuilder>();
            _reportWriter = Mock.Of<IReportWriter>();
            _tradingService = Mock.Of<TradingService>();

            _job = new ReportExtractJob(
                _tradingService, 
                _reportBuilder, 
                _reportWriter, 
                _timeProvider);
        }

        [TestCase]
        public void ShouldPassCurrentTimeAndTradesToReportBuilder()
        {
            var dateTime = new DateTime(2019, 09, 25, 13, 45, 37);
            Mock.Get(_timeProvider).Setup(x => x.GetCurrentTime()).Returns(dateTime);
            _job.ExecuteAsync().Wait();
            Mock.Get(_reportBuilder).Verify(x => x.PrepareReport(dateTime, It.IsAny<IEnumerable<Trade>>()), Times.Once());
        }

        [TestCase]
        public void ShouldPassReportToReportWriter()
        {
            var dateTime = new DateTime(2019, 09, 25, 13, 45, 37);
            Mock.Get(_timeProvider).Setup(x => x.GetCurrentTime()).Returns(dateTime);

            var report = new TradingReport(new DateTime(2019, 09, 25, 13, 45, 37), new TradingSummary[0]);
            Mock.Get(_reportBuilder)
                .Setup(x => x.PrepareReport(It.IsAny<DateTime>(), It.IsAny<IEnumerable<Trade>>()))
                .Returns(report);

            _job.ExecuteAsync().Wait();
            Mock.Get(_reportWriter).Verify(x => x.WriteReportAsync(report), Times.Once);
        }

        [TestCase]
        public void ShouldWaitUntilReportWriteIsCompleted()
        {
            var dateTime = new DateTime(2019, 09, 25, 13, 45, 37);
            Mock.Get(_timeProvider).Setup(x => x.GetCurrentTime()).Returns(dateTime);

            var writeTask = new Task(() => Thread.Sleep(TimeSpan.FromSeconds(5)));
            Mock.Get(_reportWriter)
                .Setup(x => x.WriteReportAsync(It.IsAny<TradingReport>()))
                .Returns(() => { writeTask.Start(); return writeTask; });
            _job.ExecuteAsync().Wait();
            Assert.IsTrue(writeTask.IsCompleted);
        }
    }
}
