using Com.Ericmas001.Rpi.Gpio.Abstractions;
using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.Gpio
{
    public class Led : IOnOffListener
    {
        public IGpioPin Pin { get; }
        public Led(IGpioController controller, GpioEnum gpio)
        {
            Pin = controller.OpenPin(gpio);
            Pin.Write(GpioPinValueEnum.High);
            Pin.SetDriveMode(GpioPinDriveModeEnum.Output);
        }

        public void TurnOn(object activator = null)
        {
            Pin.Write(GpioPinValueEnum.Low);
        }

        public void TurnOff(object activator = null)
        {
            Pin.Write(GpioPinValueEnum.High);
        }
    }
}
