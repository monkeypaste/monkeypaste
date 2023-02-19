using System;
using System.Collections.ObjectModel;

namespace MonkeyPaste {
    public interface MpISelectableViewModel : MpIViewModel {
        bool IsSelected { get; set; }
        DateTime LastSelectedDateTime { get; set; }
    }

    public interface MpIConditionalSelectableViewModel : MpISelectableViewModel {
        bool CanSelect { get; }
    }

    public interface MpISelectorItemViewModel : MpIViewModel {
        MpISelectorViewModel Selector { get; }
    }
    public interface MpISelectorItemViewModel<TItemType> : MpISelectorItemViewModel where TItemType : MpViewModelBase {
        new MpISelectorViewModel<TItemType> Selector { get; }
    }

    public interface MpISelectorViewModel : MpIViewModel {
        object SelectedItem { get; set; }
    }

    public interface MpISelectorViewModel<T> : MpISelectorViewModel where T : MpViewModelBase {
        new T SelectedItem { get; set; }
        ObservableCollection<T> Items { get; set; }
    }
    //public interface MpIPluginComponentViewModel : MpIViewModel {
    //    public MpParameterHostBaseFormat ComponentFormat { get; }
    //}

}
