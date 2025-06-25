using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RoomTempDisplay.Patch
{
    /// <summary>
    /// Provides Harmony patches for modifying the behavior of the game to support room temperature display
    /// functionality.
    /// </summary>
    [StaticConstructorOnStartup]
    internal static class HarmonyPatches
    {
        /// <summary>
        /// Initializes and applies Harmony patches for the Room Temperature Display mod.
        /// </summary>
        /// <remarks>
        /// This static constructor sets up Harmony patches to modify the behavior of specific
        /// methods in the game. 
        /// It applies the following patches: 
        /// <list type="bullet"> 
        /// <item> 
        /// A postfix patch to <see cref="MapInterface.MapInterfaceOnGUI_AfterMainTabs"/> to draw room temperature overlays. 
        /// </item> 
        /// <item> 
        /// A postfix patch to <c>PlaySettings.DoPlaySettingsGlobalControls</c> to add a toggle for room temperature
        /// display in the global settings menu. 
        /// </item> 
        /// </list> 
        /// If the <c>PlaySettings.DoPlaySettingsGlobalControls</c> method cannot be found, a warning is logged.
        /// </remarks>
        static HarmonyPatches()
        {
            var harmony = new Harmony("com.hawqeye19.RoomTempDisplay");

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

            Log.Message("[RoomTempDisplay] Initialized.");
        }

        /// <summary>
        /// A patch for the <see cref="Room.Notify_RoomShapeChanged"/> method that ensures the label cache for the
        /// room's temperature overlay is cleared when the room's shape changes.
        /// </summary>
        /// <remarks>
        /// This patch is applied to the <see cref="Room.Notify_RoomShapeChanged"/> method to
        /// maintain consistency in the temperature overlay by removing outdated label cache entries whenever the room's
        /// shape is modified.
        /// </remarks>
        [HarmonyPatch(typeof(Room), nameof(Room.Notify_RoomShapeChanged))]
        internal static class RoomNotifyRoomShapeChangedPatch
        {
            internal static void Postfix(Room __instance)
            {
                RoomTemperatureOverlay.RemoveLabelCache(__instance.ID);
            }
        }

        /// <summary>
        /// A Harmony patch for the <see cref="PlaySettings.DoPlaySettingsGlobalControls"/> method that adds a toggle
        /// button to display room temperatures in the game UI.
        /// </summary>
        /// <remarks>
        /// This patch modifies the global play settings controls to include a button for toggling the display of room 
        /// temperatures. The button is not shown when the world view is active or when the world is being rendered.
        /// </remarks>
        [HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
        public static class PlaySettings_RoomTempToggle_Patch
        {
            public static void Postfix(WidgetRow row, bool worldView)
            {
                // Do not show the button in world view or if the world is being rendered
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

                TooltipHandler.TipRegion(buttonRect, "RoomTempDisplay_Label_Toggle".Translate());

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
