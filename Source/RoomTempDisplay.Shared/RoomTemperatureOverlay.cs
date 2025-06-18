using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RoomTempDisplay
{
    internal static class RoomTemperatureOverlay
    {
        // Fix for CS8370: Replace collection expressions with explicit instantiation  
        internal static readonly Dictionary<int, IntVec3> RoomLabelCache = new Dictionary<int, IntVec3>();

        private struct TempLabelData
        {
            public string Label;
            public Color Color;
            public float LastTemp;
        }

        private static readonly Dictionary<int, TempLabelData> TempLabelCache = new Dictionary<int, TempLabelData>();
        private const float TempChangeThreshold = 0.5f;

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

                if (overlayActive)
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

        internal static void RemoveLabelCache(int roomId)
        {
            RoomLabelCache.Remove(roomId);
            TempLabelCache.Remove(roomId);
        }
    }
}
