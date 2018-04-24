// From original: https://github.com/jbienzms/iot-devices/blob/master/Lib/Microsoft.IoT.DeviceCore/Scheduling/ScheduleOptions.cs

using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.Gpio.Scheduling
{
    public class ScheduleOptions
    {
        public ScheduleOptions(uint updateInterval, SchedulerPriorityEnum priority)
        {
            UpdateInterval = updateInterval;
            Priority = priority;
        }

        public ScheduleOptions(uint reportInterval) : this(reportInterval, SchedulerPriorityEnum.Default) { }
        public ScheduleOptions WithNewUpdateInterval(uint updateInterval)
        {
            return new ScheduleOptions(updateInterval, Priority);
        }

        public ScheduleOptions WithNewPriority(SchedulerPriorityEnum priority)
        {
            return new ScheduleOptions(UpdateInterval, priority);
        }
        public SchedulerPriorityEnum Priority { get; }
        public uint UpdateInterval { get; }
    }
}
