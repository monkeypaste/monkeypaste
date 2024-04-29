using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAnalyticToolbarTreeView.xaml
    /// </summary>
    public partial class MpAvSearchCriteriaItemView :
        MpAvUserControl<MpAvSearchCriteriaItemViewModel> {
        public MpAvSearchCriteriaItemView() {
            InitializeComponent();
            var db = this.FindControl<Control>("CriteriaDragButton");
            db.AddHandler(PointerPressedEvent, Db_PointerPressed, RoutingStrategies.Tunnel);
        }

        private async void Db_PointerPressed(object sender, PointerPressedEventArgs e) {
            var dragButton = sender as Control;
            if (dragButton == null) {
                return;
            }

            // HACK since preview8 drag source datacontext becomes null after dnd so storing to finsih up
            MpAvSearchCriteriaItemViewModel dc = BindingContext;
            BindingContext.IsDragging = true;
            MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;

            var mpdo = new MpAvDataObject(MpPortableDataFormats.INTERNAL_SEARCH_CRITERIA_ITEM_FORMAT, BindingContext);
            var result = await MpAvDoDragDropWrapper.DoDragDropAsync(dragButton, e, mpdo, /*DragDropEffects.Move | */DragDropEffects.Copy);

            if (BindingContext == null) {
                dc.IsDragging = false;
            } else {
                BindingContext.IsDragging = false;
            }
            MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;
            MpConsole.WriteLine($"SearchCriteria Drop Result: '{result}'");
        }
    }
}
