using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    [TemplatePart(Name = "PART_AnimatedScrollViewer", Type = typeof(AnimatedScrollViewer))]

    public class AnimatedListView : ListView {
        #region PART holders
        public AnimatedScrollViewer AnimatedScrollViewer;
        #endregion

        static AnimatedListView() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnimatedListView), new FrameworkPropertyMetadata(typeof(AnimatedListView)));
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            AnimatedScrollViewer scrollViewerHolder = base.GetTemplateChild("PART_AnimatedScrollViewer") as AnimatedScrollViewer;
            if (scrollViewerHolder != null) {
                AnimatedScrollViewer = scrollViewerHolder;
            }

            this.SelectionChanged += new SelectionChangedEventHandler(AnimatedListView_SelectionChanged);
            this.Loaded += new RoutedEventHandler(AnimatedListView_Loaded);
            this.LayoutUpdated += new EventHandler(AnimatedListView_LayoutUpdated);
        }

        void AnimatedListView_LayoutUpdated(object sender, EventArgs e) {
            updateScrollPosition(sender);
        }

        void AnimatedListView_Loaded(object sender, RoutedEventArgs e) {
            updateScrollPosition(sender);
        }

        void AnimatedListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            updateScrollPosition(sender);
        }

        public void updateScrollPosition(object sender) {
            AnimatedListView thisLB = (AnimatedListView)sender;

            if (thisLB != null) {
                if (thisLB.ScrollToSelectedItem) {
                    double scrollTo = 0;
                    for (int i = 0; i < (thisLB.SelectedIndex + thisLB.SelectedIndexOffset); i++) {
                        ListViewItem tempItem = thisLB.ItemContainerGenerator.ContainerFromItem(thisLB.Items[i]) as ListViewItem;

                        if (tempItem != null) {
                            scrollTo += IsListViewHorizontal ? tempItem.ActualWidth : tempItem.ActualHeight;
                        }
                    }

                    if(IsListViewHorizontal) {
                        AnimatedScrollViewer.TargetHorizontalOffset = scrollTo;
                    } else {
                        AnimatedScrollViewer.TargetVerticalOffset = scrollTo;
                    }
                }
            }
        }



        #region ScrollToSelectedItem (DependencyProperty)

        /// <summary>
        /// A description of the property.
        /// </summary>
        public bool ScrollToSelectedItem {
            get { return (bool)GetValue(ScrollToSelectedItemProperty); }
            set { SetValue(ScrollToSelectedItemProperty, value); }
        }
        public static readonly DependencyProperty ScrollToSelectedItemProperty =
            DependencyProperty.Register("ScrollToSelectedItem", typeof(bool), typeof(AnimatedListView),
            new PropertyMetadata(false));
        #endregion

        #region SelectedIndexOffset (DependencyProperty)

        /// <summary>
        /// Use this property to choose the scroll to an item that is not selected, but is X above or below the selected item
        /// </summary>
        public int SelectedIndexOffset {
            get { return (int)GetValue(SelectedIndexOffsetProperty); }
            set { SetValue(SelectedIndexOffsetProperty, value); }
        }
        public static readonly DependencyProperty SelectedIndexOffsetProperty =
            DependencyProperty.Register("SelectedIndexOffset", typeof(int), typeof(AnimatedListView),
              new PropertyMetadata(0));

        #endregion

        public bool IsListViewHorizontal {
            get { return (bool)GetValue(IsListViewHorizontalProperty); }
            set { SetValue(IsListViewHorizontalProperty, value); }
        }
        public static readonly DependencyProperty IsListViewHorizontalProperty =
            DependencyProperty.Register(
                "IsListViewHorizontal", 
                typeof(bool), 
                typeof(AnimatedListView),
                new PropertyMetadata(false));
    }
}
