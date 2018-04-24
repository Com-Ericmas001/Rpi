using Windows.Devices.Pwm;
using Com.Ericmas001.Rpi.Gpio.Abstractions;

namespace Com.Ericmas001.Rpi.Gpio.Windows
{
    public class WindowsPwmPin : IPwmPin
    {
        private readonly PwmPin m_PwmPin;

        public WindowsPwmPin(PwmPin pwmPin)
        {
            m_PwmPin = pwmPin;
        }

        public void Dispose()
        {
            m_PwmPin.Dispose();
        }

        public void Start()
        {
            m_PwmPin.Start();
        }

        public void SetActiveDutyCyclePercentage(double value)
        {
            m_PwmPin.SetActiveDutyCyclePercentage(value);
        }
    }
}
