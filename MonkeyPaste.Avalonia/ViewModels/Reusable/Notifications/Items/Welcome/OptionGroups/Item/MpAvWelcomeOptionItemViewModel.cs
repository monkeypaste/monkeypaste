
using Avalonia.Controls;
using MonkeyPaste.Common;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvWelcomeOptionItemViewModel :
        MpAvViewModelBase<MpAvWelcomeNotificationViewModel> {
        public bool IsHovering { get; set; }
        public bool IsChecked { get; set; }

        public object OptionId { get; set; }
        public object IconSourceObj { get; set; }

        public string LabelText { get; set; }
        public string DescriptionText { get; set; }
        public MpAvWelcomeOptionItemViewModel() : base(null) { }
        public MpAvWelcomeOptionItemViewModel(MpAvWelcomeNotificationViewModel parent, object optId) : base(parent) {
            PropertyChanged += MpAvGestureProfileItemViewModel_PropertyChanged;
            OptionId = optId;
        }

        private void MpAvGestureProfileItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsHovering):
                    if (Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.HoverItem));
                    }
                    break;
            }
        }

        public ICommand ToggleOptionCommand => new MpCommand<object>(
            (args) => {
                IsChecked = !IsChecked;
            },
            (args) => {
                if (IsChecked) {
                    return false;
                }
                if (!Parent.IS_PASSIVE_GESTURE_TOGGLE_ENABLED) {
                    return true;
                }
                if (Parent != null &&
                    OptionId is int optVal &&
                    args is Control &&
                    (Parent.CurPageType == MpWelcomePageType.ScrollWheel ||
                    Parent.CurPageType == MpWelcomePageType.DragToOpen)) {
                    if (optVal == 0 && IsChecked) {
                        return false;
                    }
                    if (optVal == 1 && IsChecked) {
                        return false;
                    }
                    // these options are enabled passively so if cmd arg is the view
                    // its coming from ui which should be ignored
                    //return false;
                }
                return true;
            });
    }
}
