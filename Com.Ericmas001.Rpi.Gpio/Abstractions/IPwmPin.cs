using System;
using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.Gpio.Abstractions
{
    public interface IPwmPin : IDisposable
    {
        void Start();
        void SetActiveDutyCyclePercentage(double value);
    }
}
