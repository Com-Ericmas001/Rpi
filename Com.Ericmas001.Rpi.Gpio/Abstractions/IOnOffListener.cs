namespace Com.Ericmas001.Rpi.Gpio.Abstractions
{
    public interface IOnOffListener
    {
        void TurnOn(object activator);
        void TurnOff(object activator);
    }
}
