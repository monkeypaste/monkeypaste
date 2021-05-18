using System;
using AppKit;
using Foundation;
using Xamarin.Essentials;
using MonkeyPaste;

namespace MonkeyPaste.Mac
{
    public partial class ViewController : NSViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var timer = new System.Timers.Timer();
            timer.Elapsed += async (s,e) => {
                if(Clipboard.HasText)
                {
                    string cbText = await Clipboard.GetTextAsync();
                    Console.WriteLine("Mac clipboard: " + cbText);
                }
            };
             // Do any additional setup after loading the view.
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }
    }
}
