using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Pwm;
using Com.Ericmas001.Rpi.Gpio.Abstractions;

namespace Com.Ericmas001.Rpi.DemoWindows.Implementations
{
    internal class MyPwmPin : IPwmPin
    {
        private readonly PwmPin m_PwmPin;

        public MyPwmPin(PwmPin pwmPin)
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
