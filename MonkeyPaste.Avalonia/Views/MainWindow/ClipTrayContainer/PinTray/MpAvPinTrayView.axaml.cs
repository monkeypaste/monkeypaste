using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPinTrayView : MpAvUserControl<MpAvClipTrayViewModel> {
        #region Private Variables

        private MpAvDropHostAdorner _dropAdorner;

        #endregion

        #region Statics

        public static MpAvPinTrayView Instance { get; private set; }

        #endregion

        #region Drop

        #region Drop Events

        private void DragEnter(object sender, DragEventArgs e) {
            //MpConsole.WriteLine("[DragEnter] PinTrayListBox: ");
            BindingContext.IsDragOverPinTray = true;
        }

        private void DragOver(object sender, DragEventArgs e) {
            //MpConsole.WriteLine("[DragOver] PinTrayListBox: ");
            var ptr_lb = this.FindControl<ListBox>("PinTrayListBox");
            var ptr_mp = e.GetPosition(ptr_lb).ToPortablePoint();
            int drop_idx = ptr_lb.GetDropIdx(ptr_mp, Orientation.Horizontal);

            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool is_drop_valid = IsDropValid(e.Data, drop_idx, is_copy);
            //MpConsole.WriteLine("[DragOver] PinTrayListBox DropIdx: " + drop_idx + " IsCopy: " + is_copy + " IsValid: " + is_drop_valid);
            e.DragEffects = DragDropEffects.None;
            if (is_drop_valid) {
                e.DragEffects = is_copy ? DragDropEffects.Copy : DragDropEffects.Move;
                MpLine dropLine = CreateDropLine(drop_idx, is_copy);
                DrawAdorner(dropLine);
            } else {
                ClearAdorner();
            }
        }
        private void DragLeave(object sender, DragEventArgs e) {
            // MpConsole.WriteLine("[DragLeave] PinTrayListBox: ");
            ResetDrop();
        }

        private async void Drop(object sender, DragEventArgs e) {
            // NOTE only pin tray allows drop not clip tray

            var ptr_lb = this.FindControl<ListBox>("PinTrayListBox");
            var ptr_mp = e.GetPosition(ptr_lb).ToPortablePoint();
            int drop_idx = ptr_lb.GetDropIdx(ptr_mp, Orientation.Horizontal);

            bool is_copy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool is_drop_valid = IsDropValid(e.Data, drop_idx, is_copy);
            // MpConsole.WriteLine("[Drop] PinTrayListBox DropIdx: " + drop_idx + " IsCopy: " + is_copy + " IsValid: " + is_drop_valid);

            e.DragEffects = DragDropEffects.None;
            if (is_drop_valid) {
                BindingContext.IsPinTrayBusy = true;

                e.DragEffects = is_copy ? DragDropEffects.Copy : DragDropEffects.Move;
                bool is_internal = e.Data.ContainsContentRef();
                if (is_internal) {
                    // Internal Drop
                    await PerformTileDropAsync(drop_idx, e.Data, is_copy);
                } else {
                    // External Drop
                    await PerformExternalOrPartialDropAsync(drop_idx, e.Data);
                }

                Dispatcher.UIThread.Post(async () => {
                    while (BindingContext.IsAnyBusy) {
                        await Task.Delay(100);
                    }
                    BindingContext.IsPinTrayBusy = false;
                });
            }

            ResetDrop();
        }

        #endregion

        #region Drop Helpers

        private bool IsDropValid(IDataObject avdo, int drop_idx, bool is_copy) {
            // called in DropExtension DragOver 

            //MpConsole.WriteLine("IsDropValid DropIdx: " + drop_idx);
            if (drop_idx < 0) {
                return false;
            }
            string drag_ctvm_pub_handle = avdo.Get(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT) as string;
            if (string.IsNullOrEmpty(drag_ctvm_pub_handle)) {
                // Tile drop is always valid
                return true;
            }

            var drag_pctvm = BindingContext.PinnedItems.FirstOrDefault(x => x.PublicHandle == drag_ctvm_pub_handle);
            int drag_pctvm_idx = BindingContext.PinnedItems.IndexOf(drag_pctvm);

            bool is_drop_onto_same_idx = drop_idx == drag_pctvm_idx || drop_idx == drag_pctvm_idx + 1;
            bool is_internal = avdo.ContainsContentRef();
            bool is_partial_drop = !is_internal;

            if (!is_copy && !is_partial_drop && is_drop_onto_same_idx) {
                // don't allow moving full item if it'll be at same position 
                return false;
            }
            // TODO Check data here (which shouldn't matter for internal but would be general app-level check from external)

            //avdo.GetDataFormats().ForEach(x => MpConsole.WriteLine($"FORMAT: '{x}' Data: '{avdo.Get(x)}'"));


            return true;
        }

        private void ResetDrop() {
            BindingContext.IsDragOverPinTray = false;
            ClearAdorner();
        }

        private async Task PerformTileDropAsync(int drop_idx, IDataObject avdo, bool isCopy) {
            string drop_ctvm_pub_handle = avdo.Get(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT) as string;
            var drop_ctvm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.PublicHandle == drop_ctvm_pub_handle);
            if (drop_ctvm == null) {
                Debugger.Break();
            }

            if (isCopy) {
                //  duplicate
                var dup_ci = await BindingContext.SelectedItem.CopyItem.CloneDbModelAsync(deepClone: true);

                await dup_ci.WriteToDatabaseAsync();


                drop_ctvm = await BindingContext.CreateClipTileViewModelAsync(dup_ci, -1);
            }
            BindingContext.PinTileCommand.Execute(new object[] { drop_ctvm, drop_idx });
            MpConsole.WriteLine($"Tile '{drop_ctvm}' dropped onto pintray idx: {drop_idx}");
        }

        private async Task PerformExternalOrPartialDropAsync(int drop_idx, IDataObject avdo) {
            MpPortableDataObject mpdo = await
                Mp.Services.DataObjectHelperAsync
                .ReadDragDropDataObjectAsync(avdo) as MpPortableDataObject;

            bool from_ext = !avdo.ContainsContentRef();

            var avdo_ci = await Mp.Services.CopyItemBuilder.BuildAsync(
                pdo: mpdo,
                force_ext_sources: from_ext,
                transType: MpTransactionType.Created);

            var drop_ctvm = await BindingContext.CreateClipTileViewModelAsync(avdo_ci, -1);
            BindingContext.PinTileCommand.Execute(new object[] { drop_ctvm, drop_idx });

            MpConsole.WriteLine($"PARTIAL Tile '{drop_ctvm}' dropped onto pintray idx: {drop_idx}");
        }

        #endregion

        #region Adorner

        private void ClearAdorner() {
            _dropAdorner.IsVisible = false;
            _dropAdorner.DrawDropAdorner(null);
        }

        private void DrawAdorner(MpLine dropLine) {
            _dropAdorner.IsVisible = true;
            if (dropLine == null) {
                return;
            }
            _dropAdorner.DrawDropAdorner(new MpShape[] { dropLine });
        }

        private MpLine CreateDropLine(int drop_idx, bool isCopy) {
            var ptr_lb = this.FindControl<ListBox>("PinTrayListBox");
            int total_count = BindingContext.PinnedItems.Count;
            if (drop_idx > total_count || total_count == 0) {
                return null;
            }
            MpLine drop_line;
            int ref_lbi_idx = Math.Clamp(drop_idx, 0, total_count - 1);
            var ref_lbi = ptr_lb.ContainerFromIndex(ref_lbi_idx) as ListBoxItem;
            var ref_lbi_rect = ref_lbi.Bounds.ToPortableRect();

            if (drop_idx < total_count) {
                drop_line = ref_lbi_rect.GetSideByLabel("l");
            } else {
                drop_line = ref_lbi_rect.GetSideByLabel("r");
            }
            // NOTE this cleans up the line...for some reason container bounds ignores lb padding 
            var ptr_lb_wp = ptr_lb.GetVisualDescendant<WrapPanel>();

            // lbi bounds need to be relative to items panel not lb, lb doesn't account for box model calc
            var items_panel_offset = ptr_lb_wp.TranslatePoint(new Point(0, 0), ptr_lb).Value.ToPortablePoint();
            drop_line.P1 += items_panel_offset;
            drop_line.P2 += items_panel_offset;

            // get child to adjust height
            var ctv = ref_lbi.GetVisualDescendant<MpAvClipTileView>();
            // NOTE get inner child because of border thickness
            var lbi_child = ctv.FindControl<Border>("ClipTileContainerBorder").Child;
            var lbi_child_tl = lbi_child.TranslatePoint(new Point(0, 0), ptr_lb_wp);
            drop_line.P1.Y = lbi_child_tl.Value.Y;

            var lbi_child_bl = lbi_child.TranslatePoint(new Point(0, lbi_child.Bounds.Height), ptr_lb_wp);
            drop_line.P2.Y = lbi_child_bl.Value.Y;

            drop_line.StrokeOctColor = isCopy ? MpSystemColors.limegreen : MpSystemColors.White;
            drop_line.StrokeThickness = 2;
            drop_line.StrokeDashStyle = new double[] { 5, 2 };

            return drop_line;
        }

        private void InitAdorner(Control adornedControl) {
            if (_dropAdorner != null) {
                // should only happen once
                Debugger.Break();
            }

            _dropAdorner = new MpAvDropHostAdorner(adornedControl);
            adornedControl
                .AddOrReplaceAdornerAsync(_dropAdorner)
                .FireAndForgetSafeAsync(BindingContext);

            MpConsole.WriteLine("Adorner added to control: " + adornedControl);
        }

        #endregion

        #endregion
        public MpAvPinTrayView() {
            if (Instance != null) {
                // ensure singleton
                Debugger.Break();
                return;
            }
            Instance = this;

            InitializeComponent();

            PinTrayListBox = this.FindControl<ListBox>("PinTrayListBox");
            PinTrayListBox.AttachedToVisualTree += PinTrayListBox_AttachedToVisualTree;
            PinTrayListBox.GotFocus += PinTrayListBox_GotFocus;
            this.EffectiveViewportChanged += MpAvPinTrayView_EffectiveViewportChanged;
            this.DataContextChanged += MpAvPinTrayView_DataContextChanged;
            if (DataContext != null) {
                MpAvPinTrayView_DataContextChanged(null, null);
            }


        }

        private void MpAvPinTrayView_EffectiveViewportChanged(object sender, EffectiveViewportChangedEventArgs e) {
            this.FindControl<Control>("PinTrayEmptyContainer")?.InvalidateAll();
        }

        private void MpAvPinTrayView_DataContextChanged(object sender, EventArgs e) {
            if (BindingContext == null) {
                return;
            }
            BindingContext.PinnedItems.CollectionChanged += PinnedItems_CollectionChanged;
        }

        private void PinnedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset) {
                OnItemRemoved();
            }
            if (e.Action == NotifyCollectionChangedAction.Add) {
                OnItemAdded();
            }
        }

        private void PinTrayListBox_GotFocus(object sender, GotFocusEventArgs e) {
            if (BindingContext.IsPinTrayEmpty) {
                return;
            }
            if (e.NavigationMethod == NavigationMethod.Tab) {
                if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) {
                    // shift tab from clip tray select last
                    BindingContext.SelectedItem = BindingContext.PinnedItems.Last();
                } else {
                    BindingContext.SelectedItem = BindingContext.PinnedItems.First();
                }
            }
        }

        private void OnItemRemoved() {
            var ctrcv = this.GetVisualAncestor<MpAvClipTrayContainerView>();
            if (BindingContext == null ||
                BindingContext.HasUserAlteredPinTrayWidthSinceWindowShow ||
                ctrcv == null) {
                // ignore collection changed if user in workflow
                //MpConsole.WriteLine($"PinTray dematerialized {BindingContext.Items.Count} items was ignored. HasUserAlteredPinTrayWidthSinceWindowShow: {BindingContext.HasUserAlteredPinTrayWidthSinceWindowShow} ");
                return;
            }
            double new_length = 0;
            if (!BindingContext.IsPinTrayEmpty) {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    new_length = BindingContext.PinnedItems.Max(x => x.TrayRect.Right);
                } else {
                    new_length = BindingContext.PinnedItems.Max(x => x.TrayRect.Bottom);
                }
            }
            ctrcv.UpdatePinTrayVarDimension(new GridLength(new_length, GridUnitType.Auto));
        }

        private void OnItemAdded() {
            var ctrcv = this.GetVisualAncestor<MpAvClipTrayContainerView>();
            if (BindingContext == null ||
                BindingContext.HasUserAlteredPinTrayWidthSinceWindowShow ||
                BindingContext.Items.Count == 0 ||
                ctrcv == null) {
                // ignore collection changed if user in workflow
                //MpConsole.WriteLine($"PinTray materialized {e.Containers.Count} items was ignored. HasUserAlteredPinTrayWidthSinceWindowShow: {BindingContext.HasUserAlteredPinTrayWidthSinceWindowShow} ");
                return;
            }
            MpSize added_size = new MpSize();
            var containers = BindingContext.PinnedItems.Select((x, idx) => PinTrayListBox.ContainerFromIndex(idx));
            added_size.Width = containers
                    .Where(x => x != null && x.DataContext != null && x.DataContext is MpAvClipTileViewModel)
                    .Select(x => x.DataContext)
                    .Cast<MpAvClipTileViewModel>()
                    .Sum(x => x.BoundWidth);
            added_size.Height = containers
                    .Where(x => x != null && x.DataContext != null && x.DataContext is MpAvClipTileViewModel)
                    .Select(x => x.DataContext)
                    .Cast<MpAvClipTileViewModel>()
                    .Sum(x => x.BoundHeight);

            var gs = ctrcv.FindControl<GridSplitter>("ClipTraySplitter");
            var gs_grid = ctrcv.FindControl<Grid>("ClipTrayContainerGrid");
            double new_length = 0;
            if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                new_length = Math.Min(BindingContext.MaxPinTrayScreenWidth + gs.Bounds.Width, Math.Max(this.Bounds.Width + gs.Bounds.Width, added_size.Width + 40.0d));
            } else {
                new_length = Math.Min(BindingContext.MaxPinTrayScreenHeight + gs.Bounds.Height, Math.Max(this.Bounds.Height + gs.Bounds.Height, added_size.Height));
            }
            ctrcv.UpdatePinTrayVarDimension(new GridLength(new_length, GridUnitType.Auto));
        }

        private void PinTrayListBox_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var ptrlb = sender as ListBox;
            DragDrop.SetAllowDrop(ptrlb, true);
            ptrlb.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            ptrlb.AddHandler(DragDrop.DragOverEvent, DragOver);
            ptrlb.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            ptrlb.AddHandler(DragDrop.DropEvent, Drop);
            InitAdorner(ptrlb);

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.PinTrayEmptyOrHasTile:
                    // NOTE these layout values need to match UpdateContentOrientation settings
                    var ptrlb = this.FindControl<ListBox>("PinTrayListBox");
                    if (MpAvClipTrayViewModel.Instance.IsAnyTilePinned) {
                        // to avoid interfering w/ user-defined layout
                    } else {
                        ptrlb.Padding = new Thickness();
                    }
                    if (MpAvClipTrayViewModel.Instance.ListOrientation == Orientation.Horizontal) {
                        ptrlb.Padding = new Thickness(10, 0, 10, 0);
                    } else {
                        ptrlb.Padding = new Thickness(10, 10, 10, 10);
                    }
                    break;
            }
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
