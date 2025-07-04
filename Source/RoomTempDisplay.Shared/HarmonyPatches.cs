using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RoomTempDisplay.Patch
{
    /// <summary>
    /// Applies Harmony patches to modify the behavior of the game at runtime.
    /// </summary>
    /// <remarks>This static class is responsible for initializing and applying Harmony patches to specific
    /// methods in the game. It uses the Harmony library to inject custom functionality, such as displaying room
    /// temperatures on the map interface. The patches are applied during the static constructor, ensuring they are set
    /// up as soon as the class is loaded.  If the target method <see cref="MapInterface.MapInterfaceOnGUI_AfterMainTabs"/> 
    /// cannot be found, a warning is logged, and the patch for room temperature display will not be applied. This ensures 
    /// graceful handling of potential version mismatches or missing methods.</remarks>
    [StaticConstructorOnStartup]
    internal static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("com.hawqeye19.RoomTempDisplay");
            harmony.PatchAll();

            System.Reflection.MethodInfo drawMethod = AccessTools.Method(
                typeof(MapInterface),
                nameof(MapInterface.MapInterfaceOnGUI_AfterMainTabs)
            );

            if (drawMethod != null)
            {
                harmony.Patch(
                    drawMethod,
                    postfix: new HarmonyMethod(
                        typeof(RoomTemperatureOverlay),
                        nameof(RoomTemperatureOverlay.DrawRoomTemperatures)
                    )
                );
            }
            else
            {
                Log.Warning("[RoomTempDisplay] Could not find MapInterface.MapInterfaceOnGUI_AfterMainTabs to patch.");
            }

            Log.Message("[RoomTempDisplay] initialized.");
        }
    }

    /// <summary>
    /// A patch applied to the <see cref="Room.Notify_RoomShapeChanged"/> method to handle additional behavior when the
    /// shape of a room changes.
    /// </summary>
    /// <remarks>This patch ensures that the label cache for the room's temperature overlay is cleared
    /// whenever the room's shape changes. This helps maintain accurate and up-to-date information in the
    /// overlay.</remarks>
    [HarmonyPatch(typeof(Room), nameof(Room.Notify_RoomShapeChanged))]
    internal static class RoomNotifyRoomShapeChangedPatch
    {
        internal static void Postfix(Room __instance)
        {
            RoomTemperatureOverlay.RemoveLabelCache(__instance.ID);
        }
    }

    /// <summary>
    /// A Harmony patch for the <see cref="PlaySettings.DoPlaySettingsGlobalControls"/> method that adds a toggle button
    /// to display room temperatures in the game UI.
    /// </summary>
    /// <remarks>This patch modifies the global play settings controls to include a button for toggling the
    /// display of room temperatures. The button is only displayed when the world view is not active. Clicking the
    /// button toggles the state of <see cref="RoomTempToggleState.ShowTemperatures"/> and plays a click sound. A
    /// tooltip is also provided for the button.</remarks>
    [HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
    public static class PlaySettings_RoomTempToggle_Patch
    {
        public static void Postfix(WidgetRow row, bool worldView)
        {
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
                const float size = 12f;
                var checkRect = new Rect(buttonRect.xMax - size, buttonRect.yMin, size, size);
                GUI.DrawTexture(checkRect, checkmark);
            }
        }
    }

    /// <summary>
    /// A Harmony patch for the <see cref="DesignationCategoryDef.ResolvedAllowedDesignators"/> property getter.
    /// </summary>
    /// <remarks>This patch modifies the list of allowed designators for the "Orders" designation category by
    /// adding a custom designator.</remarks>
    [HarmonyPatch(typeof(DesignationCategoryDef))]
    [HarmonyPatch("ResolvedAllowedDesignators", MethodType.Getter)]
    public static class DesignationCategoryDef_ResolvedAllowedDesignators_Patch
    {
        public static void Postfix(ref IEnumerable<Designator> __result, DesignationCategoryDef __instance)
        {
            if (__result == null)
            {
                return;
            }

            if (__instance.defName != "Orders")
            {
                return;
            }

            var list = __result.ToList();
            list.Add(new Designator_ToggleRoomTempOverride());
            __result = list;
        }
    }
}
