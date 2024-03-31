namespace MonkeyPaste {
    public static class MpShortcutExtensions {

        public static bool CanBeGlobal(this MpShortcutType st) {
            switch (st) {
                // APPLICATION
                case MpShortcutType.ToggleMainWindow:
                case MpShortcutType.ToggleAppendPaused:
                case MpShortcutType.ToggleAppendPositionMode:
                case MpShortcutType.ToggleAppendInlineMode:
                case MpShortcutType.ToggleAppendBlockMode:
                case MpShortcutType.ToggleAutoCopyMode:
                case MpShortcutType.ToggleRightClickPasteMode:
                case MpShortcutType.ToggleListenToClipboard:
                case MpShortcutType.ManuallyAddFromClipboard:
#if DEBUG
                case MpShortcutType.ToggleGlobalHooks: 
#endif

                // USER
                case MpShortcutType.PasteCopyItem:
                case MpShortcutType.InvokeTrigger:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsEditorShortcut(this MpShortcutType st) {
            switch (st) {
                case MpShortcutType.ToggleAppendInlineMode:
                case MpShortcutType.ToggleAppendBlockMode:
                case MpShortcutType.ToggleAppendPositionMode:
                case MpShortcutType.ToggleAppendPaused:
                case MpShortcutType.ToggleAppendDirectionMode:
                    return true;
                default:
                    return false;
            }
        }


        public static bool IsUserDefinedShortcut(this MpShortcutType stype) {
            return (int)stype > (int)MpShortcutType.MAX_APP_SHORTCUT;
        }

        public static MpRoutingType GetProfileBasedRoutingType(
            this MpShortcutRoutingProfileType profile,
            MpShortcutType st = MpShortcutType.None) {
            if (!st.CanBeGlobal()) {
                return MpRoutingType.Internal;
            }

            switch (profile) {
                case MpShortcutRoutingProfileType.Default:
                    //if (st == MpShortcutType.ToggleMainWindow) {
                    //    return MpRoutingType.ExclusiveOverride;
                    //}
                    return MpRoutingType.Passive;
                default:
                case MpShortcutRoutingProfileType.Internal:
                    return MpRoutingType.Internal;
            }
        }
    }
}
