using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MonkeyPaste.Common;
using MonkeyPaste;
using MonkeyPaste.Common.Wpf;
using CefSharp.DevTools.CSS;
using Microsoft.Office.Interop.Outlook;
using System.Runtime.Serialization;
using Exception = System.Exception;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpPasteTemplateToolbarView.xaml
    /// </summary>
    public partial class MpPasteTemplateToolbarView : MpUserControl<MpTemplateCollectionViewModel> {
        RichTextBox _activeRtb;

        public MpPasteTemplateToolbarView() {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }
        public void SetActiveRtb(RichTextBox trtb) {
            if (_activeRtb == trtb) {
                return;
            }
            _activeRtb = trtb;
        }


        private void ClipTilePasteTemplateToolbar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(BindingContext == null || ((bool)e.NewValue) == false) {
                return;
            }
            if(BindingContext.Items.Count == 0) {
                return;
            }
            if(BindingContext.SelectedItem == null) {
                BindingContext.SelectedItem = BindingContext.Items[0];                
            }

            var rtb = this.GetVisualAncestor<MpClipTileView>()
                .GetVisualDescendent<MpRtbContentView>()
                .GetVisualDescendent<RichTextBox>();
            //rtb.Selection.Select(rtb.Document.ContentStart, rtb.Document.ContentStart);

            BindingContext.SelectedItem.IsPasteTextBoxFocused = true;
        }

        private void SelectedTemplateTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter) {
                if(BindingContext.IsAllTemplatesFilled) {
                    BindingContext.PasteTemplateCommand.Execute(null);
                } else {
                    BindingContext.SelectNextTemplateCommand.Execute(null);
                }
                e.Handled = true;
            }
        }

        private void ContactComboBox_Loaded(object sender, RoutedEventArgs e) {
            var contact_cmb = sender as ComboBox;
            var tvm = contact_cmb.DataContext as MpTextTemplateViewModelBase;
            MpHelpers.RunOnMainThread(async () => {
                tvm.IsBusy = true;

                contact_cmb.ItemsSource = await MpMasterTemplateModelCollectionViewModel.Instance.GetContacts();
                
                if(contact_cmb.Items.Count == 0) {
                    contact_cmb.Items.Add(MpContact.EmptyContact);
                }
                tvm.IsBusy = false;
            });
        }

        private void ContactComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var contact_cmb = sender as ComboBox;
            var tvm = contact_cmb.DataContext as MpTextTemplateViewModelBase;

            if (tvm == null) {
                return;
            }
            var selected_contact = contact_cmb.SelectedItem as MpContact;

            tvm.TemplateText = selected_contact.GetField(tvm.TemplateData) as string;
        }

        private void DateTimeTextBox_Loaded(object sender, RoutedEventArgs e) {
            var tb = sender as TextBox;
            var tvm = tb.DataContext as MpTextTemplateViewModelBase;
            if (tvm == null) {
                return;
            }
            string templateText;
            try {
                templateText = DateTime.Now.ToString(tvm.TemplateData);
             }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error converting date time format '{tvm.TemplateData}', using default", ex);
                templateText = DateTime.Now.ToString();
            }
            tvm.TemplateText = templateText;

            BindingContext.SelectNextTemplateCommand.Execute(null);
        }

        private void StaticTextBox_Loaded(object sender, RoutedEventArgs e) {
            var tb = sender as TextBox;
            var tvm = tb.DataContext as MpTextTemplateViewModelBase;
            if (tvm == null) {
                return;
            }
            tvm.TemplateText = tvm.TemplateData;

            BindingContext.SelectNextTemplateCommand.Execute(null);
        }

        private void StaticTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var tb = sender as TextBox;
            var tvm = tb.DataContext as MpTextTemplateViewModelBase;
            if (tvm == null) {
                return;
            }
            tvm.TemplateText = tvm.TemplateData;
        }

        private void DateTimeTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var tb = sender as TextBox;
            var tvm = tb.DataContext as MpTextTemplateViewModelBase;
            if (tvm == null) {
                return;
            }
            string templateText;
            try {
                templateText = DateTime.Now.ToString(tvm.TemplateData);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error converting date time format '{tvm.TemplateData}', using default", ex);
                templateText = DateTime.Now.ToString();
            }
            tvm.TemplateText = templateText;
        }

        private void ContactComboBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var contact_cmb = sender as ComboBox;
            var tvm = contact_cmb.DataContext as MpTextTemplateViewModelBase;
            MpHelpers.RunOnMainThread(async () => {
                tvm.IsBusy = true;

                contact_cmb.ItemsSource = await MpMasterTemplateModelCollectionViewModel.Instance.GetContacts();

                if (contact_cmb.Items.Count == 0) {
                    contact_cmb.Items.Add(MpContact.EmptyContact);
                }
                tvm.IsBusy = false;
            });
        }
    }
}
