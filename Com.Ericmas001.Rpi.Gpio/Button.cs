using System;
using System.Threading.Tasks;
using Com.Ericmas001.Logs.Enums;
using Com.Ericmas001.Logs.Services.Interfaces;
using Com.Ericmas001.Rpi.Gpio.Abstractions;
using Com.Ericmas001.Rpi.Gpio.Enums;

namespace Com.Ericmas001.Rpi.Gpio
{
    internal class Button
    {
        public string Name { get; }
        public IGpioPin ButtonPin { get; }

        public bool IsOn { get; protected set; }

        protected event EventHandler TurnedOn;
        protected event EventHandler TurnedOff;

        public Button(IGpioController controller, GpioEnum gpio, string name, ILoggerService loggerService = null)
        {
            Name = name;
            ButtonPin = controller.OpenPin(gpio);
            ButtonPin.SetDriveMode(GpioPinDriveModeEnum.InputPullUp);
            if (loggerService != null)
            {
                TurnedOn += (s, a) => loggerService.Log(LogLevelEnum.Information, $"{Name} is turned on");
                TurnedOff += (s, a) => loggerService.Log(LogLevelEnum.Information, $"{Name} is turned off");
            }
        }

        protected virtual void OnButtonOn() => TurnedOn?.Invoke(this, EventArgs.Empty);
        protected virtual void OnButtonOff() => TurnedOff?.Invoke(this, EventArgs.Empty);

        public Button AttachListener(IOnOffListener listener)
        {
            TurnedOn += (s, a) => listener.TurnOn(s);
            TurnedOff += (s, a) => listener.TurnOff(s);
            return this;
        }
        public Button AttachListener(IOnPressListener listener)
        {
            TurnedOn += (s, a) => listener.Do(s);
            return this;
        }

        public virtual async Task RunAsync()
        {
            while (true)
            {
                if (ButtonPin.Read() == GpioPinValueEnum.Low && !IsOn)
                {
                    IsOn = true;
                    OnButtonOn();
                }
                if (ButtonPin.Read() == GpioPinValueEnum.High && IsOn)
                {
                    IsOn = false;
                    OnButtonOff();
                }
                await Task.Delay(10);
            }
        }
    }
}