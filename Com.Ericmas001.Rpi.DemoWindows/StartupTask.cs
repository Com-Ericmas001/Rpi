using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Com.Ericmas001.DependencyInjection.Unity;
using Com.Ericmas001.Logs;
using Com.Ericmas001.Logs.Enums;
using Com.Ericmas001.Logs.Services.Interfaces;
using Com.Ericmas001.Rpi.Gpio;
using Com.Ericmas001.Rpi.Gpio.Abstractions;
using Com.Ericmas001.Rpi.Gpio.Enums;
using Com.Ericmas001.Rpi.Gpio.Windows;
using Com.Ericmas001.Rpi.Gpio.Windows.Pwm;
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
            var pwmManager = new WindowsPwmProvider(controller);
            var dimPwmController = await pwmManager.GetDimmablePwmControllerProviderAsync();

            Task red = new Button(controller, GpioEnum.Gpio12, logger){Name = "RED"}.AttachListener((IOnPressListener)new ToggleLed(controller, GpioEnum.Gpio05, true)).RunAsync();
            Task blue = new ToggleButton(controller, GpioEnum.Gpio16, logger) { Name = "BLUE" }.AttachListener(new Led(controller, GpioEnum.Gpio06, true)).RunAsync();
            Task green = FadeInFadeOutAsync(dimPwmController, GpioEnum.Gpio21);

            Task.WaitAll(red, blue, green);
        }

        private async Task FadeInFadeOutAsync(IPwmController controller, GpioEnum gpio)
        {
            var led = new DimmableLed(controller, gpio, 10);
            while (true)
            {
                for (int i = 10; i <= 90; i += 4)
                {
                    led.Dim(i, this);
                    await Task.Delay(10);
                }
                for (int i = 90; i >= 10; i -= 4)
                {
                    led.Dim(i, this);
                    await Task.Delay(50);
                }
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
            return new WindowsGpioController(gpio);
        }
    }
}