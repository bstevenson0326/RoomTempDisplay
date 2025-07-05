using System;
using System.Linq;
using Verse;

namespace RoomTempDisplay
{
    /// <summary>
    /// Provides utility methods for converting temperature values between different temperature scales.
    /// </summary>
    /// <remarks>
    /// This class includes methods for converting temperatures to and from Fahrenheit, supporting
    /// Celsius, Kelvin, and Fahrenheit as input and output scales. It is intended for internal use and assumes valid
    /// input values for the specified temperature scales.
    /// </remarks>
    internal static class TemperatureUtils
    {
        /// <summary>
        /// Converts a temperature value to Fahrenheit from the specified temperature display mode.
        /// </summary>
        /// <param name="value">The temperature value to convert.</param>
        /// <param name="from">The <see cref="TemperatureDisplayMode"/> representing the unit of the input temperature value.</param>
        /// <returns>The equivalent temperature in Fahrenheit.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the <paramref name="from"/> parameter is not a valid <see cref="TemperatureDisplayMode"/>.</exception>
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

        /// <summary>
        /// Converts a temperature value from Fahrenheit to the specified temperature scale.
        /// </summary>
        /// <param name="fahrenheit">The temperature value in Fahrenheit to convert.</param>
        /// <param name="to">The target temperature scale to convert to. Must be one of the <see cref="TemperatureDisplayMode"/> values.</param>
        /// <returns>The converted temperature value in the specified scale. If <paramref name="to"/> is <see
        /// cref="TemperatureDisplayMode.Fahrenheit"/>,  the original value is returned unchanged.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="to"/> is not a valid <see cref="TemperatureDisplayMode"/> value.</exception>
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

        /// <summary>
        /// Determines whether a room is a valid candidate for temperature display based on ID, cell count, 
        /// proper room designation, and fog status.
        /// </summary>
        /// <param name="room">The room to interrogate</param>
        /// <param name="map">The map in which the room resides</param>
        /// <returns>Returns true if it is a room; otherwise, false.</returns>
        internal static bool IsRoomCandidate(this Room room, Map map)
        {
            return room != null && room.ID != 0 && room.ProperRoom && room.CellCount > 1 && room.Cells.All(x => !x.Fogged(map));
        }
    }
}
