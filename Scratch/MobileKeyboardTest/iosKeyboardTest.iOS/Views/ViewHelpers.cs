using CoreGraphics;
using Foundation;
using UIKit;

namespace iosKeyboardTest.iOS {
    public static class ViewHelpers {
        public static CGSize TextSize(this UITextView tv) {
            var attr = new NSAttributedString(tv.Text, tv.Font);
            return attr.Size;
        }
        public static void Redraw(this UIView v) {
            v.Layer.SetNeedsDisplay();
            v.Layer.DisplayIfNeeded();
        }
        public static T SetDefaultProps<T>(this T uiv) where T: UIView {
            uiv.TranslatesAutoresizingMaskIntoConstraints = false;
            uiv.UserInteractionEnabled = false;
            uiv.ClipsToBounds = false;
            return uiv;
        }
        public static T SetDefaultTextProps<T>(this T uitv) where T: UITextView{
            uitv = uitv.SetDefaultProps();
            uitv.Selectable = false;
            uitv.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0);
            uitv.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
            uitv.TextContainer.MaximumNumberOfLines = 1;
            return uitv;
        }
    }
}