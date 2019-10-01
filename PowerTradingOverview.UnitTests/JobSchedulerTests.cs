using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PowerTradingOverview.UnitTests
{
    [TestFixture]
    public class JobSchedulerTests
    {
        private static readonly TimeSpan _waitTimeout = TimeSpan.FromMinutes(1);
        private static readonly DateTime _startTime = new DateTime(2019, 09, 24, 11, 40, 00);

        private IJob _job;
        private IJobSchedule _jobSchedule;
        private ITimeProvider _timeProvider;
        private JobScheduler _jobScheduler;

        [SetUp]
        public void SetUp()
        {
            _job = Mock.Of<IJob>();
            _jobSchedule = Mock.Of<IJobSchedule>();
            _timeProvider = Mock.Of<ITimeProvider>();
            _jobScheduler = new JobScheduler(_jobSchedule, _job, _timeProvider);
        }

        [TearDown]
        public void TearDown()
        {
            _jobScheduler.Dispose();
        }

        [TestCase]
        public void ShouldRunFirstJobOnStart()
        {
            var jobRunEvent = new AutoResetEvent(false);
            SetCurrentTime(_startTime, TimeSpan.FromSeconds(5));
            SetNextTime(_startTime.AddHours(1), TimeSpan.FromSeconds(5));
            SetJobExecuteCallback(() => { jobRunEvent.Set(); return Task.CompletedTask; });

            StartAndStopScheduler(jobRunEvent);

            Mock.Get(_job).Verify(x => x.ExecuteAsync(), Times.Once());
            Mock.Get(_jobSchedule).Verify(x => x.GetNextTime(It.IsAny<DateTime>()), Times.Once());
        }

        [TestCase]
        public void ShouldNotFailOnFirstJobFailSync()
        {
            var jobRunEvent = new AutoResetEvent(false);
            SetCurrentTime(_startTime, TimeSpan.FromSeconds(5));
            SetNextTime(_startTime.AddSeconds(5), TimeSpan.FromSeconds(5));
            SetJobExecuteCallback(
                () => throw new ArgumentNullException(),
                () => { jobRunEvent.Set(); return Task.CompletedTask; });

            StartAndStopScheduler(jobRunEvent);

            Mock.Get(_job).Verify(x => x.ExecuteAsync(), Times.Exactly(2));
        }

        [TestCase]
        public void ShouldNotFailOnFirstJobFailAsync()
        {
            var jobRunEvent = new AutoResetEvent(false);
            SetCurrentTime(_startTime, TimeSpan.FromSeconds(5));
            SetNextTime(_startTime.AddSeconds(5), TimeSpan.FromSeconds(5));
            SetJobExecuteCallback(
                () => Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(x => throw new ArgumentException()),
                () => { jobRunEvent.Set(); return Task.CompletedTask; });

            StartAndStopScheduler(jobRunEvent);

            Mock.Get(_job).Verify(x => x.ExecuteAsync(), Times.Exactly(2));
        }

        [TestCase]
        public void ShouldNotFailOnSecondJobFailSync()
        {
            var jobRunEvent = new AutoResetEvent(false);
            SetCurrentTime(_startTime, TimeSpan.FromSeconds(5));
            SetNextTime(_startTime.AddSeconds(5), TimeSpan.FromSeconds(5));
            SetJobExecuteCallback(
                () => Task.CompletedTask,
                () => throw new ArgumentNullException(),
                () => { jobRunEvent.Set(); return Task.CompletedTask; });

            StartAndStopScheduler(jobRunEvent);

            Mock.Get(_job).Verify(x => x.ExecuteAsync(), Times.Exactly(3));
        }

        [TestCase]
        public void ShouldNotFailOnSecondJobFailAsync()
        {
            var jobRunEvent = new AutoResetEvent(false);
            SetCurrentTime(_startTime, TimeSpan.FromSeconds(5));
            SetNextTime(_startTime.AddSeconds(5), TimeSpan.FromSeconds(5));
            SetJobExecuteCallback(
                () => Task.CompletedTask,
                () => Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(x => throw new ArgumentException()),
                () => { jobRunEvent.Set(); return Task.CompletedTask; });

            StartAndStopScheduler(jobRunEvent);

            Mock.Get(_job).Verify(x => x.ExecuteAsync(), Times.Exactly(3));
        }

        [TestCase]
        public void ShouldRunThreeJobs()
        {
            var jobRunEvent = new AutoResetEvent(false);
            SetCurrentTime(_startTime, TimeSpan.FromSeconds(5));
            SetNextTime(_startTime.AddSeconds(5), TimeSpan.FromSeconds(5));
            SetJobExecuteCallback(
                () => Task.Delay(TimeSpan.FromSeconds(1)),
                () => Task.Delay(TimeSpan.FromSeconds(1)),
                () => { jobRunEvent.Set(); return Task.CompletedTask; });

            StartAndStopScheduler(jobRunEvent);

            Mock.Get(_job).Verify(x => x.ExecuteAsync(), Times.Exactly(3));
        }

        [TestCase]
        public void ShouldWaitForFirstLongRunningJob()
        {
            var jobRunEvent = new AutoResetEvent(false);
            var longRunningTask = Task.CompletedTask;
            SetCurrentTime(_startTime, TimeSpan.FromSeconds(5));
            SetNextTime(_startTime.AddSeconds(5), TimeSpan.FromSeconds(5));
            SetJobExecuteCallback(() => { jobRunEvent.Set(); return longRunningTask = Task.Delay(TimeSpan.FromSeconds(30)); });

            StartAndStopScheduler(jobRunEvent);

            Assert.IsTrue(longRunningTask.IsCompleted);
        }

        [TestCase]
        public void ShouldWaitForSecondLongRunningJob()
        {
            var jobRunEvent = new AutoResetEvent(false);
            var longRunningTask = Task.CompletedTask;
            SetCurrentTime(_startTime, TimeSpan.FromSeconds(5));
            SetNextTime(_startTime.AddSeconds(5), TimeSpan.FromSeconds(5));
            SetJobExecuteCallback(
                () => Task.Delay(TimeSpan.FromSeconds(1)),
                () => { jobRunEvent.Set(); return longRunningTask = Task.Delay(TimeSpan.FromSeconds(30)); });

            StartAndStopScheduler(jobRunEvent);

            Assert.IsTrue(longRunningTask.IsCompleted);
        }

        private void StartAndStopScheduler(WaitHandle stopEvent)
        {
            _jobScheduler.Start();
            Assert.IsTrue(stopEvent.WaitOne(_waitTimeout));
            _jobScheduler.Stop();
        }

        private void SetCurrentTime(DateTime startTime, TimeSpan interval)
        {
            var currentTime = startTime;
            Mock.Get(_timeProvider).Setup(x => x.GetCurrentTime())
                .Returns(() => currentTime += interval);
        }

        private void SetNextTime(DateTime startTime, TimeSpan interval)
        {
            var currentTime = startTime;
            Mock.Get(_jobSchedule).Setup(x => x.GetNextTime(It.IsAny<DateTime>()))
                .Returns(() => currentTime += interval);
        }

        private void SetJobExecuteCallback(params Func<Task>[] callbacks)
        {
            int counter = -1;

            Mock.Get(_job).Setup(x => x.ExecuteAsync()).Returns(() =>
            {
                counter++;
                if (counter < callbacks.Length)
                    return callbacks[counter]();

                return Task.CompletedTask;
            });
        }
    }
}
