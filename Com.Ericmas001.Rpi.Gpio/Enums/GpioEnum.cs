namespace Com.Ericmas001.Rpi.Gpio.Enums
{
    public enum GpioEnum
    {
        Gpio02 = 3,
        I2CSca = 3,
        Gpio03 = 5,
        I2CScl = 5,
        Gpio04 = 7,
        Gpio05 = 29,
        Gpio06 = 31,
        Gpio07 = 26,
        SpiCe1 = 26,
        Gpio08 = 24,
        SpiCe0 = 24,
        Gpio09 = 21,
        SpiMiso = 21,
        Gpio10 = 19,
        SpiMosi = 19,
        Gpio11 = 23,
        SpiSclk = 23,
        Gpio12 = 32,
        Gpio13 = 33,
        Gpio14 = 8,
        UartTxd = 8,
        Gpio15 = 10,
        UartRxd = 10,
        Gpio16 = 36,
        Gpio17 = 11,
        Gpio18 = 12,
        PcmClk = 12,
        Gpio19 = 35,
        PcmFs = 35,
        Gpio20 = 38,
        PcmDin = 38,
        Gpio21 = 40,
        PcmDout = 40,
        Gpio22 = 15,
        Gpio23 = 16,
        Gpio24 = 18,
        Gpio25 = 22,
        Gpio26 = 37,
    }

    public static class GpioEnumExtensions
    {
        public static int ToPinNumber(this GpioEnum e)
        {
            return (int)e;
        }
        public static int ToGpioNumber(this GpioEnum e)
        {
            switch (e)
            {
                case GpioEnum.Gpio02:
                    return 2;
                case GpioEnum.Gpio03:
                    return 3;
                case GpioEnum.Gpio04:
                    return 4;
                case GpioEnum.Gpio05:
                    return 5;
                case GpioEnum.Gpio06:
                    return 6;
                case GpioEnum.Gpio07:
                    return 7;
                case GpioEnum.Gpio08:
                    return 8;
                case GpioEnum.Gpio09:
                    return 9;
                case GpioEnum.Gpio10:
                    return 10;
                case GpioEnum.Gpio11:
                    return 11;
                case GpioEnum.Gpio12:
                    return 12;
                case GpioEnum.Gpio13:
                    return 13;
                case GpioEnum.Gpio14:
                    return 14;
                case GpioEnum.Gpio15:
                    return 15;
                case GpioEnum.Gpio16:
                    return 16;
                case GpioEnum.Gpio17:
                    return 17;
                case GpioEnum.Gpio18:
                    return 18;
                case GpioEnum.Gpio19:
                    return 19;
                case GpioEnum.Gpio20:
                    return 20;
                case GpioEnum.Gpio21:
                    return 21;
                case GpioEnum.Gpio22:
                    return 22;
                case GpioEnum.Gpio23:
                    return 23;
                case GpioEnum.Gpio24:
                    return 24;
                case GpioEnum.Gpio25:
                    return 25;
                case GpioEnum.Gpio26:
                    return 26;
            }

            return -1;
        }
    }
}
