using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Com.Ericmas001.DependencyInjection.Unity;
using Com.Ericmas001.Logs;
using Com.Ericmas001.Logs.Enums;
using Com.Ericmas001.Logs.Services.Interfaces;
using Com.Ericmas001.Rpi.DemoWindows.Implementations;
using Com.Ericmas001.Rpi.Gpio;
using Com.Ericmas001.Rpi.Gpio.Abstractions;
using Com.Ericmas001.Rpi.Gpio.Enums;
using Unity;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Com.Ericmas001.Rpi.DemoWindows
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            IUnityContainer container = new UnityContainer();
            new TraceLogsRegistrant().RegisterTypes(container);

            var logger = container.Resolve<ILoggerService>();
            var controller = InitGPIO(logger);

            Task red = new Button(controller, GpioEnum.Gpio12, "RED", logger).AttachListener((IOnPressListener)new ToggleLed(controller, GpioEnum.Gpio05)).RunAsync();
            Task blue = new ToggleButton(controller, GpioEnum.Gpio16, "BLUE", logger).AttachListener(new Led(controller, GpioEnum.Gpio06)).RunAsync();

            Task.WaitAll(red, blue);
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