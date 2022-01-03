using MonkeyPaste;

namespace MpWpfApp {
    public interface MpISourceItemViewModel : MpISourceItem, MpIMouseEnabledViewModel {
        MpIconViewModel IconViewModel { get; set; }
    }
}
