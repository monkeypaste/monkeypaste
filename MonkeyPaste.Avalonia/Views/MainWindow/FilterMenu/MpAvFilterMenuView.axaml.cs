using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvFilterMenuView : MpAvUserControl<MpAvFilterMenuViewModel> {

        public MpAvFilterMenuView() {
            AvaloniaXamlLoader.Load(this);

            var sort_view = this.FindControl<MpAvClipTileSortView>("SortView");
            sort_view.GetObservable(InputElement.IsKeyboardFocusWithinProperty).Subscribe(value => OnFilterControlFocusWithinChanged(sort_view));
            var search_view = this.FindControl<MpAvSearchBoxView>("SearchBoxView");
            search_view.GetObservable(InputElement.IsKeyboardFocusWithinProperty).Subscribe(value => OnFilterControlFocusWithinChanged(search_view));

        }

        private void OnFilterControlFocusWithinChanged(Control filterControl) {
            bool has_focus = filterControl.IsKeyboardFocusWithin;
            if (has_focus) {
                if (filterControl is MpAvSearchBoxView sbv &&
                    sbv.DataContext is MpAvSearchBoxViewModel sbvm) {

                }
            } else {
                if (filterControl is MpAvSearchBoxView sbv &&
                    sbv.DataContext is MpAvSearchBoxViewModel sbvm) {

                }
            }
        }
    }
}
