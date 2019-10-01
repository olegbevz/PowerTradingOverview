using log4net;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PowerTradingOverview
{
    public class JobScheduler : IDisposable
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(JobScheduler));
        private static readonly TimeSpan _waitDeviation = TimeSpan.FromMilliseconds(200);

        private readonly ManualResetEventSlim _stopSchedulerEvent = new ManualResetEventSlim(false);
        private readonly TimeSpan _stopTimeout = TimeSpan.FromMinutes(5);
        private readonly Thread _schedulerThread;

        private readonly IJobSchedule _jobSchedule;
        private readonly IJob _job;
        private readonly ITimeProvider _timeProvider;
        public JobScheduler(IJobSchedule jobSchedule, IJob job, ITimeProvider timeProvider)
        {
            _logger.Info("JobScheduler constructor was called");

            _jobSchedule = jobSchedule ?? throw new ArgumentNullException(nameof(jobSchedule));
            _job = job ?? throw new ArgumentNullException(nameof(job));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

            _schedulerThread = new Thread(SchedulerThread) { IsBackground = true };
        }

        /// <summary>
        /// Finalizer will be called is Dispose() method had not been run.
        /// Here we dispose ManualResetEventSlim (despite it already has its own finalizer).
        /// </summary>
        ~JobScheduler()
        {
            Dispose();
        }

        public void Start()
        {
            // Reset stop scheduler event to nonsignaled state
            _stopSchedulerEvent.Reset();
            // Start scheduler thread
            _schedulerThread.Start();

            _logger.Info("JobScheduler was started");
        }

        public void Stop()
        {       
            // Signal to scheduler thread that stop was triggered 
            _stopSchedulerEvent.Set();
            
            // Wait for scheduler thread to finish with timeout
            if (!_schedulerThread.Join(_stopTimeout))
                throw new TimeoutException("Waiting for JobScheduler thread to complete failed with timeout");

            _logger.Info("JobScheduler was stopped");
        }

        /// <summary>
        /// Method represents scheduler thread
        /// </summary>
        private void SchedulerThread()
        {
            _logger.Info("JobScheduler thread was started");
            _logger.Debug(nameof(SynchronizationContext) + ": " + SynchronizationContext.Current ?? "null");
            _logger.Debug(nameof(TaskScheduler) + ": " + TaskScheduler.Current);

            // Here we use collection to store active tasks. 
            // No need to use concurrent collection here cause it will be accessed from one thread
            var activeTasks = new Queue<Task>();

            try
            {
                // Executed job first time
                activeTasks.Enqueue(RunJob());
            }
            catch (Exception ex)
            {
                _logger.Error("JobScheduler loop failed.", ex);
            }

            do
            {
                try
                {
                    // Remove finished tasks from collection
                    RemoveCompletedTasks(activeTasks);

                    // Get time left before the next job
                    var leftTime = GetLeftTimeBeforeNextJob();
                    
                    _logger.Debug($"Waiting {leftTime} for the next job ...");

                    // Wait for the next job time
                    if (!_stopSchedulerEvent.Wait(leftTime))
                    {
                        // Execute job if stop event was not raised 
                        activeTasks.Enqueue(RunJob());
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("JobScheduler loop failed.", ex);
                }

            } while (!_stopSchedulerEvent.Wait(TimeSpan.Zero));

            // If stop event was raised and we still have uncompleted tasks then wait for it with timeout
            if (activeTasks.Count > 0)
            {                
                _logger.Info("Waiting for active tasks to be finished...");
                if (!Task.WaitAll(activeTasks.ToArray(), _stopTimeout))
                    throw new TimeoutException("Waiting for active tasks finish failed with timeout");
            }

            _logger.Info("JobScheduler thread is completed");
        }

        private TimeSpan GetLeftTimeBeforeNextJob()
        {
            var currentTime = _timeProvider.GetCurrentTime();
            _logger.Debug($"Current time is {currentTime}");

            var nextTime = _jobSchedule.GetNextTime(currentTime);
            _logger.Debug($"Next job time is {nextTime}");

            return nextTime - currentTime + _waitDeviation;
        }

        private Task RunJob()
        {
            _logger.Debug($"Executing {_job} ...");
            var task = _job.ExecuteAsync();
            task.ContinueWith(OnJobFault, TaskContinuationOptions.OnlyOnFaulted);
            task.ContinueWith(OnJobCompleted, TaskContinuationOptions.OnlyOnRanToCompletion);

            return task;
        }

        private void RemoveCompletedTasks(Queue<Task> activeTasks)
        {
            if (activeTasks.Count == 0) return;
            var activeTask = activeTasks.Peek();
            while (!IsTaskActive(activeTask) && activeTasks.Count > 0)
            {
                activeTasks.Dequeue();
                if (activeTasks.Count == 0) break;
                activeTask = activeTasks.Peek();
            }
        }

        private bool IsTaskActive(Task task)
        {
            return !task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
        }

        private void OnJobCompleted(Task completedTask)
        {
            _logger.Debug($"{_job} completed successfully");
        }

        private void OnJobFault(Task faultedTask)
        {
            var faultException = faultedTask.Exception.InnerException ?? faultedTask.Exception; 
            _logger.Error($"JobScheduler job failed with {faultException.GetType()}.", faultedTask.Exception);
        }

        /// <summary>
        /// JobScheduler dispose method is called by Topshelf on service stop/restart. 
        /// Here we dispose ManualResetEventSlim (despite it already has its own finalizer).
        /// </summary>
        public void Dispose()
        {
            if (_stopSchedulerEvent != null)
                _stopSchedulerEvent.Dispose();

            if (_logger != null)
                _logger.Info("JobScheduler was disposed");

            GC.SuppressFinalize(this);
        }
    }
}
