using Windows.Devices.Gpio;
using Com.Ericmas001.Rpi.Gpio.Abstractions;
using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.Gpio.Windows
{
    public class WindowsGpioController : IGpioController
    {
        private readonly GpioController m_Controller;

        public WindowsGpioController(GpioController controller)
        {
            m_Controller = controller;
        }
        public IGpioPin OpenPin(GpioEnum gpio)
        {
            return new WindowsGpioPin(m_Controller.OpenPin(gpio.ToGpioNumber()));
        }

        public int PinCount => m_Controller.PinCount;
    }
}
