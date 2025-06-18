using UnityEngine;
using Verse;

namespace RoomTempDisplay
{
    public class RoomTempDisplayMod : Mod
    {
        public static RoomTempDisplaySettings Settings;

        public RoomTempDisplayMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<RoomTempDisplaySettings>();
        }

        public override string SettingsCategory() => "Room Temperature Display";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            // ───────── Section: General Options ─────────
            Text.Font = GameFont.Medium;
            listing.Label("RoomTempDisplay_Settings_Options".Translate());
            Text.Font = GameFont.Small;
            listing.CheckboxLabeled("RoomTempDisplay_Settings_ShowColors".Translate(), ref Settings.showTemperatureRangeColors);
            listing.GapLine();
            listing.Gap();

            // ───────── Section: Gradient Extremes ─────────
            Text.Font = GameFont.Medium;
            listing.Label("RoomTempDisplay_Settings_GradientLabel".Translate());
            Text.Font = GameFont.Small;

            string unitIcon = Prefs.TemperatureMode == TemperatureDisplayMode.Fahrenheit ? "\u00B0F" :
                              Prefs.TemperatureMode == TemperatureDisplayMode.Celsius ? "\u00B0C" : "K";

            TemperatureDisplayMode mode = Prefs.TemperatureMode;

            float coldMinDisplay = TemperatureUtils.ConvertFromFahrenheit(Settings.coldMinFahrenheit, mode);
            float hotMaxDisplay = TemperatureUtils.ConvertFromFahrenheit(Settings.hotMaxFahrenheit, mode);

            listing.Label("RoomTempDisplay_Settings_ColdMin".Translate(unitIcon) + $" {coldMinDisplay:F0}");
            coldMinDisplay = listing.Slider(coldMinDisplay,
                TemperatureUtils.ConvertFromFahrenheit(-100, mode),
                TemperatureUtils.ConvertFromFahrenheit(Settings.minComfortableFahrenheit, mode));

            listing.Label("RoomTempDisplay_Settings_HotMax".Translate(unitIcon) + $" {hotMaxDisplay:F0}");
            hotMaxDisplay = listing.Slider(hotMaxDisplay,
                TemperatureUtils.ConvertFromFahrenheit(Settings.maxComfortableFahrenheit, mode),
                TemperatureUtils.ConvertFromFahrenheit(160, mode));

            listing.GapLine();
            listing.Gap();

            // ───────── Section: Comfortable Temperature Range ─────────
            Text.Font = GameFont.Medium;
            listing.Label("RoomTempDisplay_Settings_ComfortLabel".Translate());
            Text.Font = GameFont.Small;

            float minComfortDisplay = TemperatureUtils.ConvertFromFahrenheit(Settings.minComfortableFahrenheit, mode);
            float maxComfortDisplay = TemperatureUtils.ConvertFromFahrenheit(Settings.maxComfortableFahrenheit, mode);

            listing.Label("RoomTempDisplay_Settings_MinComfort".Translate(unitIcon) + $" {minComfortDisplay:F0}");
            minComfortDisplay = listing.Slider(minComfortDisplay,
                TemperatureUtils.ConvertFromFahrenheit(-40, mode),
                TemperatureUtils.ConvertFromFahrenheit(120, mode));

            listing.Label("RoomTempDisplay_Settings_MaxComfort".Translate(unitIcon) + $" {maxComfortDisplay:F0}");
            maxComfortDisplay = listing.Slider(maxComfortDisplay,
                TemperatureUtils.ConvertFromFahrenheit(-40, mode),
                TemperatureUtils.ConvertFromFahrenheit(140, mode));

            // Store updated values back in °F
            Settings.minComfortableFahrenheit = TemperatureUtils.ConvertToFahrenheit(minComfortDisplay, mode);
            Settings.maxComfortableFahrenheit = TemperatureUtils.ConvertToFahrenheit(maxComfortDisplay, mode);
            Settings.coldMinFahrenheit = TemperatureUtils.ConvertToFahrenheit(coldMinDisplay, mode);
            Settings.hotMaxFahrenheit = TemperatureUtils.ConvertToFahrenheit(hotMaxDisplay, mode);

            listing.End();
        }
    }
}
