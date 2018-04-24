//From original: https://github.com/jbienzms/iot-devices/blob/master/Lib/Microsoft.IoT.Devices/Pwm/SoftPwm.cs

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Com.Ericmas001.Rpi.Gpio.Abstractions;
using Com.Ericmas001.Rpi.Gpio.Enums;
using Com.Ericmas001.Rpi.Gpio.Scheduling;

namespace Com.Ericmas001.Rpi.Gpio.Pwm
{
    public class SoftwarePwmController : IDisposable
    {
        private class SoftPwmPin
        {
            internal double TargetTicks;
            public SoftPwmPin(IGpioPin pin)
            {
                Pin = pin;
            }
            public double DutyCycle { get; set; }
            public bool Enabled { get; set; }
            public bool InvertPolarity { get; set; }

            public IGpioPin Pin { get; }
        }
        
        private const int MAX_FREQUENCY = 38000;
        private const int MIN_FREQUENCY = 40;

        public int PinCount => m_PinCount;
        private double m_ActualFrequency;
        private readonly IGpioController m_GpioController;
        private readonly int m_PinCount;
        private readonly Stopwatch m_Stopwatch;
        private readonly long m_TicksPerSecond;
        private Dictionary<GpioEnum, SoftPwmPin> m_Pins;
        private ScheduledUpdater m_Updater;
        public double ActualFrequency => m_ActualFrequency;
        public double MaxFrequency => MAX_FREQUENCY;
        public double MinFrequency => MIN_FREQUENCY;

        public SoftwarePwmController(IGpioController controller)
        {
            // Get GPIO
            m_GpioController = controller;

            // How many pins
            m_PinCount = m_GpioController.PinCount;

            // Create pin lookup
            m_Pins = new Dictionary<GpioEnum, SoftPwmPin>(m_PinCount);

            // Create
            m_Stopwatch = new Stopwatch();

            // Defaults
            m_ActualFrequency = MIN_FREQUENCY;
            m_TicksPerSecond = Stopwatch.Frequency;

            // Create the updater. Default to 0 seconds between updates, meaning run as fast as possible.
            // IMPORTANT: Do not use Scheduler.Default, create a new Scheduler.
            // This puts us in parallel priority with other sensors and allows 
            // us to run on a separate core if available.
            m_Updater = new ScheduledUpdater(new ScheduleOptions(0), new Scheduler());
            m_Updater.SetUpdateAction(Update);
        }
        
        private void Update()
        {
            var enabledPins = m_Pins.Values.Where(p => p.Enabled && Math.Abs(p.DutyCycle) > double.Epsilon).ToList();

            // If there are no enabled pins, stop updates
            if (enabledPins.Count == 0)
            {
                m_Updater.Stop();
                return;
            }

            foreach (var softPin in enabledPins)
            {
                var value = (softPin.InvertPolarity) ? GpioPinValueEnum.Low : GpioPinValueEnum.High;
                softPin.Pin.Write(value);
            }

            if (!m_Stopwatch.IsRunning)
            {
                m_Stopwatch.Start();
            }
            else
            {
                m_Stopwatch.Restart();
            }

            long startTicks = m_Stopwatch.ElapsedTicks;
            long currentTicks;
            double period = 1000.0 / m_ActualFrequency;

            // Calculate target ticks
            foreach (var softPin in enabledPins)
            {
                softPin.TargetTicks = startTicks + softPin.DutyCycle * period * m_TicksPerSecond / 1000.0;
            }

            int processedPins = 0;
            while (processedPins < enabledPins.Count)
            {
                currentTicks = m_Stopwatch.ElapsedTicks;

                foreach (var softPin in enabledPins)
                {
                    if ((softPin.TargetTicks > 0) && (currentTicks > softPin.TargetTicks))
                    {
                        softPin.TargetTicks = 0;
                        processedPins++;

                        var pinValue = (softPin.InvertPolarity) ? GpioPinValueEnum.High : GpioPinValueEnum.Low;
                        softPin.Pin.Write(pinValue);
                    }
                }
            }

            double endCycleTicks = startTicks + period * m_TicksPerSecond / 1000.0;
            currentTicks = m_Stopwatch.ElapsedTicks;

            while (currentTicks < endCycleTicks)
            {
                currentTicks = m_Stopwatch.ElapsedTicks;
            }
        }

        public void AcquirePin(GpioEnum pin)
        {
            lock (m_Pins)
            {
                if (m_Pins.ContainsKey(pin))
                {
                    throw new UnauthorizedAccessException();
                }

                var gpioPin = m_GpioController.OpenPin(pin);
                gpioPin.SetDriveMode(GpioPinDriveModeEnum.Output);
                m_Pins[pin] = new SoftPwmPin(gpioPin);
            }
        }

        public void DisablePin(GpioEnum pin)
        {
            lock (m_Pins)
            {
                if (!m_Pins.ContainsKey(pin))
                {
                    throw new UnauthorizedAccessException();
                }

                m_Pins[pin].Enabled = false;
            }
        }

        public IGpioPin OpenPin(GpioEnum pin)
        {
            AcquirePin(pin);
            EnablePin(pin);
            lock (m_Pins)
            {
                return m_Pins[pin].Pin;
            }
        }

        public void EnablePin(GpioEnum pin)
        {
            lock (m_Pins)
            {
                if (!m_Pins.ContainsKey(pin))
                {
                    throw new UnauthorizedAccessException();
                }

                m_Pins[pin].Enabled = true;
            }

            // Make sure updates are running
            if (!m_Updater.IsStarted)
            {
                m_Updater.Start();
            }
        }

        public void ReleasePin(GpioEnum pin)
        {
            lock (m_Pins)
            {
                if (!m_Pins.ContainsKey(pin))
                {
                    throw new UnauthorizedAccessException();
                }

                m_Pins[pin].Pin.Dispose();
                m_Pins.Remove(pin);
            }
        }

        public double SetDesiredFrequency(double frequency)
        {
            if (frequency < MIN_FREQUENCY || frequency > MAX_FREQUENCY)
            {
                throw new ArgumentOutOfRangeException(nameof(frequency));
            }

            m_ActualFrequency = frequency;

            return m_ActualFrequency;
        }

        public void SetPulseParameters(GpioEnum pin, double dutyCycle, bool invertPolarity)
        {
            if ((dutyCycle < 0) || (dutyCycle > 1)) throw new ArgumentOutOfRangeException("dutyCycle");

            lock (m_Pins)
            {
                if (!m_Pins.ContainsKey(pin))
                {
                    throw new UnauthorizedAccessException();
                }

                var softPin = m_Pins[pin];
                softPin.DutyCycle = dutyCycle;
                softPin.InvertPolarity = invertPolarity;
            }

            // If duty cycle isn't zero we need to make sure updates are running
            if ((Math.Abs(dutyCycle) > double.Epsilon) && (!m_Updater.IsStarted))
            {
                m_Updater.Start();
            }
        }

        public void Dispose()
        {
            if (m_Updater != null)
            {
                m_Updater.Dispose();
                m_Updater = null;
            }

            // Dispose each pin
            lock (m_Pins)
            {
                foreach (var pinKey in m_Pins.Keys.ToArray())
                {
                    m_Pins[pinKey].Pin.Dispose();
                    m_Pins.Remove(pinKey);
                }
            }

            m_Pins = null;
        }
    }
}
