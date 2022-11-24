using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using PropertyChanged;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvTagView : MpAvUserControl<MpAvTagTileViewModel> {
        #region Private Variables

        #endregion

        public MpAvTagView() {
            InitializeComponent();
            this.AttachedToVisualTree += MpAvTagView_AttachedToVisualTree;

            //var tagNameBorder = this.FindControl<MpAvClipBorder>("TagNameBorder");
            //tagNameBorder.PointerPressed += TagNameBorder_PointerPressed;
        }

        private void MpAvTagView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var drop_control = this;
            DragDrop.SetAllowDrop(drop_control, true);
            drop_control.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            drop_control.AddHandler(DragDrop.DragOverEvent, DragOver);
            drop_control.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            drop_control.AddHandler(DragDrop.DropEvent, Drop);
        }

        private void TagNameTextBox_KeyDown(object sender, global::Avalonia.Input.KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                e.Handled = true;
                BindingContext.FinishRenameTagCommand.Execute(null);

            } else if (e.Key == Key.Escape) {
                e.Handled = true;
                BindingContext.CancelRenameTagCommand.Execute(null);
            }
        }


        private void TagNameBorder_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if(e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed) {
                return;
            }
            if(e.ClickCount > 1) {
                BindingContext.RenameTagCommand.Execute(null);
            } else if (BindingContext.IsSelected) {
                //MpAvQueryInfoViewModel.Current.NotifyQueryChanged();
            }
            //MpAvDragDropManager.StartDragCheck(BindingContext);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }


        #region Drop

        #region Drop Events

        private void DragEnter(object sender, DragEventArgs e) {
            MpConsole.WriteLine("[DragEnter] TagTile: " + BindingContext);
             BindingContext.IsDragOverTag  = true;
        }

        private async void DragOver(object sender, DragEventArgs e) {
            MpConsole.WriteLine("[DragOver] TagTile: " + BindingContext);
            // e.DragEffects = DragDropEffects.Default;
             
            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            BindingContext.IsDragOverTagValid = await IsDropValidAsync(e.Data, is_copy);
            MpConsole.WriteLine("[DragOver] TagTile: " + BindingContext + " IsCopy: " + is_copy + " IsValid: " + BindingContext.IsDragOverTagValid);

            if (BindingContext.IsDragOverTagValid) {
                e.DragEffects = is_copy ? DragDropEffects.Copy : DragDropEffects.Move;
            } else {

                e.DragEffects = DragDropEffects.None;
            }
        }
        private void DragLeave(object sender, RoutedEventArgs e) {
            MpConsole.WriteLine("[DragLeave] TagTile: " + BindingContext);
            ResetDrop();
        }

        private async void Drop(object sender, DragEventArgs e) {
            // NOTE only pin tray allows drop not clip tray

            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            BindingContext.IsDragOverTagValid = await IsDropValidAsync(e.Data, is_copy);
            MpConsole.WriteLine("[Drop] TagTile: " + BindingContext + " IsCopy: " + is_copy + " IsValid: " + BindingContext.IsDragOverTagValid);

            if (BindingContext.IsDragOverTagValid) {
                e.DragEffects = is_copy ? DragDropEffects.Copy : DragDropEffects.Move;
                bool is_internal = e.Data.ContainsInternalContentItem();
                if (is_internal) {
                    // Internal Drop
                    await PerformTileDropAsync(e.Data, is_copy);
                } else {
                    // External Drop
                    await PerformExternalOrPartialDropAsync(e.Data);
                }
            }

            ResetDrop();
        }

        #endregion

        #region Drop Helpers

        private async Task<bool> IsDropValidAsync(IDataObject avdo, bool is_copy) {
            // called in DropExtension DragOver 

            MpConsole.WriteLine($"DragOverTag: " + BindingContext + " IsCopy: "+is_copy);

            bool is_internal =  avdo.ContainsInternalContentItem();
            if(!is_copy && is_internal) {
                // invalidate tile drag if tag is already linked to copy item and its not a copy operation
                string drop_ctvm_pub_handle = avdo.Get(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT) as string;
                var ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == drop_ctvm_pub_handle);
                if(ctvm != null) {
                    bool is_already_linked = await BindingContext.IsCopyItemLinkedAsync(ctvm.CopyItemId);
                    if(is_already_linked) {
                        return false;
                    }
                }
            }
            return true;
        }

        private void ResetDrop() {
            BindingContext.IsDragOverTag = false;
            BindingContext.IsDragOverTagValid = false;
        }

        private async Task PerformTileDropAsync(IDataObject avdo, bool isCopy) {
            string drop_ctvm_pub_handle = avdo.Get(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT) as string;
            var drop_ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == drop_ctvm_pub_handle);
            if (drop_ctvm == null) {
                Debugger.Break();
            }
            MpCopyItem drop_ci;

            if (isCopy) {
                //  duplicate
                drop_ci = (MpCopyItem)await drop_ctvm.CopyItem.Clone(true);
                await drop_ci.WriteToDatabaseAsync();
            } else {
                // move
                drop_ci = drop_ctvm.CopyItem;
            }
            if(drop_ci == null)  {
                return;
            }

            BindingContext.LinkCopyItemCommand.Execute(drop_ci.Id);
        }

        private async Task PerformExternalOrPartialDropAsync(IDataObject avdo) {
            MpPortableDataObject mpdo = await MpPlatformWrapper.Services.DataObjectHelperAsync.ReadDragDropDataObject(avdo) as MpPortableDataObject ;

            int drag_ciid = -1;
            string drag_ctvm_pub_handle = mpdo.GetData(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT) as string;
            if (!string.IsNullOrEmpty(drag_ctvm_pub_handle)) {
                var drag_ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == drag_ctvm_pub_handle);
                if(drag_ctvm != null) {
                    // tile sub-selection drop
                    drag_ciid = drag_ctvm.CopyItemId;
                }
            }

            MpCopyItem drop_ci = await MpPlatformWrapper.Services.CopyItemBuilder.CreateAsync(mpdo, drag_ciid);

            if (drop_ci == null) {
                return;
            }

            BindingContext.LinkCopyItemCommand.Execute(drop_ci.Id);

            // wait for all tags to update before notifiying clip tray
            while(MpAvTagTrayViewModel.Instance.IsAnyBusy) { await Task.Delay(100); }

            //push new item onto new item list so it shows in pin regardless if tile is selected
            // NOTE avoiding altering query tray without clicking the thing as consistent behavior.
            // (scrolloffset or sort may not make it visible)          

            MpAvClipTrayViewModel.Instance.AddNewItemsCommand.Execute(drop_ci);
        }

        #endregion

        

        #endregion
    }
}
