

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common.Avalonia;
using MonoMac.ImageKit;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAnalyticItemSelectorView.xaml
    /// </summary>
    public partial class MpAvTriggerActionChooserView : MpAvUserControl<MpAvTriggerCollectionViewModel> {
        public MpAvTriggerActionChooserView() {
            InitializeComponent();
            //var tcmb = this.FindControl<ComboBox>("TriggerComboBox");
            //tcmb.SelectionChanged += Tcmb_SelectionChanged;
            //tcmb.AttachedToVisualTree += Tcmb_AttachedToVisualTree;
            //if(BindingContext != null) {
            //    MpAvTriggerActionChooserView_DataContextChanged(this, null);
            //}
            //this.DataContextChanged += MpAvTriggerActionChooserView_DataContextChanged;
        }


        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        //private void MpAvTriggerActionChooserView_DataContextChanged(object sender, EventArgs e) {
        //    if(BindingContext == null) {
        //        return;
        //    }
        //    BindingContext.PropertyChanged += BindingContext_PropertyChanged;
        //    var tcmb = this.FindControl<ComboBox>("TriggerComboBox");
        //    tcmb.SelectedItem = BindingContext.SelectedItem;
        //}

        //private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        //    switch(e.PropertyName) {
        //        case nameof(BindingContext.SelectedItem):
        //            if(BindingContext.SelectedItem == null) {
        //                //Debugger.Break();
        //                return;
        //            }
        //            var tcmb = this.FindControl<ComboBox>("TriggerComboBox");
        //            tcmb.SelectedItem = BindingContext.SelectedItem;
        //            break;
        //    }
        //}

        //private void Tcmb_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
        //    var tcmb = sender as ComboBox;
        //    if(BindingContext == null) {
        //        //Debugger.Break();
        //        return;
        //    }
        //    tcmb.SelectedItem = BindingContext.SelectedItem;
        //}
        //private void Tcmb_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        //    var tcmb = sender as ComboBox;
        //    if(tcmb.SelectedItem == null) {
        //        //Debugger.Break();
        //        return;
        //    }
        //    BindingContext.SelectActionCommand.Execute(tcmb.SelectedItem as MpAvTriggerActionViewModelBase);
        //}
    }
}
