using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace MpWpfApp {
    public class MultiSelectCollectionView<T,C> : ListCollectionView, IMultiSelectCollectionView
        where T: MpViewModelBase
        where C: MpViewModelBase<T> {
        
        #region Private Variables
        
        List<Selector> controls = new List<Selector>();

        #endregion

        #region Properties

        public bool IgnoreSelectionChanged { get; set; }

        public ObservableCollection<T> SelectedItems { get; private set; }

        #endregion

        #region Constructors

        public MultiSelectCollectionView(IList list) : base(list) {
            SelectedItems = new ObservableCollection<T>();
        }

        #endregion

        #region IMultiSelectCollectionView Implementation

        void IMultiSelectCollectionView.AddControl(Selector selector) {
            this.controls.Add(selector);
            SetSelection(selector);
            selector.SelectionChanged += control_SelectionChanged;
            if(selector is ListBox lb) {
                lb.GetScrollViewer().ScrollChanged += MultiSelectCollectionView_ScrollChanged;
            }
        }

        private void MultiSelectCollectionView_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            SetSelection((sender as ScrollViewer).GetVisualAncestor<ListBox>());
        }

        void IMultiSelectCollectionView.RemoveControl(Selector selector) {
            if (this.controls.Remove(selector)) {
                selector.SelectionChanged -= control_SelectionChanged;
            }
        }

        public void UpdateSelection() {

        }
        #endregion

        #region Private Methods

        void SetSelection(Selector selector) {
            MultiSelector multiSelector = selector as MultiSelector;
            ListBox listBox = selector as ListBox;

            if (multiSelector != null) {
                multiSelector.SelectedItems.Clear();

                foreach (T item in SelectedItems) {
                    multiSelector.SelectedItems.Add(item);
                }
            } else if (listBox != null) {
                listBox.SelectedItems.Clear();

                foreach (T item in SelectedItems) {
                    listBox.SelectedItems.Add(item);
                }
            }
        }

        void control_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!this.IgnoreSelectionChanged) {
                bool changed = false;

                this.IgnoreSelectionChanged = true;

                try {
                    foreach (var addedItem in e.AddedItems) {
                        T item = addedItem is T ? (T)addedItem : ((C)addedItem).Parent;
                        if (!SelectedItems.Contains(item)) {
                            SelectedItems.Add(item);
                            changed = true;
                        }
                    }

                    foreach (var removedItem in e.RemovedItems) {
                        T item = removedItem is T ? (T)removedItem : ((C)removedItem).Parent;
                        if (SelectedItems.Remove(item)) {
                            changed = true;
                        }
                    }

                    if (changed) {
                        foreach (Selector control in this.controls) {
                            if (control != sender) {
                                SetSelection(control);
                            }
                        }
                    }
                }
                finally {
                    this.IgnoreSelectionChanged = false;
                }
            }
        }

        #endregion
    }
}
