using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using MonkeyPaste.Common.Avalonia;
using System;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAnalyticItemSelectorView.xaml
    /// </summary>
    public partial class MpAvTriggerActionChooserView : MpAvUserControl<MpAvTriggerCollectionViewModel> {
        public MpAvTriggerActionChooserView() {
            AvaloniaXamlLoader.Load(this);
            //var tcmb = this.FindControl<ComboBox>("TriggerComboBox");
            //tcmb.SelectionChanged += Tcmb_SelectionChanged;
            //tcmb.AttachedToVisualTree += Tcmb_AttachedToVisualTree;
            if (BindingContext != null) {
                MpAvTriggerActionChooserView_DataContextChanged(this, null);
            }
            this.DataContextChanged += MpAvTriggerActionChooserView_DataContextChanged;
        }


        private void MpAvTriggerActionChooserView_DataContextChanged(object sender, EventArgs e) {
            if (BindingContext == null) {
                return;
            }
            BindingContext.PropertyChanged += BindingContext_PropertyChanged;
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (MpAvSidebarItemCollectionViewModel.Instance.SelectedItem != BindingContext) {
                return;
            }

            switch (e.PropertyName) {
                case nameof(BindingContext.SidebarOrientation):
                    var g = this.FindControl<Grid>("TriggerSidebarContainer");
                    var vert_sv = this.FindControl<ScrollViewer>("TriggerScrollViewer");
                    var horiz_sv = this.FindControl<ScrollViewer>("PropertyScrollViewer");
                    var adc = this.FindControl<Control>("ActionDesignerOuterContainerBorder");
                    if (g == null) {
                        return;
                    }
                    g.RowDefinitions.Clear();
                    g.ColumnDefinitions.Clear();
                    if (BindingContext.SidebarOrientation == Orientation.Horizontal) {
                        var rd = new RowDefinition(GridLength.Star);
                        if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                            rd.Bind(
                            RowDefinition.MaxHeightProperty,
                            new Binding() {
                                Source = MpAvClipTrayViewModel.Instance,
                                Path = nameof(MpAvClipTrayViewModel.Instance.ObservedContainerScreenHeight),
                                Mode = BindingMode.OneWay
                            });
                        } else {
                            rd.Bind(
                            RowDefinition.HeightProperty,
                            new Binding() {
                                Source = this,
                                Path = nameof(Bounds.Height),
                                Mode = BindingMode.OneWay
                            });
                        }

                        g.RowDefinitions.Add(rd);
                        g.ColumnDefinitions.AddRange(new[] { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) });


                        Grid.SetRow(adc, 0);
                        Grid.SetColumn(adc, 1);

                        vert_sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                        vert_sv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

                        horiz_sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                        horiz_sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;


                        adc.MinHeight = 0;
                    } else {
                        g.RowDefinitions.AddRange(new[] { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) });
                        g.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                        adc.MinHeight = 350;
                        //adc.MaxHeight = double.PositiveInfinity;
                        //adc.Height = double.NaN;
                        Grid.SetRow(adc, 1);
                        Grid.SetColumn(adc, 0);

                        vert_sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                        vert_sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                        horiz_sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                        horiz_sv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    }

                    BindingContext.ResetDesignerViewCommand.Execute(null);
                    break;
            }
        }

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
