﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(CopyItemId), "CopyItemId")]
    public partial class MpCopyItemTagAssociationPageView : ContentPage {
        public int CopyItemId {
            set {
                LoadCopyItem(value);
            }
        }
        public MpCopyItemTagAssociationPageView()  {
            InitializeComponent();
        }

        //public MpCopyItemTagAssociationPageView(MpCopyItemTagAssociationPageViewModel ctapvm) : base() {
        //    InitializeComponent();
        //    //BindingContext = ctapvm;
        //}

        private async void LoadCopyItem(int ciid) {
            try {
                var ci = await MpDb.GetItemAsync<MpCopyItem>(ciid);
                BindingContext = new MpCopyItemTagAssociationPageViewModel(ci);
            }
            catch (Exception) {
                MpConsole.WriteLine($"Failed to load copy item {ciid}.");
            }
        }
    }
}