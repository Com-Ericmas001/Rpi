using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Windows.Devices.Pwm;
using Com.Ericmas001.DependencyInjection.Unity;
using Com.Ericmas001.Logs;
using Com.Ericmas001.Logs.Enums;
using Com.Ericmas001.Logs.Services.Interfaces;
using Com.Ericmas001.Rpi.DemoWindows.Implementations;
using Com.Ericmas001.Rpi.Gpio;
using Com.Ericmas001.Rpi.Gpio.Abstractions;
using Com.Ericmas001.Rpi.Gpio.Enums;
using Microsoft.IoT.DeviceCore.Pwm;
using Microsoft.IoT.Devices.Pwm;
using Unity;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Com.Ericmas001.Rpi.DemoWindows
{
    public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            IUnityContainer container = new UnityContainer();
            new TraceLogsRegistrant().RegisterTypes(container);

            var logger = container.Resolve<ILoggerService>();
            var controller = InitGPIO(logger);

            Task red = new Button(controller, GpioEnum.Gpio12, "RED", logger).AttachListener((IOnPressListener)new ToggleLed(controller, GpioEnum.Gpio05)).RunAsync();
            Task blue = new ToggleButton(controller, GpioEnum.Gpio16, "BLUE", logger).AttachListener(new Led(controller, GpioEnum.Gpio06)).RunAsync();

            var pwmManager = new PwmProviderManager();
            pwmManager.Providers.Add(new SoftPwm());
            var pwmControllers = await pwmManager.GetControllersAsync();
            //use the first available PWM controller an set refresh rate (Hz)
            var _pwmController = pwmControllers[0];
            _pwmController.SetDesiredFrequency(240);

            var _redLed = _pwmController.OpenPin(21);
            _redLed.Start();

            Task green = HaveFunWithLedAsync(_redLed);

            Task.WaitAll(red, blue, green);
        }

        private async Task HaveFunWithLedAsync(PwmPin pin)
        {
            while (true)
            {
                await Task.Delay(2000);
                pin.SetActiveDutyCyclePercentage(0.20);
                await Task.Delay(2000);
                pin.SetActiveDutyCyclePercentage(0.80);
                await Task.Delay(2000);
                pin.SetActiveDutyCyclePercentage(0.50);
                await Task.Delay(2000);
                pin.SetActiveDutyCyclePercentage(0);
                await Task.Delay(2000);
                pin.SetActiveDutyCyclePercentage(1);
            }
        }

        private IGpioController InitGPIO(ILoggerService logger)
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                logger.Log(LogLevelEnum.Error, "There is no GPIO controller on this device.");
                throw new Exception("There is no GPIO controller on this device.");
            }

            logger.Log(LogLevelEnum.Information, "GPIO initialized correctly.");
            return new MyGpioController(gpio);
        }
    }
}