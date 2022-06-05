using Microsoft.Office.Interop.Outlook;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using Application = System.Windows.Application;

namespace MpWpfApp {
    public class MpStylizedTooltipExtension : DependencyObject {
        #region TooltipInfoViewModel DependencyProperty

        public static MpITooltipInfoViewModel GetTooltipInfoViewModel(DependencyObject obj) {
            return (MpITooltipInfoViewModel)obj.GetValue(TooltipInfoViewModelProperty);
        }

        public static void SetTooltipInfoViewModel(DependencyObject obj, MpITooltipInfoViewModel value) {
            obj.SetValue(TooltipInfoViewModelProperty, value);
        }

        public static readonly DependencyProperty TooltipInfoViewModelProperty =
            DependencyProperty.Register(
                "TooltipInfoViewModel", 
                typeof(MpITooltipInfoViewModel), 
                typeof(MpStylizedTooltipExtension), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region IsEnabled DependencyProperty

        public static bool GetIsEnabled(DependencyObject obj) {
            return (bool)obj.GetValue(IsEnabledProperty);
        }
        public static void SetIsEnabled(DependencyObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }
        public static readonly DependencyProperty IsEnabledProperty =
          DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(MpStylizedTooltipExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    if(e.NewValue is bool isEnabled) {
                        var fe = obj as FrameworkElement;
                        if(fe == null) {
                            if(obj == null) {
                                return;
                            }
                            throw new System.Exception("This extension must be attach to a textbox control");
                        }

                        if (isEnabled) {
                            if(fe.IsLoaded) {
                                Fe_Loaded(fe, null);
                            } else {
                                fe.Loaded += Fe_Loaded;
                            }
                        } else {
                            Fe_Unloaded(fe, null);
                        }
                    }
                }
            });

        private static void Fe_Loaded(object sender, RoutedEventArgs e) {
            var fe = sender as FrameworkElement;
            fe.Unloaded += Fe_Unloaded;

        }

        private static void Fe_Unloaded(object sender, RoutedEventArgs e) {
            var fe = sender as FrameworkElement;
            fe.Loaded -= Fe_Loaded;
            fe.Unloaded -= Fe_Unloaded;
        }


        #endregion
    }
}