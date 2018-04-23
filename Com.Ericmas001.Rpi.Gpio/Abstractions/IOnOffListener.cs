namespace Com.Ericmas001.Rpi.Gpio.Abstractions
{
    internal interface IOnOffListener
    {
        void TurnOn(object activator);
        void TurnOff(object activator);
    }
}
