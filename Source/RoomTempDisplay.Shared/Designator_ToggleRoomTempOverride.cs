using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RoomTempDisplay
{
    /// <summary>
    /// Represents a designator that toggles the temperature override state for a room.
    /// </summary>
    /// <remarks>This designator allows the user to enable or disable a temperature override for a specific
    /// room. The override state is toggled when the user designates a valid cell within the room.</remarks>
    public class Designator_ToggleRoomTempOverride : Designator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Designator_ToggleRoomTempOverride"/> class.
        /// </summary>
        /// <remarks>This constructor sets up the default label, description, and icon for the toggle room
        /// temperature override designator. The label and description are localized using the translation system, and
        /// the icon is loaded from the specified content path.</remarks>
        public Designator_ToggleRoomTempOverride()
        {
            defaultLabel = "RoomTempDisplay_OverrideToggle_Label".Translate();
            defaultDesc = "RoomTempDisplay_OverrideToggle_Desc".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Buttons/RoomTemperature", true);
            useMouseIcon = true;
        }

        /// <summary>
        /// Determines whether the specified cell can be designated for a particular operation.
        /// </summary>
        /// <remarks>A cell is considered valid for designation if it belongs to a room that: <list
        /// type="bullet"> <item><description>Is not null.</description></item> <item><description>Has a valid ID (not
        /// 0).</description></item> <item><description>Contains more than one cell.</description></item>
        /// <item><description>Has no fogged cells.</description></item> </list> If the current map is null, the method
        /// will return <see langword="false"/>.</remarks>
        /// <param name="c">The cell to evaluate.</param>
        /// <returns>An <see cref="AcceptanceReport"/> indicating whether the cell can be designated.  Returns <see
        /// langword="true"/> if the cell is valid for designation; otherwise, <see langword="false"/>.</returns>
        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            Map map = Find.CurrentMap;
            if (map == null)
            {
                return false;
            }

            Room room = c.GetRoom(map);

            // Check if the room is valid for designation
            return room.IsRoomCandidate(map);
        }

        /// <summary>
        /// Designates a single cell for the temperature override toggle operation.
        /// </summary>
        /// <param name="c"></param>
        public override void DesignateSingleCell(IntVec3 c)
        {
            if (!CanDesignateCell(c).Accepted)
            {
                return;
            }

            Map map = Find.CurrentMap;
            if (map == null)
            {
                return;
            }

            Room room = c.GetRoom(map);
            if (room != null)
            {
                // Pass along the exact cell so we can store a stable, persistent key
                RoomTempOverrideState.Toggle(room, c);
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
        }
    }
}
