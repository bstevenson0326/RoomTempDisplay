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
    /// <remarks>
    /// This class is responsible for displaying temperature labels and overlays for rooms in the
    /// game world. It manages caching of temperature data and label positions to optimize performance. The temperature
    /// display adapts to the current temperature mode (Celsius, Fahrenheit, or Kelvin) and can optionally use color
    /// coding to indicate temperature ranges (e.g., cold, comfortable, or hot).
    /// </remarks>
    internal static class RoomTemperatureOverlay
    {
        private struct TempLabelData
        {
            public string Label;
            public Color Color;
            public float LastTemp;
        }

        /// <summary>
        /// Represents a cache that maps room identifiers to their corresponding labels.
        /// </summary>
        /// <remarks>
        /// This dictionary is used to store precomputed room labels for quick lookup.  The key
        /// is the room's unique identifier, and the value is the label represented as an 
        /// <see cref="IntVec3"/>.
        /// </remarks>
        internal static readonly Dictionary<int, IntVec3> RoomLabelCache = new Dictionary<int, IntVec3>();
        private static readonly Dictionary<int, TempLabelData> TempLabelCache = new Dictionary<int, TempLabelData>();
        private const float TempChangeThreshold = 0.5f;

        /// <summary>
        /// Draws temperature labels for rooms on the map, based on the current temperature settings and display
        /// preferences.
        /// </summary>
        /// <remarks>
        /// This method renders temperature labels for rooms in the current map, provided that
        /// the temperature overlay or the custom room temperature toggle is enabled. Labels are displayed only for
        /// valid rooms that are fully roofed, not fogged, and do not use outdoor temperature. The labels are
        /// color-coded based on temperature ranges if the corresponding setting is enabled.  The method respects the
        /// current temperature display mode (Celsius, Fahrenheit, or Kelvin) and adjusts the label format accordingly.
        /// It also ensures that labels are only drawn for rooms visible within the camera's current view.  If the
        /// temperature overlay is active, larger labels are displayed at the center of each room. Otherwise, smaller
        /// labels are displayed at a designated label cell within the room.
        /// </remarks>
        internal static void DrawRoomTemperatures()
        {
            // Exit if world map is currently rendered
#if RW_1_5
            if (WorldRendererUtility.WorldRenderedNow)
            {
                return;
            }
#else
            if (WorldRendererUtility.CurrentWorldRenderMode == WorldRenderMode.Planet)
            {
                return;
            }
#endif
            if (!RoomTempToggleState.ShowTemperatures && !Find.PlaySettings.showTemperatureOverlay)
            {
                return;
            }

            Map map = Find.CurrentMap;
            if (map == null)
            {
                return;
            }

            bool overlayActive = Find.PlaySettings.showTemperatureOverlay;

#if RW_1_5
            foreach (Room room in map.regionGrid.allRooms)
#else
            foreach (Room room in map.regionGrid.AllRooms)
#endif
            {
                if (room == null
                    || room.UsesOutdoorTemperature
                    || room.CellCount <= 1
                    || !room.Cells.All(c => c.Roofed(map) && !c.Fogged(map)))
                {
                    continue;
                }

                float tempDisplay = GenTemperature.CelsiusTo(room.Temperature, Prefs.TemperatureMode);
                float tempF = GenTemperature.CelsiusTo(room.Temperature, TemperatureDisplayMode.Fahrenheit);

                if (!TempLabelCache.TryGetValue(room.ID, out TempLabelData cachedData) || Mathf.Abs(cachedData.LastTemp - tempDisplay) > TempChangeThreshold)
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

                        var coldBlue = new Color(0.4f, 0.6f, 1f);
                        var hotRed = new Color(1f, 0.4f, 0.3f);

                        if (tempF < comfortMinF)
                        {
                            float t = Mathf.InverseLerp(coldMinF, comfortMinF, tempF);
                            color = Color.Lerp(coldBlue, Color.white, t);
                        }
                        else if (tempF > comfortMaxF)
                        {
                            float t = Mathf.InverseLerp(comfortMaxF, hotMaxF, tempF);
                            color = Color.Lerp(Color.white, hotRed, t);
                        }
                    }

                    cachedData = new TempLabelData
                    {
                        Label = label,
                        Color = color,
                        LastTemp = tempDisplay
                    };

                    TempLabelCache[room.ID] = cachedData;
                }

                if (RoomTempToggleState.ShowTemperatures && !overlayActive)
                {
                    IntVec3 labelCell = GetLabelCell(room);
                    if (labelCell.IsValid && Find.CameraDriver.CurrentViewRect.ExpandedBy(1).Contains(labelCell))
                    {
                        Vector2 screenPos = GenMapUI.LabelDrawPosFor(labelCell);
                        Text.Font = GameFont.Tiny;
                        GUI.color = cachedData.Color;
                        Widgets.Label(new Rect(screenPos.x - 15f, screenPos.y - 7f, 50f, 20f), cachedData.Label);
                    }
                }

                if (overlayActive && RoomTempDisplayMod.Settings.showOverlayText)
                {
                    IntVec3 center = room.ExtentsClose.CenterCell;
                    if (center.IsValid && Find.CameraDriver.CurrentViewRect.ExpandedBy(1).Contains(center))
                    {
                        Vector2 screenPos = GenMapUI.LabelDrawPosFor(center);
                        Text.Font = GameFont.Medium;
                        GUI.color = cachedData.Color;
                        Widgets.Label(new Rect(screenPos.x - 20f, screenPos.y - 12f, 100f, 30f), cachedData.Label);
                    }
                }

                GUI.color = Color.white;
            }
        }

        /// <summary>
        /// Determines the most suitable cell to use as the label position for the specified room.
        /// </summary>
        /// <param name="room">The room for which the label cell is being determined. Must not be null.</param>
        /// <returns>The <see cref="IntVec3"/> representing the label cell for the room. If no valid cell is found,  returns an
        /// invalid <see cref="IntVec3"/> instance.</returns>
        /// <remarks>
        /// The label cell is selected based on specific criteria, prioritizing border cells that
        /// are within  bounds, roofed, and have an edifice. If no such cell is found, fallback criteria are applied, 
        /// including checking other roofed border cells or roofed cells within the room. The result is cached  for
        /// future calls to improve performance.
        /// </remarks>
        private static IntVec3 GetLabelCell(Room room)
        {
            if (RoomLabelCache.TryGetValue(room.ID, out IntVec3 cached) && cached.IsValid)
            {
                return cached;
            }

            Map map = room.Map;
            IntVec3 cell = room.BorderCells
                .Where(c => c.InBounds(map) && c.Roofed(map) && c.GetEdifice(map) != null)
                .OrderByDescending(c => c.z)
                .ThenByDescending(c => c.x)
                .FirstOrDefault();

            if (!cell.IsValid)
            {
                cell = room.BorderCells
                    .Where(c => c.InBounds(map) && c.Roofed(map))
                    .OrderByDescending(c => c.z)
                    .ThenByDescending(c => c.x)
                    .FirstOrDefault();
            }

            if (!cell.IsValid)
            {
                cell = room.Cells
                    .Where(c => c.InBounds(map) && c.Roofed(map))
                    .OrderByDescending(c => c.z)
                    .ThenByDescending(c => c.x)
                    .FirstOrDefault();
            }

            if (cell.IsValid)
            {
                RoomLabelCache[room.ID] = cell;
            }

            return cell;
        }

        /// <summary>
        /// Removes the cached labels associated with the specified room ID.
        /// </summary>
        /// <param name="roomId">The unique identifier of the room whose labels should be removed from the cache.</param>
        /// <remarks>
        /// This method clears both the room label cache and the temporary label cache for the specified room ID.
        /// </remarks>
        internal static void RemoveLabelCache(int roomId)
        {
            RoomLabelCache.Remove(roomId);
            TempLabelCache.Remove(roomId);
        }
    }
}
