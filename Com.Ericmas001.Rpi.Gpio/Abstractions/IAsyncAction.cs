using System;

namespace Com.Ericmas001.Rpi.Gpio.Abstractions
{
    public interface IAsyncAction
    {
        void GetResults();
        MulticastDelegate Completed { get; set; }
    }
}
