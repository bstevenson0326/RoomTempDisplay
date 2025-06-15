using Verse;

namespace RoomTempDisplay
{
    public class RoomTempDisplaySettings : ModSettings
    {
        public bool showTemperatureRangeColors = false;

        public float minComfortableFahrenheit = 45f;
        public float maxComfortableFahrenheit = 84f;

        public float coldMinFahrenheit = 15f;
        public float hotMaxFahrenheit = 110f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref showTemperatureRangeColors, "showTemperatureRangeColors", false);
            Scribe_Values.Look(ref coldMinFahrenheit, "coldMinFahrenheit", 15f);
            Scribe_Values.Look(ref hotMaxFahrenheit, "hotMaxFahrenheit", 110f);
            Scribe_Values.Look(ref minComfortableFahrenheit, "minComfortableFahrenheit", 45f);
            Scribe_Values.Look(ref maxComfortableFahrenheit, "maxComfortableFahrenheit", 84f);
        }
    }
}
