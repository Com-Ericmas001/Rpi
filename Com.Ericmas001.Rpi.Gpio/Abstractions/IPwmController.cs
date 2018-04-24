using System;
using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.Gpio.Abstractions
{
    public interface IPwmController
    {
        IPwmPin OpenPin(GpioEnum gpio);
    }
}
