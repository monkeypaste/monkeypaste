using Avalonia.Controls;

namespace MonkeyPaste.Avalonia.Behaviors._Factory {
    public static class MpAvViewBehaviorFactory {
        public static void BuildAllViewBehaviors(Control view, Control controlToAttach) {
            // NOTE this should be called after/in view's attachedToVisualTree

            // DROP BEHAVIORS
            if (view is MpAvClipTileContentView ctcv) {
                return;

                ctcv.ContentViewDropBehavior = new MpAvContentViewDropBehavior();
                ctcv.ContentViewDropBehavior.Attach(controlToAttach);
            } else if (view is MpAvPinTrayView ptv) {
                return;
                //ptv.PinTrayDropBehavior = new MpAvPinTrayDropBehavior();
                //ptv.PinTrayDropBehavior.Attach(controlToAttach);
            }


            // HIGHLIGHTING

            if (view is MpAvClipTileTitleView cttv) {
                return;
                cttv.ClipTileTitleHighlightBehavior = new MpAvClipTileTitleHighlightBehavior();
                cttv.ClipTileTitleHighlightBehavior.Attach(controlToAttach);

                cttv.SourceHighlightBehavior = new MpAvSourceHighlightBehavior();
                cttv.SourceHighlightBehavior.Attach(controlToAttach);
            } else if (view is MpAvClipTileContentView ctcv_hl) {
                return;
                ctcv_hl.HighlightBehavior = new MpAvContentHighlightBehavior();
                ctcv_hl.HighlightBehavior.Attach(controlToAttach);
            }

        }
    }
}
