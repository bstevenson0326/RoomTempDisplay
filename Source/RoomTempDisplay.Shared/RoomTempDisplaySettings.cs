using Verse;

namespace RoomTempDisplay
{
    /// <summary>
    /// Represents the display settings for room temperature visualization, including options for temperature range
    /// colors, overlay text, and comfort thresholds.
    /// </summary>
    /// <remarks>
    /// This class allows customization of how room temperature information is displayed, such as 
    /// enabling or disabling visual indicators for temperature ranges and overlay text. It also defines thresholds for
    /// comfortable, cold, and hot temperature ranges in Fahrenheit.
    /// </remarks>
    public class RoomTempDisplaySettings : ModSettings
    {
        public bool showTemperatureRangeColors = false;
        public bool showOverlayText = true;

        public float minComfortableFahrenheit = 45f;
        public float maxComfortableFahrenheit = 84f;

        public float coldMinFahrenheit = 15f;
        public float hotMaxFahrenheit = 110f;

        /// <summary>
        /// Saves and loads the state of the object's fields for serialization purposes.
        /// </summary>
        /// <remarks>
        /// This method is typically used during the save/load process to persist or restore the
        /// values of the object's fields. It ensures that the state of the object is correctly serialized and
        /// deserialized using the Scribe system.
        /// </remarks>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref showTemperatureRangeColors, "showTemperatureRangeColors", false);
            Scribe_Values.Look(ref showOverlayText, "showOverlayText", true);
            Scribe_Values.Look(ref coldMinFahrenheit, "coldMinFahrenheit", 15f);
            Scribe_Values.Look(ref hotMaxFahrenheit, "hotMaxFahrenheit", 110f);
            Scribe_Values.Look(ref minComfortableFahrenheit, "minComfortableFahrenheit", 45f);
            Scribe_Values.Look(ref maxComfortableFahrenheit, "maxComfortableFahrenheit", 84f);
        }
    }
}
