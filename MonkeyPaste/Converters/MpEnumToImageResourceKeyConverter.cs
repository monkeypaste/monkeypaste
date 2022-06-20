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
                            return "ClipboardIcon";
                        case MpTriggerType.ContentTagged:
                            return "PinToCollectionIcon";
                        case MpTriggerType.FileSystemChange:
                            return "FolderEventIcon";
                        case MpTriggerType.Shortcut:
                            return "HotkeyIcon"; 
                        case MpTriggerType.ParentOutput:
                            return "ChainIcon";
                    }
                } else if(valueEnum is MpActionType at) {
                    switch(at) {
                        case MpActionType.Analyze:
                            return "BrainIcon";                            
                        case MpActionType.Classify:
                            return "PinToCollectionIcon";                            
                        case MpActionType.Compare:
                            return "ScalesIcon";                            
                        case MpActionType.Macro:
                            return "HotkeyIcon";                            
                        case MpActionType.Timer:
                            return "AlarmClockIcon";                            
                        case MpActionType.FileWriter:
                            return "WandIcon";                            
                    }
                }
            }

            return "QuestionMarkIcon";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
