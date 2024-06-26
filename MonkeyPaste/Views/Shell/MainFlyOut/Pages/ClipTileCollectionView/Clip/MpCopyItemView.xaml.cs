﻿using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MpCopyItemView : ContentView {
        MpContextMenuView cm;

        #region Events
        public event EventHandler OnGlobalTouch;
        #endregion

        public MpCopyItemView() : this(new MpCopyItemViewModel()) { }

        public MpCopyItemView(MpCopyItemViewModel viewModel) : base() {
            InitializeComponent();
            BindingContextChanged += MpCopyItemView_BindingContextChanged;
            BindingContext = viewModel;            
        }

        private void MpCopyItemView_BindingContextChanged(object sender, EventArgs e) {
            if (BindingContext != null && BindingContext is MpCopyItemViewModel civm) {
                civm.PropertyChanged += MpCopyItemViewModel_PropertyChanged;
                cm = new MpContextMenuView();
                cm.BindingContext = civm.ContextMenuViewModel;
                //OnGlobalTouch += MpCopyItemView_OnGlobalTouch;
                //(Application.Current.MainPage as MpMainShell).GlobalTouchService.Subscribe(OnGlobalTouch);
            } else {
               // OnGlobalTouch -= MpCopyItemView_OnGlobalTouch;
                //(Application.Current.MainPage as MpMainShell).GlobalTouchService.Unsubscribe(OnGlobalTouch);
            }
        }

        private void MpCopyItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var civm = sender as MpCopyItemViewModel;
            switch(e.PropertyName) {
                case nameof(civm.IsTitleReadOnly):
                    if(!civm.IsTitleReadOnly) {
                        //TitleEntry.Focus();

                    }
                    break;
            }
        }
        private void TitleEntry_Completed(object sender, EventArgs e) {
            var civm = BindingContext as MpCopyItemViewModel;
            civm.IsTitleReadOnly = false;
        }

        private void ContextMenuButton_Clicked(object sender, EventArgs e) {
            Task.Run(async () => {
                if (cm.IsMenuVisible) {
                    return;
                }
                var locationFetcher = DependencyService.Get<MpIUiLocationFetcher>();
                var location = locationFetcher.GetCoordinates(sender as VisualElement);
                if(cm.BindingContext == null) {
                    var civm = (sender as BindableObject).BindingContext as MpCopyItemViewModel;
                    cm.BindingContext = civm.ContextMenuViewModel;

                    //MpConsole.WriteTraceLine("Context menu is null...");
                    //return;
                }
                var cmvm = cm.BindingContext as MpContextMenuViewModel;
                var w = cmvm.Width;
                var h = cmvm.Height;
                var bw = ContextMenuButton.Width;
                cm.AnchorX = 0;// location.X - w;
                cm.AnchorY = 0;
                cm.TranslationX = location.X - w + bw - cmvm.Padding.Left;
                cm.TranslationY = location.Y - cmvm.ItemHeight + cmvm.Padding.Top + cmvm.Padding.Bottom;
                cm.TranslationX = Math.Max(0, cm.TranslationX);
                await PopupNavigation.Instance.PushAsync(cm, false);
            });
        }
    }
}