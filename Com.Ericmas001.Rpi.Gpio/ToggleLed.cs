using Com.Ericmas001.Rpi.Gpio.Abstractions;
using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.Gpio
{
    internal class ToggleLed : Led, IOnPressListener
    {
        public bool IsOn { get; protected set; }
        public ToggleLed(IGpioController controller, GpioEnum gpio) : base(controller, gpio)
        {
        }

        public void Do(object activator = null)
        {
            // ReSharper disable once AssignmentInConditionalExpression
            if (IsOn = !IsOn)
                TurnOn(activator);
            else
                TurnOff(activator);
        }
    }
}
