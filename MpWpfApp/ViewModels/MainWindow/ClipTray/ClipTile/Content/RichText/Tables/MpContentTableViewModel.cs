﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    
    public class MpContentTableViewModel : MpViewModelBase<MpClipTileViewModel>,
        MpIContextMenuViewModel,
        MpIPopupMenuViewModel {
        #region Properties

        #region MpIPopupMenuItemViewModel Implementation

        public MpMenuItemViewModel PopupMenuViewModel { 
            get {
                var root_mivm = new MpMenuItemViewModel() {
                    IsNewTableSelector = true,
                    SubItems = new List<MpMenuItemViewModel>()
                };

                int rows = 7;
                int cols = 7;
                string default_bg = MpSystemColors.yellow1;
                string hover_bg = MpSystemColors.Red;

                System.ComponentModel.PropertyChangedEventHandler mivmPropChangedHandler = (s, e) => {
                    var cur_mivm = s as MpMenuItemViewModel;
                    switch(e.PropertyName) {
                        case nameof(cur_mivm.IsHovering):

                            //clear all cells to default
                            root_mivm.SubItems.ForEach(x => x.Header = default_bg);

                            if(cur_mivm.IsHovering) {
                                if (cur_mivm.CommandParameter is int[] argParts) {
                                    int cur_row = (int)argParts[0];
                                    int cur_col = (int)argParts[1];

                                    for (int r = 0; r < rows; r++) {
                                        for (int c = 0; c < cols; c++) {
                                            var this_mivm = root_mivm.SubItems
                                                        .FirstOrDefault(x => x.CommandParameter is int[] paramParts && paramParts[0] == r && paramParts[1] == c);

                                            if(this_mivm == null) {
                                                Debugger.Break();
                                            }

                                            this_mivm.Header = r <= cur_row && c <= cur_col ?
                                                    hover_bg : default_bg;

                                        }
                                    }
                                    //Debugger.Break();
                                }
                            }
                            
                            break;
                    }
                };

                for (int r = 0; r < rows; r++) {
                    for (int c = 0; c < cols; c++) {
                        var cell_mivm = new MpMenuItemViewModel() {
                            IsVisible = true,
                            Header = default_bg,
                            Command = CreateTableCommand,
                            CommandParameter = new int[] { r, c }
                        };
                        cell_mivm.PropertyChanged += mivmPropChangedHandler;

                        root_mivm.SubItems.Add(cell_mivm);
                    }
                }
                //return root_mivm;

                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel> { root_mivm }
                };
            }
        }

        #endregion

        #region MpIContextMenuViewModel Implementation

        public MpMenuItemViewModel ContextMenuViewModel {
            get {
                var cmvm = new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>()
                };

                for (int i = 0; i < Enum.GetValues(typeof(MpContentTableContextActionType)).Length; i++) {
                    if (i == 0) {
                        continue;
                    }
                    MpContentTableContextActionType tableAction = (MpContentTableContextActionType)i;
                    var mivm = new MpMenuItemViewModel() {
                        IconResourceKey = new MpEnumToImageSourceConverter().Convert(tableAction, null, null, null) as string,
                        Header = tableAction.EnumToLabel(),
                        Command = PerformTableActionCommand,
                        CommandParameter = tableAction
                    };
                    cmvm.SubItems.Add(mivm);
                    if (tableAction == MpContentTableContextActionType.UnmergeCells) {
                        cmvm.SubItems.Add(new MpMenuItemViewModel() { IsSeparator = true });
                    } 
                }
                return cmvm;
            }
        }

        #endregion

        #region MpIRtfSelectionRange (Table) Properties

        public IEnumerable<Table> SelectedTables {
            get {
                if(Parent == null) {
                    return null;
                }
                return MpContentDocumentRtfExtension.GetSelectedTables(Parent);
            }
        }

        public IEnumerable<TableCell> SelectedTableCells {
            get {
                if (Parent == null) {
                    return null;
                }
                return MpContentDocumentRtfExtension.GetSelectedTableCells(Parent);
            }
        }
        #endregion

        #region State


        #endregion


        #endregion

        #region Constructors

        public MpContentTableViewModel() : base(null) { }

        public MpContentTableViewModel(MpClipTileViewModel parent) : base(parent) {
            PropertyChanged += MpContentTableViewModel_PropertyChanged;
        }

        private void MpContentTableViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            throw new NotImplementedException();
        }




        #endregion

        #region Public Methods

        #endregion

        #region Commands

        public ICommand CreateTableCommand => new MpCommand<object>(
            (args) => {
                if(args is int[] argParts) {
                    // NOTE the +1 is because arg is cells index location not dimension
                    int rowCount = (int)argParts[0] + 1;
                    int colCount = (int)argParts[1] + 1;

                    string tableCsv = string.Empty;
                    for (int r = 0; r <= rowCount; r++) {
                        for (int c = 0; c <= colCount; c++) {
                            tableCsv += ",";
                        }
                        tableCsv += Environment.NewLine;
                    }
                    
                    var rtbcv = Application.Current.MainWindow
                                .GetVisualDescendents<MpRtbContentView>()
                                .FirstOrDefault(x => x.DataContext == Parent);

                    string tableRtf = tableCsv.ToRichTextTable();
                    rtbcv.Rtb.Selection.LoadRtf(tableRtf, out Size tableSize);

                    MpContextMenuView.Instance.CloseMenu();
                }
            });

        public ICommand PerformTableActionCommand => new MpCommand<object>(
            (args) => {
                if(args is MpContentTableContextActionType tableAction) {

                    var tables = MpContentDocumentRtfExtension.GetSelectedTables(Parent);
                    var cells = MpContentDocumentRtfExtension.GetSelectedTableCells(Parent);

                    cells.ForEach(x => MpConsole.WriteLine("Selected cell Row: " + x.Row() + " Col: " + x.Col()));

                    switch(tableAction) {
                        case MpContentTableContextActionType.InsertColumnRight:

                            if(cells != null && cells.Count() > 0) {

                                var maxRightColumn = cells.Aggregate((a, b) => a.Col() > b.Col() ? a : b);
                                var table = maxRightColumn.Parent.FindParentOfType<Table>();
                                table.Columns.Insert(maxRightColumn.Col(), new TableColumn());
                            }
                            break;
                    }
                }
            });
        #endregion
    }
}