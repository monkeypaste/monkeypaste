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
            db.PointerPressed += Db_PointerPressed;
        }

        private async void Db_PointerPressed(object sender, PointerPressedEventArgs e) {
            var dragButton = sender as Control;
            if (dragButton == null) {
                return;
            }
            var mpdo = new MpAvDataObject(MpPortableDataFormats.INTERNAL_SEARCH_CRITERIA_ITEM_FORMAT, BindingContext);
            var result = await DragDrop.DoDragDrop(e, mpdo, DragDropEffects.Move | DragDropEffects.Copy);

            MpConsole.WriteLine($"SearchCriteria Drop Result: '{result}'");
        }
    }
}
