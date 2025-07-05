using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace RoomTempDisplay
{
    /// <summary>
    /// A map component that stores manual overrides for room temperature display.
    /// </summary>
    /// <remarks>This component allows players to manually toggle the temperature display for specific rooms,
    /// enabling or disabling the temperature overlay as needed. It maintains two sets: one for rooms where the
    /// temperature display is manually turned on, and another for those turned off.</remarks>
    /// </summary>
    public class RoomTempOverrideMapComponent : MapComponent
    {
        public HashSet<int> manualOn = new HashSet<int>();
        public HashSet<int> manualOff = new HashSet<int>();

        /// <summary>
        /// Represents a component that manages room temperature overrides for a specific map.
        /// </summary>
        /// <remarks>This component is used to handle temperature override logic for rooms within the
        /// specified map. It extends the base functionality provided by the parent class to include map-specific
        /// behavior.</remarks>
        /// <param name="map">The map associated with this component. Cannot be null.</param>
        public RoomTempOverrideMapComponent(Map map) : base(map) { }

        /// <inheritdoc />
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref manualOn, "manualOn", LookMode.Value);
            Scribe_Collections.Look(ref manualOff, "manualOff", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                manualOn = manualOn ?? new HashSet<int>();
                manualOff = manualOff ?? new HashSet<int>();
            }
        }

        /// <inheritdoc />
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            FieldInfo lastRebuildField = typeof(RoomTemperatureOverlay)
                .GetField("_lastRebuildTick", BindingFlags.Static | BindingFlags.NonPublic);

            if (lastRebuildField != null)
            {
                const int minTickInterval = 20;
                lastRebuildField.SetValue(null, Find.TickManager.TicksGame - minTickInterval);
            }
        }
    }
}

