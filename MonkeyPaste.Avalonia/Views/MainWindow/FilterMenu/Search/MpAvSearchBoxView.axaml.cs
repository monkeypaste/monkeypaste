using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSearchBoxView : MpAvUserControl<MpAvSearchBoxViewModel> {
        private ContextMenu _searchByContextMenu;

        public MpAvSearchBoxView() {
            InitializeComponent();

            var sb = this.FindControl<AutoCompleteBox>("SearchBox");
            sb.AttachedToVisualTree += Sb_AttachedToVisualTree;
        }

        #region Drop
        private object _dropLock = System.Guid.NewGuid().ToString();

        private void Sb_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var sb = sender as AutoCompleteBox;
            
            sb.AddHandler(DragDrop.DragOverEvent, DragOver);
            sb.AddHandler(DragDrop.DropEvent, Drop);
        }
        private void DragEnter(object sender, DragEventArgs e) {
            //MpConsole.WriteLine("[DragEnter] CurDropEffects: " + _curDropEffects);
            //SendDropMsg(e.Data, "dragenter");
            e.DragEffects = DragDropEffects.Copy | DragDropEffects.Move;
            //base.OnDragEnter(e);
        }

        private async void DragOver(object sender, DragEventArgs e) {
            //e.DragEffects = DragDropEffects.None;
            var formats = await e.Data.GetDataFormats_safe(_dropLock);
            if(!formats.Contains(MpPortableDataFormats.Text)) {
                e.DragEffects = DragDropEffects.None;
            }
        }
        private async void Drop(object sender, DragEventArgs e) {
            var formats = await e.Data.GetDataFormats_safe(_dropLock);
            if (!formats.Contains(MpPortableDataFormats.Text)) {
                e.DragEffects = DragDropEffects.None;
                return;
            }
            BindingContext.SearchText = e.Data.Get(MpPortableDataFormats.Text) as string;
        }

        #endregion
        private void SearchViewContainer_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            //InitContextMenu();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void InitContextMenu() {
            _searchByContextMenu = new ContextMenu() {
                Items = BindingContext.Filters.Select(x => CreateSearchByMenuItem(x))
            };
            _searchByContextMenu.ContextMenuOpening += (s, e) => {
                MpAvMainWindowViewModel.Instance.IsShowingDialog = true;
            };
            _searchByContextMenu.ContextMenuClosing += (s, e) => {
                MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
            };

            _searchByContextMenu.PointerReleased += (s, e) => {
                var searchDropDownButton = this.FindControl<Button>("SearchDropDownButton");
                searchDropDownButton.ContextMenu.Close();
            }; 
        }

        private void SearchBox_KeyUp(object sender, global::Avalonia.Input.KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                BindingContext.PerformSearchCommand.Execute(null);
            }
        }

        private void SearchDropDownButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if(sender is Button SearchDropDownButton) {
                SearchDropDownButton.ContextMenu = _searchByContextMenu;
                SearchDropDownButton.ContextMenu.PlacementTarget = SearchDropDownButton;
                SearchDropDownButton.ContextMenu.PlacementAnchor = PopupAnchor.Top;

                SearchDropDownButton.ContextMenu.Open();
            }
        }

        private object CreateSearchByMenuItem(MpAvSearchFilterViewModel sfvm) {
            var menuItem = new MenuItem();
            if (sfvm.IsSeperator) {
                return new Separator();
            } else {
                var cb = new CheckBox();
                cb.Bind(
                    CheckBox.IsCheckedProperty,
                    new Binding() {
                        Source = sfvm,
                        Path = nameof(sfvm.IsChecked),
                        Mode = BindingMode.TwoWay
                    });
                cb.Bind(
                    CheckBox.IsEnabledProperty,
                    new Binding() {
                        Source = sfvm,
                        Path = nameof(sfvm.IsEnabled)
                    });

                var l = new Label();

                l.Bind(
                    Label.ContentProperty,
                    new Binding() {
                        Source = sfvm,
                        Path = nameof(sfvm.Label)
                    });

                menuItem.Icon = cb;
                menuItem.Header = l;
            }

            return menuItem;
        }
    }
}
