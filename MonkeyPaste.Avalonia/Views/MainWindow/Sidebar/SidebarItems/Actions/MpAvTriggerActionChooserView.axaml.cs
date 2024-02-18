using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using System;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAnalyticItemSelectorView.xaml
    /// </summary>
    public partial class MpAvTriggerActionChooserView : MpAvUserControl<MpAvTriggerCollectionViewModel> {
        public MpAvTriggerActionChooserView() {
            InitializeComponent();
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
            if (MpAvSidebarItemCollectionViewModel.Instance.SelectedItem != BindingContext &&
                !BindingContext.IsWindowOpen) {
                return;
            }

            switch (e.PropertyName) {
                case nameof(BindingContext.SidebarOrientation):
                    UpdateDesignerLayout();
                    break;
                case nameof(BindingContext.IsSelected):
                    //if (!BindingContext.IsSelected) {
                    //    break;
                    //}
                    UpdateDesignerLayout();
                    break;
            }
        }
        private void UpdateDesignerLayout() {
            var g = this.FindControl<Grid>("TriggerSidebarContainer");
            var vert_sv = this.FindControl<ScrollViewer>("TriggerScrollViewer");
            var horiz_sv = this.FindControl<ScrollViewer>("PropertyScrollViewer");
            var oc = this.FindControl<Control>("TriggerSidebarOuterContainer");
            var adc = this.FindControl<Control>("ActionDesignerOuterContainerBorder");
            var apc = this.FindControl<Control>("ActionPropertyOuterContainer");
            if (g == null) {
                return;
            }
            g.RowDefinitions.Clear();
            g.ColumnDefinitions.Clear();

            if (BindingContext.SidebarOrientation == Orientation.Horizontal) {
                var rd = new RowDefinition(GridLength.Star);
                if (!BindingContext.IsWindowOpen) {
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
                }

                g.RowDefinitions.Add(rd);
                g.ColumnDefinitions.AddRange(
                    new[] {
                                new ColumnDefinition(GridLength.Auto),
                                new ColumnDefinition(GridLength.Star) });
                Grid.SetRow(adc, 0);
                Grid.SetColumn(adc, 1);

                if (BindingContext.IsWindowOpen) {
                    vert_sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    vert_sv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    horiz_sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    horiz_sv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                } else {
                    vert_sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    vert_sv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

                    horiz_sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    horiz_sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                }


                adc.MinHeight = 0;

                // NOTE setting the height here is needed or measure fails because height becomes NaN
                //apc.Height = Math.Min(apc.Bounds.Height, oc.Bounds.Height);
                //apc.Bind(
                //    Control.MaxHeightProperty,
                //    new Binding() {
                //        Source = oc,
                //        Path = nameof(Bounds.Height),
                //        Mode = BindingMode.OneWay
                //    });
            } else {
                g.RowDefinitions.AddRange(new[] {
                            new RowDefinition(GridLength.Auto),
                            new RowDefinition(GridLength.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                Grid.SetRow(adc, 1);
                Grid.SetColumn(adc, 0);

                vert_sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                vert_sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                horiz_sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                horiz_sv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

                adc.MinHeight = 350;
                //apc.MaxHeight = double.PositiveInfinity;
            }

            BindingContext.ResetDesignerViewCommand.Execute(null);
        }
    }
}
