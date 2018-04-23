using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.Gpio.Abstractions
{
    public interface IGpioController
    {
        IGpioPin OpenPin(GpioEnum gpio);
    }
}
