using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAnalyticToolbarTreeView.xaml
    /// </summary>
    public partial class MpAvSearchCriteriaItemView :
        MpAvUserControl<MpAvSearchCriteriaItemViewModel> {
        public MpAvSearchCriteriaItemView() {
            AvaloniaXamlLoader.Load(this);
            var db = this.FindControl<Control>("CriteriaDragButton");
            db.AddHandler(PointerPressedEvent, Db_PointerPressed, RoutingStrategies.Tunnel);
        }

        private async void Db_PointerPressed(object sender, PointerPressedEventArgs e) {
            var dragButton = sender as Control;
            if (dragButton == null) {
                return;
            }
            MpAvSearchCriteriaItemViewModel dc = BindingContext;
            BindingContext.IsDragging = true;
            MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;

            var mpdo = new MpAvDataObject(MpPortableDataFormats.INTERNAL_SEARCH_CRITERIA_ITEM_FORMAT, BindingContext);
            var result = await DragDrop.DoDragDrop(e, mpdo, DragDropEffects.Move | DragDropEffects.Copy);

            if (BindingContext == null) {
                // is null after drop as of preview 8
                dc.IsDragging = false;
            } else {
                BindingContext.IsDragging = false;
            }
            MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;
            MpConsole.WriteLine($"SearchCriteria Drop Result: '{result}'");
        }
    }
}
