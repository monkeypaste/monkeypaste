
using MonkeyPaste.Common;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvWelcomeOptionItemViewModel :
        MpViewModelBase<MpAvWelcomeNotificationViewModel> {
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

        public ICommand ToggleOptionCommand => new MpCommand(
            () => {
                IsChecked = !IsChecked;
            });
    }
}
