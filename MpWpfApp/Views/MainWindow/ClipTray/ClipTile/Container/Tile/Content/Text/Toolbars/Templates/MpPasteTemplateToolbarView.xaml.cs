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
                    BindingContext.FinishPasteTemplateCommand.Execute(null);
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

                contact_cmb.ItemsSource = await MpMasterTemplateModelCollectionViewModel.Instance.GetContactsAsync();
                
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


        private void ContactComboBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if((bool)e.NewValue) {
                var contact_cmb = sender as ComboBox;
                var tvm = contact_cmb.DataContext as MpTextTemplateViewModelBase;
                MpHelpers.RunOnMainThread(async () => {
                    tvm.IsBusy = true;

                    await Task.Delay(3000);

                    //contact_cmb.ItemsSource = await MpMasterTemplateModelCollectionViewModel.Instance.GetContacts();
                    contact_cmb.ItemsSource = new List<MpContact>() {
                        new MpContact() {
                            FirstName = "Mikey",
                            LastName = "Underwood",
                            FullName = "Mikey Underwood",
                            Email = "munderwood@yahoo.com",
                            Address = "12312 Hot dog Rd, Clement Georgia 12234",
                            PhoneNumber = "802-234-5132"
                        },
                        new MpContact() {
                            FirstName = "Nina",
                            LastName = "Jacobson",
                            FullName = "Nina Jacobson",
                            Email = "ninaaaaaaa@yahoo.com",
                            Address = "123 Fort Hunt St, Lake Patunia Mississippi 41207",
                            PhoneNumber = "213-542-5223"
                        }
                    };

                    if (contact_cmb.Items.Count == 0) {
                        contact_cmb.Items.Add(MpContact.EmptyContact);
                    }
                    tvm.IsBusy = false;
                });
            }
        }
    }
}
