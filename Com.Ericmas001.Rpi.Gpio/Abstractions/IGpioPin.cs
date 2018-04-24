using System;
using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.Gpio.Abstractions
{
    public interface IGpioPin : IDisposable
    {
        void Write(GpioPinValueEnum value);
        GpioPinValueEnum Read();
        void SetDriveMode(GpioPinDriveModeEnum value);
    }
}
