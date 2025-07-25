using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RoomTempDisplay
{
    /// <summary>
    /// Provides functionality for rendering room temperature overlays in the game.
    /// </summary>
    /// <remarks>This class is responsible for displaying temperature labels and color-coded overlays for
    /// rooms based on their current temperature. It supports both custom temperature overlays and the vanilla
    /// temperature overlay, depending on the user's settings. The overlay dynamically updates to reflect temperature
    /// changes and uses caching to optimize performance.</remarks>
    internal static class RoomTemperatureOverlay
    {
        private struct TempLabelData
        {
            public string Label;
            public Color Color;
            public float LastTemp;
        }

        internal static readonly Dictionary<int, IntVec3> RoomLabelCache = new Dictionary<int, IntVec3>();
        private static readonly Dictionary<int, TempLabelData> TempLabelCache = new Dictionary<int, TempLabelData>();
        private const float TempChangeThreshold = 0.5f;
        private static readonly Color ColdBlue = new Color(0.4f, 0.6f, 1f);
        private static readonly Color HotRed = new Color(1f, 0.4f, 0.3f);

        // Throttling variables to limit how often we rebuild the temperature labels
        private static float _lastRebuildTime = -2f;

        /// <summary>
        /// Draws temperature labels for rooms on the map, based on the current temperature settings and display
        /// preferences.
        /// </summary>
        /// <remarks>This method displays temperature information for rooms on the map, either as an
        /// overlay or as labels, depending on the user's settings. It respects the current temperature mode (Celsius,
        /// Fahrenheit, or Kelvin) and applies color coding to indicate temperature ranges if enabled. The method does
        /// not render labels if the temperature overlay is disabled or if the map is not currently active.</remarks>
        internal static void DrawRoomTemperatures()
        {
#if RW_1_5
            if (WorldRendererUtility.WorldRenderedNow)
            {
                return;
            }
#else
            if (WorldRendererUtility.CurrentWorldRenderMode == WorldRenderMode.Planet || WorldComponent_GravshipController.CutsceneInProgress)
            {
                return;
            }
#endif
            bool isVanillaOverlay = Find.PlaySettings.showTemperatureOverlay;
            bool isShowTemperaturesOn = RoomTempToggleState.ShowTemperatures;
            if (!isShowTemperaturesOn && !isVanillaOverlay)
            {
                return;
            }

            Map map = Find.CurrentMap;
            if (map == null)
            {
                return;
            }

            // Throttle rebuilds to avoid performance issues
            bool isRebuilding = ShouldRebuildOverlay();

#if RW_1_5
            List<Room> rooms = map.regionGrid.allRooms;
#else
            IReadOnlyList<Room> rooms = map.regionGrid.AllRooms;
#endif

            foreach (Room room in rooms)
            {
                if (!RoomTempOverrideState.IsOverrideOn(room))
                {
                    // if we’re hiding this room, make sure its caches are cleared
                    RemoveRoomCache(room?.ID ?? -1);
                    continue;
                }

                float tempDisplay = GenTemperature.CelsiusTo(room.Temperature, Prefs.TemperatureMode);
                float tempF = GenTemperature.CelsiusTo(room.Temperature, TemperatureDisplayMode.Fahrenheit);

                // If the temperature labels are enabled, cache the label and color for the room
                if (isRebuilding && (!TempLabelCache.TryGetValue(room.ID, out TempLabelData cachedData)
                        || Mathf.Abs(cachedData.LastTemp - tempDisplay) > TempChangeThreshold))
                {
                    string suffix = Prefs.TemperatureMode == TemperatureDisplayMode.Fahrenheit ? "°F" :
                                    Prefs.TemperatureMode == TemperatureDisplayMode.Kelvin ? "K" : "°C";
                    string label = $"{Mathf.RoundToInt(tempDisplay)}{suffix}";

                    Color color = Color.white;

                    if (RoomTempDisplayMod.Settings.showTemperatureRangeColors)
                    {
                        float coldMinF = RoomTempDisplayMod.Settings.coldMinFahrenheit;
                        float comfortMinF = RoomTempDisplayMod.Settings.minComfortableFahrenheit;
                        float comfortMaxF = RoomTempDisplayMod.Settings.maxComfortableFahrenheit;
                        float hotMaxF = RoomTempDisplayMod.Settings.hotMaxFahrenheit;

                        if (tempF < comfortMinF)
                        {
                            float t = Mathf.InverseLerp(coldMinF, comfortMinF, tempF);
                            color = Color.Lerp(ColdBlue, Color.white, t);
                        }
                        else if (tempF > comfortMaxF)
                        {
                            float t = Mathf.InverseLerp(comfortMaxF, hotMaxF, tempF);
                            color = Color.Lerp(Color.white, HotRed, t);
                        }
                    }

                    TempLabelCache[room.ID] = new TempLabelData
                    {
                        Label = label,
                        Color = color,
                        LastTemp = tempDisplay
                    };
                }

                if (TempLabelCache.TryGetValue(room.ID, out TempLabelData data))
                {
                    // If the temperature labels are enabled, draw the label on the room's label cell
                    if (isShowTemperaturesOn && !isVanillaOverlay)
                    {
                        IntVec3 labelCell = GetLabelCell(room);
                        if (labelCell.IsValid && Find.CameraDriver.CurrentViewRect.ExpandedBy(1).Contains(labelCell))
                        {
                            Vector2 screenPos = GenMapUI.LabelDrawPosFor(labelCell);
                            Text.Font = GameFont.Tiny;
                            GUI.color = data.Color;
                            Widgets.Label(new Rect(screenPos.x - 15f, screenPos.y - 7f, 50f, 20f), data.Label);
                        }
                    }

                    // If the vanilla overlay is enabled, draw the overlay color on the room's cells
                    if (isVanillaOverlay && RoomTempDisplayMod.Settings.showOverlayText)
                    {
                        IntVec3 center = room.ExtentsClose.CenterCell;
                        if (center.IsValid && Find.CameraDriver.CurrentViewRect.ExpandedBy(1).Contains(center))
                        {
                            Vector2 screenPos = GenMapUI.LabelDrawPosFor(center);
                            Text.Font = GameFont.Medium;
                            GUI.color = data.Color;
                            Widgets.Label(new Rect(screenPos.x - 20f, screenPos.y - 12f, 100f, 30f), data.Label);
                        }
                    }
                }

                GUI.color = Color.white;
            }
        }

        /// <summary>
        /// Removes cached data for a specific room.
        /// </summary>
        /// <param name="roomId"></param>
        private static void RemoveRoomCache(int roomId)
        {
            if (RoomLabelCache.ContainsKey(roomId))
            {
                RoomLabelCache.Remove(roomId);
            }

            if (TempLabelCache.ContainsKey(roomId))
            {
                TempLabelCache.Remove(roomId);
            }
        }

        /// <summary>
        /// Determines the most suitable cell to use as the label position for the specified room.
        /// </summary>
        /// <remarks>The method prioritizes the label cell selection in the following order: 1. A cached
        /// label cell, if it is still valid. 2. A border cell containing an edifice. 3. Any valid border cell. 4. Any
        /// valid interior cell.  The result is cached for future calls to improve performance.</remarks>
        /// <param name="room">The room for which to determine the label cell. Cannot be null.</param>
        /// <returns>The <see cref="IntVec3"/> representing the label cell for the room.  If no valid cell is found, returns an
        /// invalid <see cref="IntVec3"/> instance.</returns>
        private static IntVec3 GetLabelCell(Room room)
        {
            if (RoomLabelCache.TryGetValue(room.ID, out IntVec3 cachedCell) && cachedCell.IsValid)
            {
                return cachedCell;
            }

            Map map = room.Map;
            IntVec3 cell = IntVec3.Invalid;

            // Try to find a border cell with an edifice first
            cell = room.BorderCells
                       .Where(c => c.InBounds(map) && c.GetEdifice(map) != null)
                       .OrderByDescending(c => c.z)
                       .ThenByDescending(c => c.x)
                       .FirstOrDefault();

            // If no such cell is found, try to find any valid border cell
            if (!cell.IsValid)
            {
                cell = room.BorderCells
                           .Where(c => c.InBounds(map))
                           .OrderByDescending(c => c.z)
                           .ThenByDescending(c => c.x)
                           .FirstOrDefault();
            }

            // If still no valid cell, try to find any valid interior cell
            if (!cell.IsValid)
            {
                cell = room.Cells
                           .Where(c => c.InBounds(map))
                           .OrderByDescending(c => c.z)
                           .ThenByDescending(c => c.x)
                           .FirstOrDefault();
            }

            // If we found a valid cell, cache it
            if (cell.IsValid)
            {
                RoomLabelCache[room.ID] = cell;
            }

            return cell;
        }

        /// <summary>
        /// Determines whether the overlay should be rebuilt based on the current game state and timing conditions.
        /// </summary>
        /// <remarks>The overlay will not be rebuilt if the game is paused or if the last rebuild occurred
        /// less than two seconds ago.</remarks>
        /// <returns><see langword="true"/> if the overlay should be rebuilt; otherwise, <see langword="false"/>.</returns>
        private static bool ShouldRebuildOverlay()
        {
            // If the game is paused, do not rebuild the overlay
            if (Time.timeScale == 0f)
            {
                return false;
            }

            // If the last rebuild was too recent, do not rebuild
            float now = Time.realtimeSinceStartup;
            if (now - _lastRebuildTime >= 2f)
            {
                _lastRebuildTime = now;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the cached labels associated with the specified room ID.
        /// </summary>
        /// <param name="roomId">The unique identifier of the room whose labels should be removed from the cache.</param>
        /// <remarks>
        /// This method clears both the room label cache and the temporary label cache for the specified room ID.
        /// </remarks>
        internal static void RemoveLabelCache(int roomId) => RemoveRoomCache(roomId);
    }
}
