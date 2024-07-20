using UIKit;

namespace iosKeyboardTest.iOS.KeyboardExt {
    public static class ViewHelpers {
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