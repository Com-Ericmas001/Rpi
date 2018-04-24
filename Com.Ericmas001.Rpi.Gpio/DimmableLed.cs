using System;
using Com.Ericmas001.Rpi.Gpio.Abstractions;
using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.Gpio
{
    public class DimmableLed : IOnOffListener
    {
        public IPwmPin Pin { get; }
        public DimmableLed(IPwmController controller, GpioEnum gpio, int initialValue)
        {
            Pin = controller.OpenPin(gpio);
            Pin.Start();
            Dim(initialValue, this);
        }

        public void TurnOn(object activator = null)
        {
            Dim(100, activator);
        }

        public void TurnOff(object activator = null)
        {
            Dim(0, activator);
        }
        public void Dim(int value, object activator = null)
        {
            Pin.SetActiveDutyCyclePercentage(Math.Max(0, Math.Min(100, value)) / 100.0);
        }
    }
}
