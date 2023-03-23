
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpActionDesignerItemView.xaml
    /// </summary>
    public partial class MpAvActionDesignerItemView : MpAvUserControl<MpAvActionViewModelBase> {
        public MpAvActionDesignerItemView() {
            AvaloniaXamlLoader.Load(this);

            var dicc = this.FindControl<ContentControl>("DesignerItemContentControl");
            dicc.AddHandler(ContentControl.KeyDownEvent, Dicc_KeyDown, RoutingStrategies.Tunnel);
            InitDnd();
        }
        private void Dicc_KeyDown(object sender, global::Avalonia.Input.KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                if (BindingContext.IsSelected) {
                    e.Handled = true;
                    BindingContext.DeleteThisActionCommand.Execute(null);
                }
            }
        }

        #region Drop

        private void InitDnd() {
            var drop_control = this;
            DragDrop.SetAllowDrop(drop_control, true);
            drop_control.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            drop_control.AddHandler(DragDrop.DragOverEvent, DragOver);
            drop_control.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            drop_control.AddHandler(DragDrop.DropEvent, Drop);
        }
        #region Drop Events

        private void DragEnter(object sender, DragEventArgs e) {
            BindingContext.IsDragOver = true;

        }
        private void DragLeave(object sender, RoutedEventArgs e) {
            ResetDrop();
        }

        private void DragOver(object sender, DragEventArgs e) {
            bool is_valid = IsDropValid(e.Data);
            e.DragEffects =
                is_valid ?
                    DragDropEffects.Move : DragDropEffects.None;
            MpConsole.WriteLine($"[DragOver] TagTile: '{e.DragEffects}'");

        }

        private async void Drop(object sender, DragEventArgs e) {
            bool is_valid = IsDropValid(e.Data);

            e.DragEffects =
                is_valid ?
                    DragDropEffects.Move : DragDropEffects.None;

            if (e.DragEffects == DragDropEffects.None) {
                ResetDrop();
                return;
            }
            var test = e.Data.GetAllDataFormats();

            List<MpCopyItem> drop_cil = new List<MpCopyItem>();
            if (e.Data.ContainsFullContentItem()) {
                string drop_ctvm_pub_handle = e.Data.Get(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT) as string;
                var drop_ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == drop_ctvm_pub_handle);
                if (drop_ctvm != null) {
                    drop_cil.Add(drop_ctvm.CopyItem);
                }
            } else if (e.Data.Contains(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT) &&
                        e.Data.Get(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT) is MpAvTagTileViewModel ttvm) {
                drop_cil = await MpDataModelProvider.GetAllCopyItemsForTagAndAllDescendantsAsync(ttvm.TagId);
            } else {
                var ext_ci = await e.Data.ToCopyItemAsync();
                if (ext_ci != null) {
                    drop_cil.Add(ext_ci);
                }
            }

            if (!drop_cil.Any()) {
                e.DragEffects = DragDropEffects.None;
                ResetDrop();
                return;
            }

            foreach (var ci in drop_cil) {
                BindingContext.InvokeThisActionCommand.Execute(ci);
                while (BindingContext.IsSelfOrAnyDescendantPerformingAction) {
                    await Task.Delay(100);
                }
            }

            ResetDrop();
        }

        #endregion

        #region Drop Helpers

        private bool IsDropValid(IDataObject avdo) {
            if (BindingContext == null ||
                !BindingContext.RootTriggerActionViewModel.IsEnabled.IsTrue()) {
                return false;
            }
            return true;
        }

        private void ResetDrop() {
            BindingContext.IsDragOver = false;

        }

        #endregion

        #endregion
    }
}
