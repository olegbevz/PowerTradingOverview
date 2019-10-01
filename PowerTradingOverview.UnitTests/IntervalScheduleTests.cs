using NUnit.Framework;
using System;

namespace PowerTradingOverview.UnitTests
{
    [TestFixture]
    public class IntervalScheduleTests
    {
        private readonly IJobSchedule _jobSchedule = new IntervalSchedule(
            new DateTime(2019, 09, 22, 10, 34, 54), 
            TimeSpan.FromHours(1));

        [TestCase]
        public void ShouldFailIfCurrentTimeIsBeforeStartTime()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _jobSchedule.GetNextTime(new DateTime(2019, 09, 22, 10, 33, 54)));
        }

        [TestCase]
        public void ShouldReturnStartTimePlusIntervalIfCurrentTimeIsStartTime()
        {
            Assert.AreEqual(
                new DateTime(2019, 09, 22, 11, 34, 54),
                _jobSchedule.GetNextTime(new DateTime(2019, 09, 22, 10, 34, 54)));
        }

        [TestCase]
        public void ShouldReturnNextTimeIfCurrentTimeIfBeforeNextTime()
        {
            Assert.AreEqual(
                new DateTime(2019, 09, 22, 11, 34, 54),
                _jobSchedule.GetNextTime(new DateTime(2019, 09, 22, 11, 04, 32)));
        }

        [TestCase]
        public void ShouldReturnNextTimePlusIntervalIfCurrentTimeIsNextTime()
        {
            Assert.AreEqual(
                new DateTime(2019, 09, 22, 12, 34, 54),
                _jobSchedule.GetNextTime(new DateTime(2019, 09, 22, 11, 34, 54)));
        }

        [TestCase]
        public void ShouldReturnNextTimeIfCurrentTimeIsAfterPreviousTime()
        {
            Assert.AreEqual(
                new DateTime(2019, 09, 22, 12, 34, 54),
                _jobSchedule.GetNextTime(new DateTime(2019, 09, 22, 12, 14, 54)));
        }

        [TestCase]
        public void ShouldFailIfNextTimeIsAfterMaxDate()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new IntervalSchedule(DateTime.MinValue, TimeSpan.FromMinutes(47))
                .GetNextTime(DateTime.MaxValue));
        }

        [TestCase]
        public void ShouldThrowArgumentOutOfRangeExceptionIfNextTimeIsMaxDate()
        {
            var schedule = new IntervalSchedule(DateTime.MinValue, TimeSpan.FromMinutes(1));
            Assert.Throws<ArgumentOutOfRangeException>(() => schedule.GetNextTime(DateTime.MaxValue));
        }

        [TestCase]
        public void RealTestCase()
        {
            var schedule = new IntervalSchedule(new DateTime(2019, 09, 25, 2, 1, 57, 662), TimeSpan.FromSeconds(1));
            //var lastCurrentTime = new DateTime(637049908146627031);
            //var lastNextTime = schedule.GetNextTime(lastCurrentTime);
            var curretTime = new DateTime(637049908156619372);
            var nextTime = schedule.GetNextTime(curretTime);

            var expectedNextTime = new DateTime(2019, 09, 25, 6, 46, 55, 662);
            Assert.AreEqual(expectedNextTime, nextTime);
        }
    }
}
