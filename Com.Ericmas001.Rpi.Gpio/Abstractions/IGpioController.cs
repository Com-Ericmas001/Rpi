using System;
using System.Collections.Generic;
using System.Text;
using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.Gpio.Abstractions
{
    public interface IGpioController
    {
        IGpioPin OpenPin(GpioEnum gpio);
    }
}
