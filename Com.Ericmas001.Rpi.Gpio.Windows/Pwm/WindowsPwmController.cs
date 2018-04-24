using Windows.Devices.Pwm;
using Com.Ericmas001.Rpi.Gpio.Abstractions;
using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.Gpio.Windows
{
    public class WindowsPwmController : IPwmController
    {
        private readonly PwmController m_PwmController;

        public WindowsPwmController(PwmController pwmController)
        {
            m_PwmController = pwmController;
        }

        public IPwmPin OpenPin(GpioEnum gpio)
        {
            return new WindowsPwmPin(m_PwmController.OpenPin(gpio.ToGpioNumber()));
        }
    }
}
