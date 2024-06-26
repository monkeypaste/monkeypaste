﻿using Avalonia.Markup.Xaml;
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
using Xamarin.Essentials;

namespace MonkeyPaste.Avalonia {
/// <summary>
/// Interaction logic for MpFileSystemTriggerPropertyListBoxItemView.xaml
/// </summary>
    public partial class MpAvContentAddTriggerPropertyView : MpAvUserControl<MpAvContentAddTriggerViewModel> {
        public MpAvContentAddTriggerPropertyView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
