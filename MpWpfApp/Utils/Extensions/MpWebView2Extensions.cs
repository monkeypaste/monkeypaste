using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public static class MpWebView2Extensions {

        public static void FitDocToWebView(this ChromiumWebBrowser wv2) {
            bool isReadOnly = false;
            if (wv2.DataContext is MpContentItemViewModel civm) {
                isReadOnly = civm.IsReadOnly;
            }
            if (!isReadOnly) {
                var clv = wv2.GetVisualAncestor<MpContentListView>();
                double w = clv == null ? wv2.ActualWidth : clv.ActualWidth;
                double h = clv == null ? wv2.ActualHeight : clv.ActualHeight;
                wv2.Width = Math.Max(0, w - wv2.Margin.Left - wv2.Margin.Right/* - wv2.Padding.Left - wv2.Padding.Right*/);
                wv2.Height = h;// Math.Max(0, wv2.ActualHeight - wv2.Margin.Top - wv2.Margin.Bottom/* - wv2.Padding.Top - wv2.Padding.Bottom*/);
            } else {
                wv2.Width = Math.Max(0, wv2.ActualWidth - wv2.Margin.Left - wv2.Margin.Right/* - wv2.Padding.Left - wv2.Padding.Right*/);
                wv2.Height = Math.Max(0, wv2.ActualHeight - wv2.Margin.Top - wv2.Margin.Bottom/* - wv2.Padding.Top - wv2.Padding.Bottom*/);
            }

            wv2.UpdateLayout();
        }
    }
}
