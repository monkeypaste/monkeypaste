using Avalonia.Controls;
using Avalonia.iOS;
using Avalonia.Layout;
using Avalonia.Media;
using CoreGraphics;
using EventKit;
using Foundation;
using ObjCRuntime;
using System;
using System.Collections.Generic;
using UIKit;

namespace iosKeyboardTest.iOS.KeyboardExt
{
    public partial class KeyboardViewController : UIInputViewController, iosIKeyboardInputConnection {    
        #region Private Variables
        UIButton nextKeyboardButton;
        //UIStackView stv;
        //UIView av;
       //DateTime ctorDt = default, loadDt = default;
        #endregion

        #region Constants
        #endregion

        #region Statics
        //static AvaloniaView KeyboardView { get; set; }
        //static KeyboardViewModel KeyboardViewModel =>
        //    KeyboardView == null || KeyboardView.Content == null ?
        //        null :
        //        (KeyboardView.Content as Control).DataContext as KeyboardViewModel;

        //public static void CreateKeyboardView() {
            //var kbv = KeyboardViewModel.CreateKeyboardView(null, iosDisplayInfo.ScaledSize);
            //KeyboardView = new AvaloniaView() {
            //    TranslatesAutoresizingMaskIntoConstraints = true,
            //    Content = kbv
            //};
            //var kb_size = new CGSize(kbv.Width * iosDisplayInfo.Scaling, kbv.Height * iosDisplayInfo.Scaling);
            //KeyboardView.Frame = new CGRect(new(), kb_size);
        //}
        #endregion

        #region Interfaces

        #region iosIKeyboardInputConnection Implementation
        bool iosIKeyboardInputConnection.NeedsInputModeSwitchKey =>
            this.NeedsInputModeSwitchKey;

        public void OnInputModeSwitched() {
            AdvanceToNextInputMode();
        }
        public void OnText(string text) {
            TextDocumentProxy.InsertText(text);
        }

        public void OnDelete() {
            TextDocumentProxy.DeleteBackward();
        }

        public void OnDone() {
            View.EndEditing(true);
        }
        #endregion
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public KeyboardViewController(IntPtr handle) : base(handle) {
            //av = CreateKeyboardView();
            //ctorDt = DateTime.Now;
            //if(KeyboardViewModel != null) {
            //    KeyboardViewModel.SetInputConnection(this);
            //}
        }
        #endregion

        #region Public Methods
        public override void DidReceiveMemoryWarning() {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public override void UpdateViewConstraints() {
            base.UpdateViewConstraints();

            // Add custom view sizing constraints here
        }

        public override void ViewDidLoad() {
            base.ViewDidLoad();
            //loadDt = DateTime.Now;
            //if(View is UIStackView) {
            //    return;
            //}
            //View = CreateKeyboardView();
            //View.AddSubview(av);
            //var nextKeyboardButtonLeftSideConstraint = NSLayoutConstraint.Create(av, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f);
            //var nextKeyboardButtonBottomConstraint = NSLayoutConstraint.Create(av, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f);
            //View.AddConstraints(new[] { nextKeyboardButtonLeftSideConstraint, nextKeyboardButtonBottomConstraint });


            nextKeyboardButton = new UIButton(UIButtonType.System);
            nextKeyboardButton.SetTitle($"Startup time: ", UIControlState.Normal);
            nextKeyboardButton.SizeToFit();
            nextKeyboardButton.TranslatesAutoresizingMaskIntoConstraints = false;
            nextKeyboardButton.AddTarget(this, new Selector("advanceToNextInputMode"), UIControlEvent.TouchUpInside);
            
            //Console.WriteLine($"ViewDidLoad called. View Rect: '{View.Bounds}'");
            //stv = new UIStackView() {
            //    BackgroundColor = UIColor.Magenta,
            //    Axis = UILayoutConstraintAxis.Vertical,
            //    TranslatesAutoresizingMaskIntoConstraints = false,
            //};
            ////var kbv = CreateKeyboardView(out var size);
            ////stv.AddArrangedSubview(kbv);
            //var size = KeyboardView.Frame.Size;
            //stv.AddArrangedSubview(nextKeyboardButton);
            //stv.Frame = new CGRect(0, 0, size.Width, size.Height + 50);

            var subView = nextKeyboardButton;

            View.AddSubview(subView);

            View.AddConstraints(new NSLayoutConstraint[] {
                CreateConstraint(subView, View, NSLayoutAttribute.Left),
                //CreateConstraint(subView, View, NSLayoutAttribute.Top),
                //CreateConstraint(subView, View, NSLayoutAttribute.Right),
                CreateConstraint(subView, View, NSLayoutAttribute.Bottom)
            });
        }
        public override void TextWillChange(IUITextInput textInput) {
            // The app is about to change the document's contents. Perform any preparation here.
            //base.TextWillChange(textInput);
        }

        public override void TextDidChange(IUITextInput textInput) {
            // The app has just changed the document's contents, the document context has been updated.
            UIColor textColor = null;

            if (TraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark) {
                textColor = UIColor.White;
            } else {
                textColor = UIColor.Black;
            }

            nextKeyboardButton.SetTitleColor(textColor, UIControlState.Normal);
            //base.TextDidChange(textInput);
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private NSLayoutConstraint CreateConstraint(UIView view, UIView parentView, NSLayoutAttribute attr) {
            return NSLayoutConstraint.Create(view, attr, NSLayoutRelation.Equal, parentView, attr, 1f, 0f);
        }
        #endregion

        #region Commands
        #endregion
    }
}
