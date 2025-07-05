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
        private static Map _cachedMap;
        private static HashSet<int> _cachedDefaultIds;

        private static int CellToIndex(IntVec3 c, Map map) => c.z * map.Size.x + c.x;

        private static IntVec3 IndexToCell(int idx, Map map) => new IntVec3(idx % map.Size.x, 0, idx / map.Size.x);

        /// <summary>
        /// Clears the cached default IDs and map reference.
        /// </summary>
        internal static void ClearDefaultCache()
        {
            _cachedMap = null;
            _cachedDefaultIds = null;
        }

        /// <summary>
        /// Gets the cached default IDs for the specified map.
        /// </summary>
        /// <param name="map">The map</param>
        /// <returns>Returns the cached IDs</returns>
        private static HashSet<int> GetDefaultIdsCached(Map map)
        {
            if (_cachedDefaultIds == null || _cachedMap != map)
            {
                _cachedDefaultIds = ComputeDefaultIds(map);
                _cachedMap = map;
            }

            return _cachedDefaultIds;
        }

        private static HashSet<int> ComputeDefaultIds(Map map)
        {
#if RW_1_5
            IReadOnlyList<Room> rooms = map.regionGrid.allRooms;
#else
            IReadOnlyList<Room> rooms = map.regionGrid.AllRooms;
#endif
            // Filter rooms that are not null, have a valid ID, are proper rooms, have more than one cell, and are not fogged.
            return rooms.Where(r => r.IsRoomCandidate(map))
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

            if (Comp.manualOn.Any(idx => room.ContainsCell(IndexToCell(idx, room.Map))))
            {
                return true;
            }

            if (Comp.manualOff.Any(idx => room.ContainsCell(IndexToCell(idx, room.Map))))
            {
                return false;
            }

            // very cheap, cached:
            return GetDefaultIdsCached(room.Map).Contains(room.ID);
        }

        /// <summary>
        /// Toggles the override state of the specified room.
        /// </summary>
        /// <remarks>If the room is currently overridden, the method removes the override. If the room is
        /// not overridden,  the method applies an override. The behavior also considers whether the room would be shown
        /// by default  and adjusts the override state accordingly.</remarks>
        /// <param name="room">The room whose override state is to be toggled. Cannot be <see langword="null"/>.</param>
        internal static void Toggle(Room room, IntVec3 clickedCell)
        {
            if (room == null || Comp == null)
            {
                return;
            }

            int id = room.ID;
            Map map = room.Map;
            HashSet<int> defaultIds = ComputeDefaultIds(map);
            bool defaultWouldShow = defaultIds.Contains(room.ID);
            bool currentlyOn = IsOverrideOn(room);
            int idx = CellToIndex(clickedCell, map);

            if (currentlyOn)
            {
                Comp.manualOn.RemoveWhere(i => room.ContainsCell(IndexToCell(i, map)));
                if (defaultWouldShow)
                {
                    Comp.manualOff.Add(idx);
                }
                else
                {
                    Comp.manualOff.Remove(idx);
                }
            }
            else
            {
                Comp.manualOff.RemoveWhere(i => room.ContainsCell(IndexToCell(i, map)));
                Comp.manualOn.Add(idx);
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

            Comp.manualOn.RemoveWhere(i => room.ContainsCell(IndexToCell(i, room.Map)));
            Comp.manualOff.RemoveWhere(i => room.ContainsCell(IndexToCell(i, room.Map)));
        }
    }
}
