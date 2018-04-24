// From original: https://github.com/jbienzms/iot-devices/blob/master/Lib/Microsoft.IoT.DeviceCore/Devices/ScheduledUpdater.cs

using System;

namespace Com.Ericmas001.Rpi.Gpio.Scheduling
{
    public class ScheduledUpdater
    {
        private ScheduledAsyncAction m_AsyncUpdateAction;
        private readonly ScheduleOptions m_DefaultScheduleOptions;
        private uint m_EventsSubscribed;
        private bool m_IsStarted;
        private bool m_Scheduled;
        private Scheduler m_Scheduler;
        private ScheduleOptions m_ScheduleOptions;
        private Action m_UpdateAction;
        public ScheduledUpdater(ScheduleOptions scheduleOptions, Scheduler scheduler)
        {

            // Store
            m_Scheduler = scheduler ?? throw new ArgumentNullException("scheduler");
            m_ScheduleOptions = scheduleOptions ?? throw new ArgumentNullException("scheduleOptions");
            m_DefaultScheduleOptions = scheduleOptions;

            // Defaults
            StartWithEvents = true;
            StopWithEvents = true;
        }

        public ScheduledUpdater(ScheduleOptions scheduleOptions) : this(scheduleOptions, Scheduler.Default) { }

        private void SetUpdateInterval(uint newInterval)
        {
            // Create new options
            var options = ScheduleOptions.WithNewUpdateInterval(newInterval);

            // Update
            UpdateScheduleOptions(options);
        }

        private void UpdateScheduleOptions(ScheduleOptions options)
        {
            // Validate
            if (options == null) throw new ArgumentNullException("options");

            // Ensure changing
            if (options == m_ScheduleOptions) { return; }

            // Update variable first
            m_ScheduleOptions = options;

            // If current scheduled, update the schedule
            if (m_Scheduled)
            {
                if (m_AsyncUpdateAction != null)
                {
                    m_Scheduler.UpdateSchedule(m_AsyncUpdateAction, options);
                }
                else
                {
                    m_Scheduler.UpdateSchedule(m_UpdateAction, options);
                }
            }
        }

        private void ValidateUpdateAction()
        {
            if ((m_AsyncUpdateAction == null) && (m_UpdateAction == null))
            {
                throw new InvalidOperationException("The update action must first be specified by calling SetUpdateAction or SetAsyncUpdateAction");
            }
        }
        public void Dispose()
        {
            if (m_Scheduled)
            {
                if (m_AsyncUpdateAction != null)
                {
                    lock (m_AsyncUpdateAction)
                    {
                        m_Scheduler.Unschedule(m_AsyncUpdateAction);
                    }
                }

                if (m_UpdateAction != null)
                {
                    lock (m_UpdateAction)
                    {
                        m_Scheduler.Unschedule(m_UpdateAction);
                    }
                }

                m_Scheduled = false;
            }

            m_Scheduler = null;
        }

        public void SetAsyncUpdateAction(ScheduledAsyncAction asyncUpdateAction)
        {
            // Validate
            if (m_Scheduled) { throw new InvalidOperationException("An existing update action has already been scheduled and cannot be changed"); }

            // Store
            m_UpdateAction = null;
            m_AsyncUpdateAction = asyncUpdateAction ?? throw new ArgumentNullException("asyncUpdateAction");
        }

        public void SetUpdateAction(Action updtAction)
        {
            if (m_Scheduled) { throw new InvalidOperationException("An existing update action has already been scheduled and cannot be changed"); }

            // Store
            m_AsyncUpdateAction = null;
            m_UpdateAction = updtAction ?? throw new ArgumentNullException("updateAction");
        }

        public void Start()
        {
            // Validate
            ValidateUpdateAction();

            // Notify starting
            Starting?.Invoke(this);

            // Actually start
            if (m_AsyncUpdateAction != null)
            {
                lock (m_AsyncUpdateAction)
                {
                    if (m_Scheduled)
                    {
                        m_Scheduler.Resume(m_AsyncUpdateAction);
                    }
                    else
                    {
                        m_Scheduler.Schedule(m_AsyncUpdateAction, m_ScheduleOptions);
                        m_Scheduled = true;
                    }
                }
            }
            else
            {
                lock (m_UpdateAction)
                {
                    if (m_Scheduled)
                    {
                        m_Scheduler.Resume(m_UpdateAction);
                    }
                    else
                    {
                        m_Scheduler.Schedule(m_UpdateAction, m_ScheduleOptions);
                        m_Scheduled = true;
                    }
                }
            }

            // Notify started
            m_IsStarted = true;
            Started?.Invoke(this);
        }

        public void Stop()
        {
            // If not scheduled, ignore
            if (!m_Scheduled) { return; }

            // Notify stopping
            Stopping?.Invoke(this);
            // Actually stop
            if (m_AsyncUpdateAction != null)
            {
                lock (m_AsyncUpdateAction)
                {
                    // Suspend instead of unschedule to maintain registration sequence.
                    // This is important in case the synchronous update order matters.
                    m_Scheduler.Suspend(m_AsyncUpdateAction);
                }
            }
            else
            {
                lock (m_UpdateAction)
                {
                    // Suspend instead of unschedule to maintain registration sequence.
                    // This is important in case the synchronous update order matters.
                    m_Scheduler.Suspend(m_UpdateAction);
                }
            }

            // Notify stopped
            m_IsStarted = false;
            Stopped?.Invoke(this);
        }
        public bool IsStarted => m_IsStarted;

        public bool StartWithEvents { get; set; }

        public bool StopWithEvents { get; set; }

        public ScheduleOptions ScheduleOptions => m_ScheduleOptions;

        public uint UpdateInterval
        {
            get => m_ScheduleOptions.UpdateInterval;
            set
            {
                // Changing?
                if (value != m_ScheduleOptions.UpdateInterval)
                {
                    // New value or default?
                    SetUpdateInterval(value == 0 ? m_DefaultScheduleOptions.UpdateInterval : value);
                }
            }
        }
        public event ScheduledUpdaterAction Starting;

        public event ScheduledUpdaterAction Started;

        public event ScheduledUpdaterAction Stopping;

        public event ScheduledUpdaterAction Stopped;
        public void FirstAdded(object sender)
        {
            m_EventsSubscribed++;
            if ((m_EventsSubscribed == 1) && (StartWithEvents))
            {
                Start();
            }
        }

        public void Added(object sender)
        {

        }

        public void Removed(object sender)
        {

        }

        public void LastRemoved(object sender)
        {
            m_EventsSubscribed--;
            if ((m_EventsSubscribed == 0) && (StopWithEvents))
            {
                Stop();
            }
        }
    }
}
