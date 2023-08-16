// #region Life Cycle

function initTable() {
    initTableToolbarButton();
    initTableOps()
    globals.DefaultCsvProps.RowSeparator = envNewLine();

    //let editor_elm = getEditorElement();
    //if (editor_elm == null) {
    //    debugger;
    //}
    //editor_elm.addEventListener("contextmenu", onWindowMouseDown, true);
}

// #endregion Life Cycle

// #region Getters

function getTableSelectionRect() {
    let sel_rect = null;
    limitSelLines();
    const sel_elms = Array.from(document.getElementsByClassName('qlbt-selection-line'));
    if (sel_elms.length == 0) {
        return cleanRect();
    }
    for (var i = 0; i < sel_elms.length; i++) {
        let cur_rect = cleanRect(screenToEditorRect(cleanRect(sel_elms[i].getBoundingClientRect())));
        if (sel_rect == null) {
            sel_rect = cur_rect;
        } else {
            sel_rect = rectUnion(sel_rect, cur_rect);
        }
    }
    sel_rect.x1 = sel_rect.right;
    sel_rect.y1 = sel_rect.bottom;
    return sel_rect;
}

function getTableSelectedCellElements() {
    if (getTableElements().length == 0) {
        return [];
    }
    const table_mod = getBetterTableModule(true);
    if (isNullOrUndefined(table_mod.tableSelection)) {
        return [];
    }
    return table_mod.tableSelection.selectedTds.map((x) => {
        return x.domNode;
    });
}
function getTableSelectedCells() {
    if (getTableElements().length == 0) {
        return [];
    }
    const table_mod = getBetterTableModule(true);
    if (isNullOrUndefined(table_mod.tableSelection)) {
        return [];
    }
    return table_mod.tableSelection.selectedTds.map((x) => {
        return {
            row: x.rowOffset(),
            col: x.cellOffset()
        }
    });
}

function getTableContextMenuElement() {
    let ops_menu_elms = document.getElementsByClassName(globals.TABLE_OPS_MENU_CLASS_NAME);
    if (ops_menu_elms == null || ops_menu_elms.length == 0) {
        return null;
    }
    return ops_menu_elms[0];
}

function getTableSelectionLineElements() {
    return Array.from(document.getElementsByClassName('qlbt-selection-line'));
}

function getTableColumnEditorContainerElement() {
    const tcec_elms = Array.from(document.getElementsByClassName(globals.TABLE_COL_TOOLS_CLASS_NAME));
    if (tcec_elms.length == 0) {
        return null;
    }
    return tcec_elms[0];
}

function getTableElementRect(table_elm) {
    // normally returns block width w/o using inner tbody element
    if (!table_elm) {
        return cleanRect();
    }

    let tbody_elm = table_elm.querySelector('tbody');
    if (!tbody_elm) {
        return cleanRect();
    }

    return cleanRect(tbody_elm.getBoundingClientRect());
}

function getTablesCsv(format, csvProps, selectionOnly = false) {
    // NOTE there should only be 1 table typically but keeping all table stuff scaled for N
    // get list of all tables csv
    let table_csvl = getTableElements().map(x => getTableCsv(x, format, csvProps, selectionOnly));

    // merge csv's w/ row sep
    csvProps = !csvProps ? globals.DefaultCsvProps : csvProps;
    return table_csvl.join(csvProps.RowSeparator);
}

