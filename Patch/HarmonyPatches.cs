using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RoomTempDisplay.Patch
{
    [StaticConstructorOnStartup]
    internal static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("com.hawqeye19.roomtemperaturedisplay");

            // Patch for tab label display
            harmony.Patch(
                AccessTools.Method(typeof(MapInterface), nameof(MapInterface.MapInterfaceOnGUI_AfterMainTabs)),
                postfix: new HarmonyMethod(typeof(RoomTemperatureOverlay), nameof(RoomTemperatureOverlay.DrawRoomTemperatures))
            );

            System.Reflection.MethodInfo playSettingsMethod = AccessTools.Method(typeof(PlaySettings), "DoPlaySettingsGlobalControls");
            if (playSettingsMethod != null)
            {
                harmony.Patch(
                    playSettingsMethod,
                    postfix: new HarmonyMethod(typeof(PlaySettings_RoomTempToggle_Patch), nameof(PlaySettings_RoomTempToggle_Patch.Postfix))
                );
            }
            else
            {
                Log.Warning("[RoomTempDisplay] Failed to find PlaySettings.DoPlaySettingsGlobalControls.");
            }

            Log.Message("[RoomTempDisplay] Harmony patches applied.");
        }

        [HarmonyPatch(typeof(Room), nameof(Room.Notify_RoomShapeChanged))]
        internal static class RoomNotifyRoomShapeChangedPatch
        {
            internal static void Postfix(Room __instance)
            {
                RoomTemperatureOverlay.RemoveLabelCache(__instance.ID);
            }
        }

        [HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
        public static class PlaySettings_RoomTempToggle_Patch
        {
            public static void Postfix(WidgetRow row, bool worldView)
            {
                Texture2D icon = ContentFinder<Texture2D>.Get("UI/Buttons/RoomTemperature", true);
                Texture2D checkmark = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOn", true);

                if (icon == null || checkmark == null)
                {
                    return;
                }

                Rect buttonRect = row.Icon(icon);

                if (Widgets.ButtonImage(buttonRect, icon))
                {
                    RoomTempToggleState.ShowTemperatures = !RoomTempToggleState.ShowTemperatures;
                    SoundDefOf.Click.PlayOneShotOnCamera();
                }

                TooltipHandler.TipRegion(buttonRect, "Toggle Room Temperature Display");

                if (RoomTempToggleState.ShowTemperatures)
                {
                    float size = 12f;
                    var checkRect = new Rect(
                        buttonRect.xMax - size,
                        buttonRect.yMin,
                        size,
                        size);
                    GUI.DrawTexture(checkRect, checkmark);
                }
            }
        }
    }
}
