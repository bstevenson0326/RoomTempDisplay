using System;
using Verse;

namespace RoomTempDisplay
{
    internal static class TemperatureUtils
    {
        internal static float ConvertToFahrenheit(float value, TemperatureDisplayMode from)
        {
            switch (from)
            {
                case TemperatureDisplayMode.Celsius:
                    return value * 1.8f + 32f;
                case TemperatureDisplayMode.Kelvin:
                    return (value - 273.15f) * 1.8f + 32f;
                case TemperatureDisplayMode.Fahrenheit:
                    return value;
                default:
                    throw new InvalidOperationException();
            }
        }

        internal static float ConvertFromFahrenheit(float fahrenheit, TemperatureDisplayMode to)
        {
            switch (to)
            {
                case TemperatureDisplayMode.Celsius:
                    return (fahrenheit - 32f) / 1.8f;
                case TemperatureDisplayMode.Kelvin:
                    return (fahrenheit + 459.67f) * 5f / 9f;
                case TemperatureDisplayMode.Fahrenheit:
                    return fahrenheit;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
