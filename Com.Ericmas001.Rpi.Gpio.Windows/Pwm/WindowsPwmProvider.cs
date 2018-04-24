using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Pwm;
using Windows.Devices.Pwm.Provider;
using Com.Ericmas001.Rpi.Gpio.Abstractions;

namespace Com.Ericmas001.Rpi.Gpio.Windows.Pwm
{
    public class WindowsPwmProvider : IPwmProvider
    {
        private readonly WindowsPwmControllerProvider m_DimmablePwmControllerProvider;
        private readonly WindowsPwmControllerProvider m_InfraredPwmControllerProvider;

        public WindowsPwmProvider(IGpioController controller)
        {
            m_DimmablePwmControllerProvider = new WindowsPwmControllerProvider(controller);
            m_DimmablePwmControllerProvider.SetDesiredFrequency(240);
            m_InfraredPwmControllerProvider = new WindowsPwmControllerProvider(controller);
            m_InfraredPwmControllerProvider.SetDesiredFrequency(3800);
        }

        public IReadOnlyList<IPwmControllerProvider> GetControllers()
        {
            return new[] { m_DimmablePwmControllerProvider, m_InfraredPwmControllerProvider };
        }

        public async Task<IPwmController> GetDimmablePwmControllerProviderAsync()
        {
            return new WindowsPwmController((await PwmController.GetControllersAsync(this))[0]);
        }

        public async Task<IPwmController> GetInfraredPwmControllerProviderAsync()
        {
            return new WindowsPwmController((await PwmController.GetControllersAsync(this))[1]);
        }
    }
}
