using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Annotations.Storage;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpContentItemView.xaml
    /// </summary>
    public partial class MpContentItemView : MpUserControl<MpContentItemViewModel> {
        //private static MpContentContextMenuView _contentContextMenu;

        public MpContentItemView() : base() {
            InitializeComponent();            
        }

        private void ContentListItemView_Loaded(object sender, RoutedEventArgs e) {
            //if (_contentContextMenu == null) {
            //    _contentContextMenu = new MpContentContextMenuView();
            //    //_contentContextMenu.Items.Refresh();
            //}
        }

        

        private void ContentListItemView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != null && e.OldValue is MpContentItemViewModel ocivm) {
                ocivm.OnUiUpdateRequest -= Civm_OnUiUpdateRequest;
            }
            if (e.NewValue != null && e.NewValue is MpContentItemViewModel ncivm) {
                if (!ncivm.IsPlaceholder) {
                    ncivm.OnUiUpdateRequest += Civm_OnUiUpdateRequest;
                }
            }
        }

        #region Event Handlers

        #region View Model Ui Requests

        private void Civm_OnUiUpdateRequest(object sender, EventArgs e) {
            this.UpdateLayout();            
        }


        #endregion

        #endregion

        private void Border_Unloaded(object sender, RoutedEventArgs e) {
            if(BindingContext != null) {
                BindingContext.OnUiUpdateRequest -= Civm_OnUiUpdateRequest;
            }
        }

        private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (BindingContext.IsEditingTitle ||
                !BindingContext.IsContentReadOnly ||
                 BindingContext.Parent.Parent.IsAnyResizing ||
                 BindingContext.Parent.Parent.CanAnyResize ||
                 MpResizeBehavior.IsAnyResizing) {
                e.Handled = false;
                return;
            }
            BindingContext.IsSelected = true;

            MpDragDropManager.StartDragCheck(
                e.GetPosition(Application.Current.MainWindow));

            e.Handled = true;
        }

        private void Border_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            //if (MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
            //    return;
            //}

            if (!BindingContext.IsSelected) {
                BindingContext.IsSelected = true;
            }

            e.Handled = true;
            var fe = sender as FrameworkElement;

            MpContextMenuView.Instance.DataContext = MpClipTrayViewModel.Instance.MenuItemViewModel;
            MpContextMenuView.Instance.PlacementTarget = this;
            MpContextMenuView.Instance.IsOpen = true;
        }

        private MemoryStream annontationStream = null;
        private AnnotationService service;

        private void FlowDocumentScrollViewer_Loaded(object sender, RoutedEventArgs e) {
            var sdsv = sender as FlowDocumentScrollViewer;
            service = new AnnotationService(sdsv);

            annontationStream = new MemoryStream();
            AnnotationStore store = new XmlStreamStore(annontationStream);

            service.Enable(store);
        }
    }
}
