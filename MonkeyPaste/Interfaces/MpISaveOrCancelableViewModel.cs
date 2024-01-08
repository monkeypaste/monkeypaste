using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpISaveOrCancelableViewModel : MpIViewModel {
        ICommand SaveCommand { get; }
        ICommand CancelCommand { get; }

        bool IsSaveCancelEnabled { get; }

        bool CanSaveOrCancel { get; }
    }
    //public interface MpIPluginComponentViewModel : MpIViewModel {
    //    public MpPresetParamaterHost ComponentFormat { get; }
    //}

}
