using System;
using Foundation;
using UIKit;
using NotificationCenter;
using CoreGraphics;
using MobileCoreServices;
using Social;
using CoreFoundation;

namespace MpShareTextExtension {
    [Register("MpShareTextExtensionViewController")]
    public class MpShareTextExtensionViewController : SLComposeServiceViewController
    {
        public static EventHandler<string> OnShareText;

        protected MpShareTextExtensionViewController(IntPtr handle) : base(handle) {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void DidReceiveMemoryWarning() {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad() {
            base.ViewDidLoad();

            OnShareText?.Invoke(this, ContentText + " " + ExtensionContext.InputItems[0].ToString());
            // Add label to view
            // var TodayMessage = new UILabel(new CGRect(0, 0, View.Frame.Width, View.Frame.Height)) {
            //    TextAlignment = UITextAlignment.Center
            //};

            //View.AddSubview(TodayMessage);



            //// Calculate the values
            //var dayOfYear = DateTime.Now.DayOfYear;
            //var leapYearExtra = DateTime.IsLeapYear(DateTime.Now.Year) ? 1 : 0;
            //var daysRemaining = 365 + leapYearExtra - dayOfYear;

            //// Display the message
            //if (daysRemaining == 1) {
            //    TodayMessage.Text = String.Format("Today is day {0}. There is one day remaining in the year.", dayOfYear);
            //} else {
            //    TodayMessage.Text = String.Format("Today is day {0}. There are {1} days remaining in the year.", dayOfYear, daysRemaining);
            //}
        }

        public override bool IsContentValid() {
            // Do validation of contentText and/or NSExtensionContext attachments here
            return true;
        }

        public override void DidSelectPost() {
            //This is called after the user selects Post. Do the upload of contentText and/ or NSExtensionContext attachments.

            // share data (read)
            //NSUserDefaults shared = new NSUserDefaults("group.com.thomaskefauver.MonkeyPaste", NSUserDefaultsType.SuiteName);
            //var value = shared.ValueForKey(new NSString("Token"));

            //var mvm = MpResolver.Resolve<MonkeyPaste.ViewModels.MpMainViewModel>();
            //mvm.AddSharedText(ContentText);

            OnShareText?.Invoke(this, ContentText + " " + ExtensionContext.InputItems[0].ToString());
            // Text = ContentText + " " + ExtensionContext.InputItems[0].ToString();
            return;
            // trigger data
            var item = ExtensionContext.InputItems[0];
            NSItemProvider prov = null;
            if (item != null) {
                prov = item.Attachments[0];
            }
            if (prov != null) {
                prov.LoadItem(UTType.Text, null, (NSObject text, NSError error) => {
                    if (text == null) {
                        return;
                    }
                    NSString newText = (NSString)text;

                    //InvokeOnMainThread(() => { LinkLabel.Text = newUrl.ToString(); });

                    // share data (write)
                    NSUserDefaults shared = new NSUserDefaults("group.com.thomaskefauver.MonkeyPaste", NSUserDefaultsType.SuiteName);
                    if (shared.ValueForKey(new NSString("Token")) != null) {
                        shared.RemoveObject("Token");
                    } else {
                        shared.SetValueForKey(new NSString(newText), new NSString("Token"));
                    }
                    shared.Synchronize();

                    //SUserDefaults.
                });
            }

            //This is called after the user selects Post. Do the upload of contentText and/ or NSExtensionContext attachments.
            var alert = UIAlertController.Create("Share extension", $"This is the step where you should post the ContentText value: '{ContentText}' to your targeted service.", UIAlertControllerStyle.Alert);
                PresentViewController(alert, true, () => {
                    DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, 5000000000), () => {
                        // Inform the host that we're done, so it un-blocks its UI. Note: Alternatively you could call super's -didSelectPost, which will similarly complete the extension context.
                        ExtensionContext.CompleteRequest(new NSExtensionItem[0], null);
                    });
                });

            //Inform the host that we're done, so it un-blocks its UI. Note: Alternatively you could call super's - didSelectPost, which will similarly complete the extension context.
            Console.WriteLine("Shared Text: " + ContentText);
            ExtensionContext.CompleteRequest(new NSExtensionItem[0], null);
        }

        public override SLComposeSheetConfigurationItem[] GetConfigurationItems() {
            // To add configuration options via table cells at the bottom of the sheet, return an array of SLComposeSheetConfigurationItem here.
            return new SLComposeSheetConfigurationItem[0];
        }
    }
}
