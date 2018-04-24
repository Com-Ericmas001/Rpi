using System.ComponentModel;
using Windows.Devices.Gpio;
using Com.Ericmas001.Rpi.Gpio.Abstractions;
using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.DemoWindows.Implementations
{
    internal class MyGpioPin : IGpioPin
    {
        private readonly GpioPin m_Pin;

        public MyGpioPin(GpioPin pin)
        {
            m_Pin = pin;
        }
        public void Write(GpioPinValueEnum value)
        {
            switch (value)
            {
                case GpioPinValueEnum.High:
                    m_Pin.Write(GpioPinValue.High);
                    break;
                case GpioPinValueEnum.Low:
                    m_Pin.Write(GpioPinValue.Low);
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(GpioPinValueEnum));
            }
        }

        public GpioPinValueEnum Read()
        {
            var value = m_Pin.Read();
            switch (value)
            {
                case GpioPinValue.High:
                    return GpioPinValueEnum.High;
                case GpioPinValue.Low:
                    return GpioPinValueEnum.Low;
                default:
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(GpioPinValue));
            }
        }

        public void SetDriveMode(GpioPinDriveModeEnum value)
        {
            switch (value)
            {
                case GpioPinDriveModeEnum.Input:
                    m_Pin.SetDriveMode(GpioPinDriveMode.Input);
                    break;
                case GpioPinDriveModeEnum.InputPullDown:
                    m_Pin.SetDriveMode(GpioPinDriveMode.InputPullDown);
                    break;
                case GpioPinDriveModeEnum.InputPullUp:
                    m_Pin.SetDriveMode(GpioPinDriveMode.InputPullUp);
                    break;
                case GpioPinDriveModeEnum.Output:
                    m_Pin.SetDriveMode(GpioPinDriveMode.Output);
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(GpioPinDriveModeEnum));
            }
        }

        public void Dispose()
        {
            m_Pin.Dispose();
        }
    }
}
