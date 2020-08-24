using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpClipTileContextMenuViewModel : MpObservableCollectionViewModel<MpClipTileContextMenuItemViewModel> {
        private MpClipTrayViewModel _clipTrayViewModel = null;
        public MpClipTrayViewModel ClipTrayViewModel {
            get {
                return _clipTrayViewModel;
            }
            set {
                if (_clipTrayViewModel != value) {
                    _clipTrayViewModel = value;
                    OnPropertyChanged(nameof(ClipTrayViewModel));
                }
            }
        }

        private ObservableCollection<MpClipTileContextMenuItemViewModel> _exportTypes = new ObservableCollection<MpClipTileContextMenuItemViewModel>();
        public ObservableCollection<MpClipTileContextMenuItemViewModel> ExportTypes {
            get {
                return _exportTypes;
            }
            set {
                if(_exportTypes != value) {
                    _exportTypes = value;
                    OnPropertyChanged(nameof(ExportTypes));
                }
            }
        }
        public MpClipTileContextMenuViewModel(MpClipTrayViewModel clipTrayViewModel) {
            ClipTrayViewModel = clipTrayViewModel;
            Add(
                new MpClipTileContextMenuItemViewModel(
                    "Bring To Front", 
                    ClipTrayViewModel.BringSelectedClipTilesToFrontCommand,
                    null,
                    false,
                    null));

            ExportTypes = new ObservableCollection<MpClipTileContextMenuItemViewModel>() {
                        new MpClipTileContextMenuItemViewModel(
                            "Files...",
                            ClipTrayViewModel.ExportSelectedClipTilesCommand,
                            null,
                            false,
                            null),
                        new MpClipTileContextMenuItemViewModel(
                            "CSV",
                            ClipTrayViewModel.ExportSelectedClipTilesCommand,
                            null,
                            false,
                            null)
                    };
            Add(
                new MpClipTileContextMenuItemViewModel(
                    "Export To", 
                    ClipTrayViewModel.BringSelectedClipTilesToFrontCommand, 
                    null,
                    false,
                    ExportTypes));

            /*
             * ObservableCollection<MpClipTileContextMenuItemViewModel> tagMenuItems = new ObservableCollection<MpClipTileContextMenuItemViewModel>();
                var tagTiles = ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel;
                foreach (var tagTile in tagTiles) {
                    if (tagTile.TagName == Properties.Settings.Default.HistoryTagTitle) {
                        continue;
                    }
                    tagMenuItems.Add(new MpClipTileContextMenuItemViewModel(tagTile, ClipTrayViewModel.LinkTagToCopyItemCommand, tagTile.Tag.IsLinkedWithCopyItem(CopyItem)));
                }
                return tagMenuItems;
             <ContextMenu x:Name="ClipTile_ContextMenu" DataContext="{Binding ClipTrayViewModel}">
                                        <MenuItem Header="Export To">
                                            <MenuItem Header="File(s)..."
                                                      Command="{Binding ExportSelectedClipTilesCommand}"
                                                      CommandParameter="{StaticResource False}" />
                                            <MenuItem Header="CSV"
                                                      Command="{Binding ExportSelectedClipTilesCommand}"
                                                      CommandParameter="{StaticResource True}" />
                                        </MenuItem>
                                        <MenuItem Header="Rename"
                                                  Command="{Binding RenameClipCommand}" />
                                        <MenuItem Header="Delete"
                                                  Command="{Binding DeleteSelectedClipsCommand}" />
                                        <!--<MenuItem Header="Say"
                                                  Command="{Binding SpeakClipCommand}" />-->
                                        <MenuItem Header="Merge"
                                                  Command="{Binding MergeClipsCommand}"
                                                  Visibility="{Binding MergeClipsCommandVisibility}" />
                                        <MenuItem Header="Search Web">
                                            <MenuItem Header="Google">
                                                <i:Interaction.Triggers>
                                                    <i:EventTrigger EventName="PreviewMouseLeftButtonUp">
                                                        <i:CallMethodAction MethodName="ContextMenuMouseLeftButtonUpOnSearchGoogle"
                                                                            TargetObject="{Binding}" />
                                                    </i:EventTrigger>
                                                </i:Interaction.Triggers>
                                            </MenuItem>
                                            <MenuItem Header="Bing">
                                                <i:Interaction.Triggers>
                                                    <i:EventTrigger EventName="PreviewMouseLeftButtonUp">
                                                        <i:CallMethodAction MethodName="ContextMenuMouseLeftButtonUpOnSearchBing"
                                                                            TargetObject="{Binding}" />
                                                    </i:EventTrigger>
                                                </i:Interaction.Triggers>
                                            </MenuItem>
                                            <MenuItem Header="DuckDuckGo">
                                                <i:Interaction.Triggers>
                                                    <i:EventTrigger EventName="PreviewMouseLeftButtonUp">
                                                        <i:CallMethodAction MethodName="ContextMenuMouseLeftButtonUpOnSearchDuckDuckGo"
                                                                            TargetObject="{Binding}" />
                                                    </i:EventTrigger>
                                                </i:Interaction.Triggers>
                                            </MenuItem>
                                            <MenuItem Header="Yandex">
                                                <i:Interaction.Triggers>
                                                    <i:EventTrigger EventName="PreviewMouseLeftButtonUp">
                                                        <i:CallMethodAction MethodName="ContextMenuMouseLeftButtonUpOnSearchYandex"
                                                                            TargetObject="{Binding}" />
                                                    </i:EventTrigger>
                                                </i:Interaction.Triggers>
                                            </MenuItem>
                                        </MenuItem>
                                        <MenuItem Header="Pin To"
                                                  ItemsSource="{Binding TagMenuItems}">
                                            <MenuItem.ItemContainerStyle>
                                                <Style TargetType="{x:Type MenuItem}">
                                                    <Setter Property="Header"
                                                            Value="{Binding Header}" />
                                                    <Setter Property="Command"
                                                            Value="{Binding Command}" />
                                                    <Setter Property="CommandParameter"
                                                            Value="{Binding TagViewModel}" />
                                                    <Setter Property="IsCheckable"
                                                            Value="True" />
                                                    <Setter Property="IsChecked"
                                                            Value="{Binding IsTagLinkedToClip}" />
                                                </Style>
                                            </MenuItem.ItemContainerStyle>
                                        </MenuItem>
                                    </ContextMenu>
             */

        }
    }
}
