﻿using MonkeyPaste;
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
    /// Interaction logic for MpActionPropertyListBoxView.xaml
    /// </summary>
    public partial class MpActionPropertyListBoxView : MpUserControl<MpActionCollectionViewModel> {
        public MpActionPropertyListBoxView() {
            InitializeComponent();
        }

        private void ActionPropertyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var lb = sender as ListBox;
            lb.ScrollIntoView(MpActionCollectionViewModel.Instance.PrimaryAction);
        }

        private void ActionPropertyListBox_Loaded(object sender, RoutedEventArgs e) {

            MpMessenger.Register(
                MpActionCollectionViewModel.Instance,
                ReceivedActionCollectionViewModelMessage);
        }

        private void ReceivedActionCollectionViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ActionViewportChanged:
                    ActionPropertyListBox.Items.Refresh();
                    break;
            }
        }
    }
}