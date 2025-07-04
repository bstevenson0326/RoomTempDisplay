using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RoomTempDisplay
{
    /// <summary>
    /// Provides functionality to manage and toggle the override state of room temperature settings.
    /// </summary>
    /// <remarks>This class allows querying and modifying the override state of individual rooms in a map. 
    /// The override state determines whether a room's temperature settings are manually controlled  or follow the
    /// default behavior. The class also provides methods to clear manual overrides.</remarks>
    internal static class RoomTempOverrideState
    {
        private static RoomTempOverrideMapComponent Comp => Find.CurrentMap?.GetComponent<RoomTempOverrideMapComponent>();

        private static HashSet<int> ComputeDefaultIds(Map map)
        {
#if RW_1_5
            IReadOnlyList<Room> rooms = map.regionGrid.allRooms;
#else
            IReadOnlyList<Room> rooms = map.regionGrid.AllRooms;
#endif
            // Filter out rooms that are null, have an ID of 0, or have only one cell (which cannot be fogged).
            return rooms
                .Where(r =>
                    r != null
                    && r.ID != 0
                    && r.CellCount > 1
                    && r.Cells.All(c => !c.Fogged(map))
                )
                .Select(r => r.ID)
                .ToHashSet();
        }

        /// <summary>
        /// Determines whether the override is enabled for the specified room.
        /// </summary>
        /// <remarks>This method evaluates the override state of a room based on manual settings and
        /// default configurations.</remarks>
        /// <param name="room">The room to check. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the override is enabled for the specified room;  otherwise, <see
        /// langword="false"/>.</returns>
        internal static bool IsOverrideOn(Room room)
        {
            if (room == null || Comp == null)
            {
                return false;
            }

            int id = room.ID;
            if (Comp.manualOn.Contains(id))
            {
                return true;
            }

            if (Comp.manualOff.Contains(id))
            {
                return false;
            }

            HashSet<int> def = ComputeDefaultIds(room.Map);
            return def.Contains(id);
        }

        /// <summary>
        /// Toggles the override state of the specified room.
        /// </summary>
        /// <remarks>If the room is currently overridden, the method removes the override. If the room is
        /// not overridden,  the method applies an override. The behavior also considers whether the room would be shown
        /// by default  and adjusts the override state accordingly.</remarks>
        /// <param name="room">The room whose override state is to be toggled. Cannot be <see langword="null"/>.</param>
        internal static void Toggle(Room room)
        {
            if (room == null || Comp == null)
            {
                return;
            }

            int id = room.ID;
            Map map = room.Map;
            HashSet<int> defaultIds = ComputeDefaultIds(map);
            bool defaultWouldShow = defaultIds.Contains(id);
            bool currentlyOn = IsOverrideOn(room);

            if (currentlyOn)
            {
                Comp.manualOn.Remove(id);

                if (defaultWouldShow)
                {
                    Comp.manualOff.Add(id);
                }
                else
                {
                    Comp.manualOff.Remove(id);
                }
            }
            else
            {
                Comp.manualOff.Remove(id);
                Comp.manualOn.Add(id);
            }
        }

        /// <summary>
        /// Clears the manual on/off state for the specified room.
        /// </summary>
        /// <remarks>This method removes the specified room's ID from the manual on and manual off
        /// collections. If the <paramref name="room"/> is <see langword="null"/> or the component is uninitialized, the
        /// method does nothing.</remarks>
        /// <param name="room">The room whose manual on/off state should be cleared. Cannot be <see langword="null"/>.</param>
        internal static void Clear(Room room)
        {
            if (room == null || Comp == null)
            {
                return;
            }

            int id = room.ID;
            Comp.manualOn.Remove(id);
            Comp.manualOff.Remove(id);
        }
    }
}