function getTableCsv(table_elm, format, csvProps, selectionOnly = false) {
    if (!table_elm) {
        return '';
    }
    let rows = table_elm.querySelectorAll('tr');
    if (!rows || rows.length == 0) {
        return '';
    }
    format = !format ? 'text' : format.toLowerCase();
    csvProps = !csvProps ? globals.DefaultCsvProps : csvProps;
    let was_enabled = globals.quill.isEnabled();
    if (!was_enabled) {
        globals.quill.enable(true);
        updateQuill();
    }
    let btm = getBetterTableModule();
    if (isNullOrUndefined(btm.tableSelection)) {
        btm = getBetterTableModule(true);
    }
    let cells_to_convert = null;
    if (!isNullOrUndefined(btm.tableSelection) &&
        !isNullOrUndefined(btm.tableSelection.selectedTds) &&
        btm.tableSelection.selectedTds.length > 0) {
        // treat cell selection like text selection, when no cell is selected convert all
        cells_to_convert =
            btm
                .tableSelection
                .selectedTds
                .map(x => {
                    return {
                        r: x.rowOffset(),
                        c: x.cellOffset()
                    };
                });
        if (cells_to_convert.length == table_elm.querySelectorAll('td').length) {
            // all selected, treat as default
            cells_to_convert = null;
        }
    } 
    let csv_output = '';

    for (var r = 0; r < rows.length; r++) {
        let row = rows[r];
        let cells = row.querySelectorAll('td');
        let row_str = '';
        for (var c = 0; c < cells.length; c++) {
            if (cells_to_convert &&
                cells_to_convert.filter(x => x.r == r && x.c == c).length == 0) {
                // ignored cell
                continue;
            }
            if (row_str != '') {
                row_str += csvProps.ColSeparator;
            }
            let cell = cells[c];
            if (isHtmlFormat(format)) {
                row_str += cell.innerHTML;
            } else if (isPlainTextFormat(format) || isCsvFormat(format)) {
                let cell_range = getElementDocRange(cell);

                let cell_text = getText(cell_range, selectionOnly).trim(); // remove new line ending
                row_str += cell_text;
            }
        }
        if (row_str != '') {
            row_str += csvProps.RowSeparator;
        }
        
        csv_output += row_str;
    }
    if (!was_enabled) {
        globals.quill.enable(false);
    }
    return csv_output;
}

function getTableElements() {
    return Array.from(document.getElementsByClassName(globals.TABLE_WRAPPER_CLASS_NAME));
}

function getBetterTableModule(forceInit = false) {    
    let btm = globals.quill.getModule('better-table');
    if (btm &&
        forceInit &&
        getTableElements().length > 0 &&
        isSubSelectionEnabled() &&
        (isNullOrUndefined(btm.tableSelection) || 
            isNullOrUndefined(btm.columnTool) ||
            isNullOrUndefined(btm.getTable()))) {
        // force init through by quickly show/hiding ops
        let sel_cell_elm = getTableCellElementAtDocIdx(getDocSelection().index);
        if (sel_cell_elm) {
            let ops_elm = getBetterTableModule().getOpsMenu(sel_cell_elm);
            ops_elm.tableOperationMenu.domNode.remove();            
        }        
    }
    return btm;
}

function getTableObject(range) {
    // table is [TableContainer, TableRow, TableCell, offset]
    let btm = getBetterTableModule();
    if (!btm) {
        return null;
    }
    return btm.getTable(range);
}

function getTableCellAtDocIdx(doc_idx) {
    const table_elm = getTableElementAtDocIdx(doc_idx);
    if (table_elm == null) {
        // probably should avoid having this occur
        return null;
    }
    const cell_elm = getTableCellElementAtDocIdx(doc_idx);
    if (cell_elm == null) {
        return null;
    }
    let row_idx = Array.from(table_elm.rows).indexOf(cell_elm.parentNode);
    let col_idx = 0;
    let prev_sib = cell_elm.previousSibling;
    while (prev_sib != null) {
        col_idx++;
        prev_sib = prev_sib.previousSibling;
    }
    return [row_idx, col_idx];
}

function getTableElementAtDocIdx(doc_idx) {
    let cur_elm = getElementAtDocIdx(doc_idx, false, false);
    if (!cur_elm) {
        return null;
    }
    cur_elm = cur_elm.parentNode;
    while (true) {
        if (cur_elm == getEditorElement() ||
            cur_elm == null) {
            return null;
        }
        if (cur_elm.tagName == 'TABLE') {
            return cur_elm;
        }
        cur_elm = cur_elm.parentNode;
    }
    return null;
}

