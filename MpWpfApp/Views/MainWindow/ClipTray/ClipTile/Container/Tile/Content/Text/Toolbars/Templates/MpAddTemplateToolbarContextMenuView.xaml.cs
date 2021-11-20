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

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpAddTemplateToolbarContextMenuView.xaml
    /// </summary>
    public partial class MpAddTemplateToolbarButton : UserControl {
        RichTextBox Rtb;
        public MpAddTemplateToolbarButton() {
            InitializeComponent();
        }

        public MpAddTemplateToolbarButton(RichTextBox rtb) : this() {
            SetActiveRtb(rtb);            
        }

        public void SetActiveRtb(RichTextBox rtb) {
            Rtb = rtb;
        }

        private void AddTemplateContextMenu_Opened(object sender, RoutedEventArgs e) {
            var tc = (Rtb.DataContext as MpContentItemViewModel).TemplateCollection.Templates;

            var mil = new List<MenuItem>();
            foreach(var thvm in tc) {
                var mi = new MenuItem() {
                    Header = thvm.TemplateName,
                    Icon = new Border() { 
                        BorderBrush = Brushes.Black,
                        Background = thvm.TemplateBrush
                    },
                    DataContext = thvm
                };
                mil.Add(mi);
            }

            var ami = new MenuItem() {
                Header = "Add New..."
            };
            var icon = new Image();
            icon.Source = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Icons/Silk/icons/add.png"));
            ami.Icon = icon;

            mil.Add(ami);
            foreach (MenuItem mi in mil) {
                mi.Click += TemplateDropDownItem_Click;
                mi.Unloaded += Mi_Unloaded;
            }

            AddButton.ContextMenu.ItemsSource = mil;
        }

        private void Mi_Unloaded(object sender, RoutedEventArgs e) {
            var mi = sender as MenuItem;
            if(mi == null) {
                return;
            }
            mi.Click -= TemplateDropDownItem_Click;
            mi.Unloaded -= Mi_Unloaded;
        }

        private void TemplateDropDownItem_Click(object sender, RoutedEventArgs e) {
            MpHelpers.Instance.RunOnMainThread(async () => {
                Rtb.Focus();
                MpTemplateHyperlink thl = null;
                var mi = sender as MenuItem;
                if (mi.DataContext == null) {
                    //when clicking add new
                    thl = await MpTemplateHyperlink.Create(Rtb.Selection, null);
                } else if (mi.DataContext is MpTemplateViewModel thlvm) {
                    //when clicking a pre-existing template
                    thl = await MpTemplateHyperlink.Create(Rtb.Selection, thlvm.CopyItemTemplate);
                }

                //add trailing run of one space to allow clicking after template
                //new Run(@" ", thl.ElementEnd);

                var ettbv = Rtb.GetVisualAncestor<MpContentListView>().GetVisualDescendent<MpEditTemplateToolbarView>();
                ettbv.SetActiveRtb(Rtb);

                (thl.DataContext as MpTemplateViewModel).WasNewOnEdit = true;

                thl.EditTemplate();
            });
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) {
            if(Rtb == null || Rtb.DataContext == null) {
                MonkeyPaste.MpConsole.WriteTraceLine("No rtb or rtb context");
                return;
            }
            MpHelpers.Instance.RunOnMainThread(async () => {
                var rtbv = Rtb.GetVisualAncestor<MpRtbView>();
                rtbv.NewOriginalText = Rtb.Selection.Text;
                rtbv.NewStartRange = Rtb.Selection;

                var rtbvm = Rtb.DataContext as MpContentItemViewModel;
                if (rtbvm.TemplateCollection.Templates.Count == 0) {
                    //when no templates exist create a new default one
                    var thl = await MpTemplateHyperlink.Create(Rtb.Selection, null);
                    var thlvm = thl.DataContext as MpTemplateViewModel;
                    thlvm.EditTemplateCommand.Execute(null);
                } else {
                    //otherwise show template menu
                    AddButton.ContextMenu.IsOpen = true;
                }
            });
        }

        private void AddButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(AddButton.IsEnabled) {
                AddButtonImage.Source = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/MpRichTextBox/templateadd.png"));
                AddButton.ToolTip = "Add Template";
            } else {
                AddButtonImage.Source = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/MpRichTextBox/templatedisabled.png"));
                var tb = new TextBlock() {
                    Foreground = Brushes.Red,
                    Background = Brushes.White,
                    FontSize = 12,
                    Text = @"-current selection intersects with a template" + Environment.NewLine +
                            @"-contains a space" + Environment.NewLine +
                            @"-contains more than 10 characters"
                };
                AddButton.ToolTip = tb;
            }
        }
    }
}
