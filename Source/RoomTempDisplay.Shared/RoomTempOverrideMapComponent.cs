using System.Collections.Generic;
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

        /// <summary>
        /// Handles the serialization and deserialization of the object's state, including manual on/off settings.
        /// </summary>
        /// <remarks>This method ensures that the <c>manualOn</c> and <c>manualOff</c> collections are
        /// properly saved and loaded. During the loading process, if the collections are null, they are initialized to
        /// empty sets.</remarks>
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
    }
}

