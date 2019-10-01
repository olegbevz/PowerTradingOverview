using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TradingPlatform;

namespace PowerTradingOverview.UnitTests
{
    [TestFixture]
    public class ReportBuilderTests
    {
        private readonly IReportBuilder _reportBuilder = new ReportBuilder();

        [TestCase]
        public void ShouldPassCurrentTimeAsReportTime()
        {
            var currentTime = new DateTime(2019, 09, 22, 10, 48, 00);
            var report = _reportBuilder.PrepareReport(currentTime, Array.Empty<Trade>());

            Assert.AreEqual(currentTime, report.ReportTime);
        }

        [TestCase]
        public void ShouldFailIfNullTrades()
        {
            Assert.Throws<ArgumentNullException>(() => _reportBuilder.PrepareReport(new DateTime(2019, 09, 22, 11, 48, 00), null));
        }

        [TestCase]
        public void ShouldConvertEmptyTrades()
        {
            var report = _reportBuilder.PrepareReport(new DateTime(2019, 09, 22, 11, 48, 00), Array.Empty<Trade>());

            Assert.IsEmpty(report.Trades);
        }

        [TestCase]
        public void ShouldConvertSingleTradeWithPeriods()
        {
            var currentTime = new DateTime(2019, 09, 22, 11, 48, 00);
            var trades = new Trade[]
            {
                CreateTrade(currentTime, 1, 100, 2, 200, 3, 125, 5, 200, 6, 500)
            };

            var report = _reportBuilder.PrepareReport(currentTime, trades);
            report.Should().BeEquivalentTo(CreateReport(currentTime, 1, 100, 2, 200, 3, 125, 5, 200, 6, 500));
        }

        [TestCase]
        public void ShouldConvertTradesWithEqualPeriodAmounts()
        {
            var currentTime = new DateTime(2019, 09, 22, 11, 48, 00);
            var trades = new Trade[]
            {
                CreateTrade(currentTime, 1, 100, 2, 200, 3, 125, 4, 200, 5, 200),
                CreateTrade(currentTime, 1, 50, 2, 250, 3, 100, 4, 150, 5, 300),
                CreateTrade(currentTime, 1, 125, 2, 100, 3, 125, 4, 200, 5, 100)
            };

            var report = _reportBuilder.PrepareReport(currentTime, trades);
            report.Should().BeEquivalentTo(CreateReport(currentTime, 1, 275, 2, 550, 3, 350, 4, 550, 5, 600));
        }

        [TestCase]
        public void ShouldConvertTradesWithNotEqualPeriodsAmount()
        {
            var currentTime = new DateTime(2019, 09, 22, 11, 48, 00);
            var trades = new Trade[]
            {
                CreateTrade(currentTime, 1, 100, 2, 200, 3, 125, 4, 200),
                CreateTrade(currentTime, 2, 50, 3, 250, 4, 100, 5, 150),
                CreateTrade(currentTime, 1, 125, 2, 100, 5, 125, 6, 200)
            };


            var report = _reportBuilder.PrepareReport(currentTime, trades);
            report.Should().BeEquivalentTo(CreateReport(currentTime, 1, 225, 2, 350, 3, 375, 4, 300, 5, 275, 6, 200));
        }

        private static Trade CreateTrade(DateTime dateTime, params double[] arguments)
        {
            if (arguments == null || arguments.Length % 2 != 0) throw new ArgumentException();

            var trade = Trade.Create(dateTime, arguments.Length / 2);
            var currentPeriod = 0;
            for (var i = 0; i < arguments.Length - 1; i+=2)
            {
                trade.Periods[currentPeriod++] = new TradingPeriod { Period = (int)arguments[i], Volume = arguments[i + 1] };
            }

            return trade;
        }

        private static TradingReport CreateReport(DateTime dateTime, params double[] arguments)
        {
            if (arguments == null || arguments.Length % 2 != 0) throw new ArgumentException();

            var tradingSummaries = new List<TradingSummary>();
            for (var i = 0; i < arguments.Length - 1; i += 2)
            {
                tradingSummaries.Add(new TradingSummary((int)arguments[i], arguments[i + 1]));
            }

            return new TradingReport(dateTime, tradingSummaries);
        }
    }
}
