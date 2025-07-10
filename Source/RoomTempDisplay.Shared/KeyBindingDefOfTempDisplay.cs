using RimWorld;
using Verse;

namespace RoomTempDisplay
{
    /// <summary>
    /// Provides key binding definitions for toggling temperature display modes.
    /// </summary>
    /// <remarks>This class contains static references to key bindings used for toggling between different
    /// temperature display modes. It is automatically initialized at runtime to ensure the definitions are properly
    /// loaded.</remarks>
    [DefOf]
    public static class KeyBindingDefOfTempDisplay
    {
        /// <summary>
        /// Represents a key binding definition for toggling the room temperature display.
        /// </summary>
        /// <remarks>This key binding can be used to toggle the visibility of the room temperature display
        /// in the application. Ensure that the key binding is properly configured in the settings before use.</remarks>
        public static KeyBindingDef RTD_ToggleRoomTemp;

        /// <summary>
        /// Represents the key binding definition for toggling the vanilla temperature display.
        /// </summary>
        /// <remarks>This key binding allows users to toggle the visibility of the vanilla temperature
        /// display in the UI. It is typically used in scenarios where temperature information is relevant to
        /// gameplay.</remarks>
        public static KeyBindingDef RTD_ToggleVanillaTemp;

        /// <summary>
        /// Provides temporary definitions for key bindings used in the application.
        /// </summary>
        /// <remarks>This static constructor ensures that all key binding definitions are properly
        /// initialized before use. It is invoked automatically when the class is accessed for the first time.</remarks>
        static KeyBindingDefOfTempDisplay()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(KeyBindingDefOfTempDisplay));
        }
    }
}