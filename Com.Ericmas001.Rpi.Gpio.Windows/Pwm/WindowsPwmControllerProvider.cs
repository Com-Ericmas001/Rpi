using System.Collections.Generic;
using Windows.Devices.Pwm.Provider;
using Com.Ericmas001.Rpi.Gpio.Abstractions;
using Com.Ericmas001.Rpi.Gpio.Enums;
using Com.Ericmas001.Rpi.Gpio.Pwm;

namespace Com.Ericmas001.Rpi.Gpio.Windows.Pwm
{
    public class WindowsPwmControllerProvider : IPwmControllerProvider
    {
        private readonly SoftwarePwmController m_SoftwarePwmController;

        public WindowsPwmControllerProvider(IGpioController controller)
        {
            m_SoftwarePwmController = new SoftwarePwmController(controller);
        }

        public double SetDesiredFrequency(double frequency)
        {
            return m_SoftwarePwmController.SetDesiredFrequency(frequency);
        }

        public void AcquirePin(int pin)
        {
            m_SoftwarePwmController.AcquirePin(GpioEnumUtil.FromGpioNumber(pin));
        }

        public void ReleasePin(int pin)
        {
            m_SoftwarePwmController.ReleasePin(GpioEnumUtil.FromGpioNumber(pin));
        }

        public void EnablePin(int pin)
        {
            m_SoftwarePwmController.EnablePin(GpioEnumUtil.FromGpioNumber(pin));
        }

        public void DisablePin(int pin)
        {
            m_SoftwarePwmController.DisablePin(GpioEnumUtil.FromGpioNumber(pin));
        }

        public void SetPulseParameters(int pin, double dutyCycle, bool invertPolarity)
        {
            m_SoftwarePwmController.SetPulseParameters(GpioEnumUtil.FromGpioNumber(pin), dutyCycle, invertPolarity);
        }

        public double ActualFrequency => m_SoftwarePwmController.ActualFrequency;
        public double MaxFrequency => m_SoftwarePwmController.MaxFrequency;
        public double MinFrequency => m_SoftwarePwmController.MinFrequency;
        public int PinCount => m_SoftwarePwmController.PinCount;
    }
}
