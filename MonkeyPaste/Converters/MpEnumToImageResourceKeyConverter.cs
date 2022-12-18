using System;
using System.Globalization;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpEnumToImageResourceKeyConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is Enum valueEnum) {
                if(valueEnum is MpTriggerType tt) {
                    switch(tt) {
                        case MpTriggerType.ContentAdded:
                            return "ClipboardImage";
                        case MpTriggerType.ContentTagged:
                            return "PinToCollectionImage";
                        case MpTriggerType.FileSystemChange:
                            return "FolderEventImage";
                        case MpTriggerType.Shortcut:
                            return "HotkeyImage"; 
                        case MpTriggerType.ParentOutput:
                            return "ChainImage";
                    }
                } else if(valueEnum is MpActionType at) {
                    switch(at) {
                        case MpActionType.Analyze:
                            return "BrainImage";                            
                        case MpActionType.Classify:
                            return "PinToCollectionImage";                            
                        case MpActionType.Compare:
                            return "ScalesImage";                            
                        case MpActionType.Macro:
                            return "HotkeyImage";                            
                        case MpActionType.Timer:
                            return "AlarmClockImage";                            
                        case MpActionType.FileWriter:
                            return "WandImage";                            
                    }
                } else if (valueEnum is MpContentTableContextActionType ctcat) {
                    switch (ctcat) {
                        default:
                            return "ScalesImage";
                    }
                }
            }

            return "QuestionMarkImage";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
