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

        public bool IsHitTestable {
            get {
                if (Parent != null &&
                   (Parent.CurPageType == MpWelcomePageType.ScrollWheel ||
                    Parent.CurPageType == MpWelcomePageType.DragToOpen) &&
                    OptionId is int optVal && optVal == 1 &&
                    !IsChecked) {
                    // disable passive enabling
                    return false;
                }
                if (Parent != null &&
                    Parent.CurOptGroupViewModel.Items.Count > 1 &&
                    IsChecked) {
                    // disable untoggling a multi radio 
                    return false;
                }
                return true;
            }
        }
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
                    OnPropertyChanged(nameof(IsHitTestable));
                    break;
            }
        }

        public ICommand ToggleOptionCommand => new MpCommand<object>(
            (args) => {
                IsChecked = !IsChecked;
            });
    }
}