function getTableCellElementAtDocIdx(doc_idx) {
    if (!isDocIdxInTable(doc_idx)) {
        // probably should avoid having this occur
        //debugger;
        return null;
    }
    let max_len = getDocLength();
    let cur_elm = getElementAtDocIdx(doc_idx, false, false);
    let adj_doc_idx = doc_idx;
    if (cur_elm == null) {
        return null;
    }
    if (cur_elm.tagName === undefined) {
        cur_elm = cur_elm.parentNode;
    }
    while (true) {
        if (!isChildOfTagName(cur_elm, 'col')) {
            // out of column group
            break;
        }
        // iterate past colgroups in file list to td content
        adj_doc_idx++;
        if (adj_doc_idx == max_len) {
            return null;
        }
        cur_elm = getElementAtDocIdx(adj_doc_idx, false, false);
        if (!isClassInElementPath(cur_elm, globals.TABLE_WRAPPER_CLASS_NAME)) {
            return null;
        }
    }

    if (cur_elm == null) {
        return null;
    }
    while (cur_elm.tagName != 'TD') {
        cur_elm = cur_elm.parentNode;

        if (cur_elm == null) {
            return null;
        }
    }
    return cur_elm;
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isTableInDocument() {
    return getTableElements().length > 0;
}

function isContentATable() {
    if (globals.ContentItemType != 'Text') {
        return false;
    }
    return Array.from(getEditorElement().getElementsByClassName(globals.TABLE_WRAPPER_CLASS_NAME)).length == 1 &&
        getEditorElement().children.length == 1;
}

function isDocIdxInTable(docIdx) {
    let doc_idx_elm = getElementAtDocIdx(docIdx,false,false);
    if (!doc_idx_elm) {
        return false;
    }
    return isClassInElementPath(doc_idx_elm, globals.TABLE_WRAPPER_CLASS_NAME);
}

function isAnyTableSelectionElementsVisible() {
    return getTableSelectionLineElements().length > 0;
}

function isScreenPointInAnyTable(client_mp) {
    if (!isTableInDocument()) {
        return false;
    }
    let table_elms = getTableElements();
    for (var i = 0; i < table_elms.length; i++) {
        let table_elm = table_elms[i];
        let table_elm_rect = getTableElementRect(table_elm);
        if (isPointInRect(table_elm_rect, client_mp)) {
            return true;
        }
    }
    return false;
}

function isContextMenuEventGoingToShowTableMenu(e) {
    if (!isTableInDocument() || !globals.IsTableOpsMenuEnabled) {
        return false;
    }
    if (e.button == 2 && isAnyTableSelectionElementsVisible()) {
        return true;
    }
    return false;
}

function canContextMenuEventShowTableOpsMenu() {
    return !isReadOnly() && isAnyTableSelectionElementsVisible();
}
function isTableContextMenuVisible() {
    return getTableContextMenuElement() != null &&
        !getTableContextMenuElement().classList.contains('hidden');
}

function hasEditableTable() {
    if (globals.ContentItemType != 'Text' ||
        getTableElements().length == 0) {
        return false;
    } 
    return true;
}
// #endregion State

// #region Actions

function updateTableDragState(e) {
    // used to know if pointer down is already on a selected cell, in which case will allow for a drag event
    if (e == null) {
        // mouse up
        globals.IsTableDragSelecting = false;
        return;
    }
    if (globals.WindowMouseDownLoc == null || !hasEditableTable()) {
        globals.IsTableDragSelecting = false;
        return true;
    }
    if (globals.IsTableDragSelecting != false) {
        // only null during confirmed cell drag
        return true;
    }

    let cell_elm_under_pointer = null;
    let sel_cell_elms = getTableSelectedCellElements();
    for (var i = 0; i < sel_cell_elms.length; i++) {
        let cell_rect = cleanRect(sel_cell_elms[i].getBoundingClientRect());
        if (isPointInRect(cell_rect, globals.WindowMouseDownLoc)) {
            cell_elm_under_pointer = sel_cell_elms[i];
            break;
        }
    }
    if (cell_elm_under_pointer == null) {
        // clean mouse down, reject dragStart, perform drag select
        globals.IsTableDragSelecting = true;
    } else {
        // down over selection, allow drag
        globals.IsTableDragSelecting = null;
    }
    log('Table Drag Selecting: ' + (globals.IsTableDragSelecting == true ? "YUP" : "NOPE"));
    if (globals.IsTableDragSelecting == null) {
        if (e) {
            //e.preventDefault();
            //e.stopPropagation();
        }
        
        return false;
    }
    return true;
}
function clearTableSelectionStates() {
    const table_mod = getBetterTableModule();
    if (!table_mod) {
        return;
    }
    //log('clearing table selection states...');
    if (!isNullOrUndefined(table_mod.tableSelection)) {
        table_mod.tableSelection.clearSelectionHandler();
    }
    if (!isNullOrUndefined(table_mod.columnTool) &&
        !isNullOrUndefined(table_mod.columnTool.domNode)) {
        getBetterTableModule().columnTool.domNode.remove();
    }
    
}

function updateTablesSizesAndPositions() {
    if (!hasEditableTable()) {
        return;
    }
    const table_mod = getBetterTableModule(true);
    if (!table_mod) {
        return;
    }
    //log('clearing table selection states...');
    if (!isNullOrUndefined(table_mod.tableSelection)) {
        table_mod.tableSelection.refreshHelpLinesPosition();
    }
    limitColTool();
    if (!isNullOrUndefined(table_mod.columnTool)) {
        table_mod.columnTool.updateToolCells();
    }
    limitSelLines();
}

function hideTableScrollbars() {
    let table_elms = getTableElements();
    for (var i = 0; i < table_elms.length; i++) {
        let table_elm = table_elms[i];
        hideElementScrollbars(table_elm);
	}
}

function showTableScrollbars() {
    let table_elms = getTableElements();
    for (var i = 0; i < table_elms.length; i++) {
        let table_elm = table_elms[i];
        showElementScrollbars(table_elm);
	}
}


function rejectTableContextMenu(e) {
    // returns TRUE to prevent showing ops menu but only if it would be shown otherwise

    if (!isTableInDocument() ||
        isNullOrUndefined(e.button) ||
        e.button != 2) {
        return false;
    }

    let is_click_in_cell = isScreenPointInAnyTable(getClientMousePos(e)); //isClassInElementPath(e.target, globals.TABLE_WRAPPER_CLASS_NAME);
    let is_cell_focus = isAnyTableSelectionElementsVisible();

    if (globals.IsTableOpsMenuEnabled) {
        if (!is_click_in_cell && is_cell_focus && globals.quill.hasFocus()) {
            // BUG prevent better table bug where cell element is null (quill-better-table.js:2942 )
            // mentioned here https://github.com/soccerloway/quill-better-table/issues/77#issue-999274656
            return true;
        } else {
            return !canContextMenuEventShowTableOpsMenu();
        }
    } else {
        if (is_click_in_cell) {
            // prevent show table context menu
            return true;
        }
    }
    
    //if (is_click_in_cell &&
    //    !isClassInElementPath(e.target, 'file-list-path') &&
    //    !globals.IsTableInteractionEnabled) {
    //    // disable any table clicks for file list
    //    return true;
    //}
    return false;
}

function updateTableOpsMenuSizeAndPosition() {
    delay(500)
        .then(() => {
            // wait for ops menu to show up

            let menu_elm = getTableContextMenuElement();
            if (!menu_elm) {
                return;
            }
            const window_rect = getWindowRect();
            const menu_rect = cleanRect(menu_elm.getBoundingClientRect());
            let mh = menu_rect.height;
            if (menu_rect.bottom > window_rect.bottom) {
                mh = window_rect.bottom - menu_rect.top;
            }
            menu_elm.style.height = `${mh}px`;
        });
}

function disableTableContextMenu() {
    globals.IsTableOpsMenuEnabled = false;
}

function enableTableContextMenu() {
    if (globals.ALLOW_TABLE_OPS_MENU) {
        globals.IsTableOpsMenuEnabled = true;
    }
}

function disableTableInteraction() {
    globals.IsTableInteractionEnabled = false;
}

function enableTableInteraction() {
    globals.IsTableInteractionEnabled = true;
}

function limitColTool() {
    if (Array.from(document.getElementsByClassName('qlbt-col-tool')).length > 1) {
        // fix when 2nd col tools comes, just remove 2nd
        Array.from(document.getElementsByClassName('qlbt-col-tool'))[0].remove();
    }
}

function limitSelLines() {
    let sel_elms = Array.from(document.getElementsByClassName('qlbt-selection-line'));
    if (sel_elms.length > 4) {
        for (var i = 0; i < 4; i++) {
            sel_elms[i].remove();
		}
    }
}

function fixTableHistory(delta, oldDelta) {
    // BUG when table create is undone the column editor
    // isn't removed and the event handlers are broken

    // NOTE 
    if (!isTableInDocument() && getTableColumnEditorContainerElement() != null) {
        getBetterTableModule().hideTableTools();
    }
}

// #endregion Actions

// #region Event Handlers
// #endregion Event Handlers