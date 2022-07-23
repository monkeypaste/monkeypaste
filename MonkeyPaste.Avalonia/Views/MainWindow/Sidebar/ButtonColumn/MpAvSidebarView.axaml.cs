using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSidebarView : MpAvUserControl<MpAvClipTrayViewModel> {
        public ToggleButton AppendModeToggleButton, MouseModeToggleButton;

        public MpAvSidebarView() {
            InitializeComponent();
            this.DataContextChanged += MpAvSidebarView_DataContextChanged;

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        private void MpAvSidebarView_DataContextChanged(object sender, System.EventArgs e) {
            if(BindingContext == null) {
                return;
            }
            BindingContext.PropertyChanged += BindingContext_PropertyChanged;
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.MainWindowOrientationChanged:
                    var ctg = this.FindControl<Grid>("SidebarContainerGrid");
                    var tbl = ctg.GetVisualDescendants<ToggleButton>().ToList();

                    if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                        // horizontal shows sidebar down left side
                        ctg.ColumnDefinitions.Clear();
                        ctg.RowDefinitions = new RowDefinitions("*,*,*,*,*");
                        tbl.ForEach(x => Grid.SetColumn(x, 0));
                        tbl.ForEach(x => Grid.SetRow(x, tbl.IndexOf(x)));
                    } else {
                        // vertical shows sidebar across bottom
                        ctg.RowDefinitions.Clear();
                        ctg.ColumnDefinitions = new ColumnDefinitions("*,*,*,*,*");
                        tbl.ForEach(x => Grid.SetRow(x, 0));
                        tbl.ForEach(x => Grid.SetColumn(x, tbl.IndexOf(x)));
                    }
                    break;
            }
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(BindingContext.IsAppendMode):
                case nameof(BindingContext.IsAppendLineMode):
                    if (AppendModeToggleButton.ContextMenu != null) {
                        AppendModeToggleButton.ContextMenu.Close();
                    }

                    //AppendModeToggleButton.IsChecked = BindingContext.IsAnyAppendMode;
                    BindingContext.OnPropertyChanged(nameof(BindingContext.IsAnyAppendMode));

                    break;
                case nameof(BindingContext.IsAutoCopyMode):
                case nameof(BindingContext.IsRightClickPasteMode):
                    if (AppendModeToggleButton.ContextMenu != null) {
                        AppendModeToggleButton.ContextMenu.Close();
                    }
                    MouseModeToggleButton.IsChecked = BindingContext.IsAnyMouseModeEnabled;
                    BindingContext.OnPropertyChanged(nameof(BindingContext.IsAnyMouseModeEnabled));
                    break;
            }
        }
    }
}
