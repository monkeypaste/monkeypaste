﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAvActionPropertyListBoxView.xaml
    /// </summary>
    public partial class MpAvActionPropertyView : MpAvUserControl<MpAvActionViewModelBase> {
        public MpAvActionPropertyView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }


    }
}