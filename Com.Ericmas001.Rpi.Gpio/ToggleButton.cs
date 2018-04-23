using System.Threading.Tasks;
using Com.Ericmas001.Logs.Services.Interfaces;
using Com.Ericmas001.Rpi.Gpio.Abstractions;
using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.Gpio
{
    public class ToggleButton : Button
    {
        public ToggleButton(IGpioController controller, GpioEnum gpio, string name, ILoggerService loggerService = null) : base(controller, gpio, name, loggerService)
        {
        }

        public override async Task RunAsync()
        {
            while (true)
            {
                if (ButtonPin.Read() == GpioPinValueEnum.Low)
                {
                    // ReSharper disable once AssignmentInConditionalExpression
                    if (IsOn = !IsOn)
                        OnButtonOn();
                    else
                        OnButtonOff();
                    await Task.Delay(200);
                }
                await Task.Delay(10);
            }
        }
    }
}
