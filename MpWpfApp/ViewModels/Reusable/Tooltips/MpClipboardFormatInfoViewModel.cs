using MonkeyPaste;

namespace MpWpfApp {
    public class MpClipboardFormatInfoViewModel : MpViewModelBase, MpITooltipInfoViewModel {
        public object Tooltip => "Be aware that format settings apply to only that specific format. When data is acquired and presented the highest resolution is used. So for example, if Html and Text are available on the clipboard the html format will take precedence. As it stands for text Rtf has the highest precedance, then html, csv, and plain text in that order.";
    }
}
