﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(ClipId), "ClipId")]
    public partial class MpClipDetailPageView : ContentPage {
        public int ClipId {
            set {
                LoadClip(value);
            }
        }

        public MpClipDetailPageView() : base() {
            InitializeComponent();
        }
        protected override async void OnDisappearing() {
            var cidpvm = BindingContext as MpClipDetailPageViewModel;
            
            var itemText = await cidpvm.EvaluateJavascript($"getText()");
            itemText = itemText.Replace("\"", string.Empty);
            cidpvm.Clip.ItemPlainText = itemText;

            var itemHtml = await cidpvm.EvaluateJavascript($"getHtml()");
            // Unescape that damn Unicode Java bull.
            itemHtml = Regex.Replace(
                itemHtml,
                @"\\[Uu]([0-9A-Fa-f]{4})", 
                m => char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)));
            itemHtml = Regex.Unescape(itemHtml);
            //itemHtml = itemHtml.Replace("\"", string.Empty);
            cidpvm.Clip.ItemHtml = itemHtml;

            await MpDb.Instance.UpdateItem<MpClip>(cidpvm.Clip);

            base.OnDisappearing();
        }

        private async void LoadClip(int ciid) {
            try {
                var ci = await MpClip.GetClipById(ciid);
                BindingContext = new MpClipDetailPageViewModel(ci);
            } catch (Exception) {
                MpConsole.WriteLine($"Failed to load copy item {ciid}.");
            }
        }
    }
}