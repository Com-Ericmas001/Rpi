// From original: https://github.com/jbienzms/iot-devices/blob/master/Lib/Microsoft.IoT.DeviceCore/Scheduling/Scheduler.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Com.Ericmas001.Rpi.Gpio.Scheduling
{
    public class Scheduler
    {
        private class Subscription
        {
            public bool IsSuspended { get; set; }
            public ScheduleOptions Options { get; set; }
        }
        private class Lookup<T> : Dictionary<T, Subscription> { }

        private const uint DEFAULT_REPORT_INTERVAL = 500;

        private static Scheduler m_DefaultScheduler;

        /// <summary>
        /// Gets the default shared scheduler.
        /// </summary>
        public static Scheduler Default => m_DefaultScheduler ?? (m_DefaultScheduler = new Scheduler());

        private Lookup<ScheduledAsyncAction> m_AsyncSubscriptions;
        private CancellationTokenSource m_CancellationSource;
        private uint m_ReportInterval = DEFAULT_REPORT_INTERVAL;
        private Lookup<Action> m_Subscriptions;
        private Task m_UpdateTask;

        public Scheduler()
        {
            AutoStart = true;
        }


        private void EnsureMinReportInterval(uint interval)
        {
            // Get count
            int acount = (m_AsyncSubscriptions?.Count ?? 0);
            int scount = (m_Subscriptions?.Count ?? 0);

            // If only one subscriber, just use it. Otherwise make sure minimum
            m_ReportInterval = (acount + scount) == 1 ? interval : Math.Min(m_ReportInterval, interval);
        }

        private Subscription GetSubscription(ScheduledAsyncAction subscriber, bool throwIfMissing = true)
        {
            // Validate
            if (subscriber == null) throw new ArgumentNullException("subscriber");

            // Try to get the subscription
            Subscription sub = null;
            if ((m_AsyncSubscriptions == null) || (!m_AsyncSubscriptions.TryGetValue(subscriber, out sub)))
            {
                if (throwIfMissing)
                {
                    throw new InvalidOperationException("Subscription not found");
                }
            }
            return sub;
        }

        private Subscription GetSubscription(Action subscriber, bool throwIfMissing = true)
        {
            // Validate
            if (subscriber == null) throw new ArgumentNullException("subscriber");

            // Try to get the subscription
            Subscription sub = null;
            if ((m_Subscriptions == null) || (!m_Subscriptions.TryGetValue(subscriber, out sub)))
            {
                if (throwIfMissing)
                {
                    throw new InvalidOperationException("Subscription not found");
                }
            }
            return sub;
        }

        private void QueryStart()
        {
            if (AutoStart)
            {
                Start();
            }
        }

        private void QueryStop()
        {
            if ((m_AsyncSubscriptions == null) || (m_AsyncSubscriptions.Count == 0))
            {
                if ((m_Subscriptions == null) || (m_Subscriptions.Count == 0))
                {
                    Stop();
                }
            }
        }

        private void RecalcReportInterval()
        {
            uint asyncMin = DEFAULT_REPORT_INTERVAL;
            uint syncMin = DEFAULT_REPORT_INTERVAL;

            if ((m_AsyncSubscriptions != null) && (m_AsyncSubscriptions.Count > 0))
            {
                lock (m_AsyncSubscriptions)
                {
                    uint? newMin = m_AsyncSubscriptions.Values.Where((s) => !s.IsSuspended).Min((s) => (uint?)s.Options.UpdateInterval);
                    if (newMin.HasValue) { asyncMin = newMin.Value; }
                }
            }

            if ((m_Subscriptions != null) && (m_Subscriptions.Count > 0))
            {
                lock (m_Subscriptions)
                {
                    uint? newMin = m_Subscriptions.Values.Where((s) => !s.IsSuspended).Min((s) => (uint?)s.Options.UpdateInterval);
                    if (newMin.HasValue) { syncMin = newMin.Value; }
                }
            }

            m_ReportInterval = Math.Min(asyncMin, syncMin);
        }

        private Task StartUpdateLoopAsync()
        {
            return Task.Run(async () =>
            {
                // int logCount=0;

                while (!m_CancellationSource.IsCancellationRequested)
                {
                    // Capture start time
                    var loopStart = DateTime.Now;

                    // Placeholder for task that represents all async tasks
                    Task asyncWhenAll = null;

                    // PHASE 1: START all asynchronous subscribers running
                    if ((m_AsyncSubscriptions != null) && (m_AsyncSubscriptions.Count > 0))
                    {
                        // What to schedule
                        var actions = new List<Task>();

                        // Thread safe
                        lock (m_AsyncSubscriptions)
                        {
                            // Look at each subscription
                            foreach (var sub in m_AsyncSubscriptions)
                            {
                                // If not suspended
                                if (!sub.Value.IsSuspended)
                                {
                                    // Add to list of things to schedule (as a task)
                                    actions.Add(new Task(() => sub.Key()));
                                }
                            }
                        }

                        // Actually schedule
                        asyncWhenAll = Task.WhenAll(actions);
                    }

                    // PHASE 2: RUN all synchronous subscribers
                    if (m_Subscriptions != null)
                    {
                        // Thread safe
                        lock (m_Subscriptions)
                        {
                            // Look at each subscription
                            foreach (var sub in m_Subscriptions)
                            {
                                // If not suspended
                                if (!sub.Value.IsSuspended)
                                {
                                    // Execute synchronously
                                    sub.Key();
                                }
                            }
                        }
                    }

                    // PHASE 3: WAIT for asynchronous subscribers to finish
                    if (asyncWhenAll != null)
                    {
                        await asyncWhenAll;
                    }

                    // How much time did the loop take?
                    var loopTime = (DateTime.Now - loopStart).TotalMilliseconds;

                    //if (logCount++ % 20 == 0)
                    //{
                    //    Debug.WriteLine(string.Format("Loop Time: {0}", loopTime));
                    //}

                    // If there's any time left, give CPU back
                    if (loopTime < m_ReportInterval)
                    {
                        await Task.Delay((int)(m_ReportInterval - loopTime));
                    }
                }
            });
        }
        public void Resume(Action subscriber)
        {
            var s = GetSubscription(subscriber);
            s.IsSuspended = false;
            EnsureMinReportInterval(s.Options.UpdateInterval);
        }

        public void Resume(ScheduledAsyncAction subscriber)
        {
            var s = GetSubscription(subscriber);
            s.IsSuspended = false;
            EnsureMinReportInterval(s.Options.UpdateInterval);
        }

        public void Schedule(Action subscriber, ScheduleOptions options)
        {
            // Check for existing subscription
            var sub = GetSubscription(subscriber, false);
            if (sub != null) { throw new InvalidOperationException("Already subscribed"); }

            // Make sure lookup exists
            if (m_Subscriptions == null) { m_Subscriptions = new Lookup<Action>(); }

            // Threadsafe
            lock (m_Subscriptions)
            {
                // Add lookup
                m_Subscriptions[subscriber] = new Subscription() { Options = options };
            }

            // Ensure interval
            EnsureMinReportInterval(options.UpdateInterval);

            // Start?
            QueryStart();
        }

        public void Schedule(ScheduledAsyncAction subscriber, ScheduleOptions options)
        {
            // Check for existing subscription
            var sub = GetSubscription(subscriber, false);
            if (sub != null) { throw new InvalidOperationException("Already subscribed"); }

            // Make sure lookup exists
            if (m_AsyncSubscriptions == null) { m_AsyncSubscriptions = new Lookup<ScheduledAsyncAction>(); }

            // Threadsafe
            lock (m_AsyncSubscriptions)
            {
                // Add lookup
                m_AsyncSubscriptions[subscriber] = new Subscription() { Options = options };
            }

            // Ensure interval
            EnsureMinReportInterval(options.UpdateInterval);

            // Start?
            QueryStart();
        }

        public void Start()
        {
            // If already running, ignore
            if (IsRunning) { return; }

            // Create (or rest) the cancellation source
            m_CancellationSource = new CancellationTokenSource();

            // Start the loop
            m_UpdateTask = StartUpdateLoopAsync();//.FailFastOnException();
        }

        public void Stop()
        {
            // If not running, ignore
            if (!IsRunning) { return; }

            // Set cancel flag
            m_CancellationSource.Cancel();

            // Wait for loop to complete
            m_UpdateTask.Wait();

            // Clear variables
            m_UpdateTask = null;
            m_CancellationSource = null;
        }

        public void Suspend(Action subscriber)
        {
            GetSubscription(subscriber).IsSuspended = true;
            RecalcReportInterval();
        }

        public void Suspend(ScheduledAsyncAction subscriber)
        {
            GetSubscription(subscriber).IsSuspended = true;
            RecalcReportInterval();
        }

        public void Unschedule(Action subscriber)
        {
            if (m_Subscriptions != null)
            {
                lock (m_Subscriptions)
                {
                    m_Subscriptions.Remove(subscriber); // Unschedule
                }
            }

            // See if we should stop
            QueryStop();

            // Recalcualte the report interval
            RecalcReportInterval();
        }

        public void Unschedule(ScheduledAsyncAction subscriber)
        {
            if (m_AsyncSubscriptions != null)
            {
                lock (m_AsyncSubscriptions)
                {
                    m_AsyncSubscriptions.Remove(subscriber); // Unschedule
                }
            }

            // See if we should stop
            QueryStop();

            // Recalcualte the report interval
            RecalcReportInterval();
        }

        public void UpdateSchedule(Action subscriber, ScheduleOptions options)
        {
            GetSubscription(subscriber).Options = options ?? throw new ArgumentNullException("options");
            if (m_ReportInterval < options.UpdateInterval)
            {
                EnsureMinReportInterval(options.UpdateInterval);
            }
            else
            {
                RecalcReportInterval();
            }
        }

        public void UpdateSchedule(ScheduledAsyncAction subscriber, ScheduleOptions options)
        {
            GetSubscription(subscriber).Options = options ?? throw new ArgumentNullException("options");
            if (m_ReportInterval < options.UpdateInterval)
            {
                EnsureMinReportInterval(options.UpdateInterval);
            }
            else
            {
                RecalcReportInterval();
            }
        }
        public bool AutoStart { get; set; }

        public bool IsRunning => m_UpdateTask != null;
    }
}
