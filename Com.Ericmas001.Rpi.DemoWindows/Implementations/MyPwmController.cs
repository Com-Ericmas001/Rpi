using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Pwm;
using Com.Ericmas001.Rpi.Gpio.Abstractions;
using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.DemoWindows.Implementations
{
    internal class MyPwmController : IPwmController
    {
        private readonly PwmController m_PwmController;

        public MyPwmController(PwmController pwmController)
        {
            m_PwmController = pwmController;
        }

        public IPwmPin OpenPin(GpioEnum gpio)
        {
            return new MyPwmPin(m_PwmController.OpenPin(gpio.ToGpioNumber()));
        }
    }
}
