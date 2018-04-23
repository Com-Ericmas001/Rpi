using Windows.Devices.Gpio;
using Com.Ericmas001.Rpi.Gpio.Abstractions;
using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.DemoWindows.Implementations
{
    class MyGpioController : IGpioController
    {
        private readonly GpioController m_Controller;

        public MyGpioController(GpioController controller)
        {
            m_Controller = controller;
        }
        public IGpioPin OpenPin(GpioEnum gpio)
        {
            return new MyGpioPin(m_Controller.OpenPin(gpio.ToGpioNumber()));
        }
    }
}
