// #region Globals

const TABLE_WRAPPER_CLASS_NAME = 'quill-better-table-wrapper';

var IsBetterTableOpsMenuEnabled = false;
var IsBetterTableInteractionEnabled = true;

var DefaultCsvProps = {
    ColSeparator: ',',
    RowSeparator: '\n'
};


// #endregion Globals

// #region Life Cycle

function initTable() {
    initTableToolbarButton();
    DefaultCsvProps.RowSeparator = envNewLine();

    //let editor_elm = getEditorElement();
    //if (editor_elm == null) {
    //    debugger;
    //}
    //editor_elm.addEventListener("contextmenu", onWindowMouseDown, true);
}

// #endregion Life Cycle

// #region Getters

function getBetterTableContextMenuElement() {
    let ops_menu_elms = document.getElementsByClassName('qlbt-operation-menu');
    if (ops_menu_elms == null || ops_menu_elms.length == 0) {
        return null;
    }
    return ops_menu_elms[0];
}

function getBetterTableSelectionLineElements() {
    return Array.from(document.getElementsByClassName('qlbt-selection-line'));
}

function getTableCsv(format, csvProps, encodeTemplates = false) {

    let table = document.getElementsByClassName('quill-better-table')[0];
    if (!table) {
        return '';
    }
    let rows = table.querySelectorAll('tr');
    if (!rows || rows.length == 0) {
        return '';
    }
    format = !format ? 'HTML Format' : format;
    csvProps = !csvProps ? DefaultCsvProps : csvProps;
    let was_enabled = quill.isEnabled();
    if (!was_enabled) {
        quill.enable(true);
        updateQuill();
    }
    let csv_output = '';

    for (var i = 0; i < rows.length; i++) {
        let row = rows[i];
        let cells = row.querySelectorAll('td');
        let row_str = '';
        for (var j = 0; j < cells.length; j++) {
            if (j > 0) {
                row_str += csvProps.ColSeparator;
            }
            let cell = cells[j];
            if (format == 'HTML Format') {
                row_str += cell.innerHTML;
            } else if (format == 'Text') {
                let cell_range = getElementDocRange(cell);

                let cell_text = getText(cell_range, encodeTemplates).trim(); // remove new line ending
                row_str += cell_text;
            }
        }
        row_str += csvProps.RowSeparator;
        csv_output += row_str;
    }
    if (!was_enabled) {
        quill.enable(false);
    }
    return csv_output;
}

function getBetterTableElements() {
    return Array.from(document.getElementsByClassName(TABLE_WRAPPER_CLASS_NAME));
}

function getBetterTableModule() {    
    let betterTableModule = quill.getModule('better-table');
    return betterTableModule;
}

function getTableObject(range) {
    // table is [TableContainer, TableRow, TableCell, offset]
    let btm = getBetterTableModule();
    if (!btm) {
        return null;
    }
    return btm.getTable(range);
}

function getBetterTableElementRect(table_elm) {
    // normally returns block width w/o using inner tbody element
    if (!table_elm) {
        return cleanRect();
    }
    let tbody_elm = table_elm.firstChild.firstChild.nextSibling;

    return cleanRect(tbody_elm.getBoundingClientRect());
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isTableInDocument() {
    return getBetterTableElements().length > 0;
}

function isContentATable() {
    if (ContentItemType != 'Text') {
        return false;
    }
    return Array.from(document.getElementsByClassName(TABLE_WRAPPER_CLASS_NAME)).length == 1;
}

function isDocIdxInTable(docIdx) {
    let doc_idx_elm = getElementAtDocIdx(docIdx);
    if (!doc_idx_elm) {
        return false;
    }
    return isClassInElementPath(doc_idx_elm, TABLE_WRAPPER_CLASS_NAME);
}

function isAnyTableSelectionElementsVisible() {
    return getBetterTableSelectionLineElements().length > 0;
}

function isScreenPointInAnyTable(client_mp) {
    if (!isTableInDocument()) {
        return false;
    }
    let table_elms = getBetterTableElements();
    for (var i = 0; i < table_elms.length; i++) {
        let table_elm = table_elms[i];
        let table_elm_rect = getBetterTableElementRect(table_elm);
        if (isPointInRect(table_elm_rect, client_mp)) {
            return true;
        }
    }
    return false;
}

function isContextMenuEventGoingToShowTableMenu(e) {
    if (!isTableInDocument() || !IsBetterTableOpsMenuEnabled) {
        return false;
    }
    if (e.button == 2 && isAnyTableSelectionElementsVisible()) {
        return true;
    }
    return false;
}

function isBetterTableContextMenuVisible() {
    return getBetterTableContextMenuElement() != null &&
        !getBetterTableContextMenuElement().classList.contains('hidden');
}
// #endregion State

// #region Actions

function hideTableScrollbars() {
    let table_elms = getBetterTableElements();
    for (var i = 0; i < table_elms.length; i++) {
        let table_elm = table_elms[i];
        hideElementScrollbars(table_elm);
	}
}

function showTableScrollbars() {
    let table_elms = getBetterTableElements();
    for (var i = 0; i < table_elms.length; i++) {
        let table_elm = table_elms[i];
        showElementScrollbars(table_elm);
	}
}

function rejectTableMouseEvent(e) {
    if (!isTableInDocument()) {
        return false;
    }

    let is_click_in_cell = isScreenPointInAnyTable(getClientMousePos(e)); //isClassInElementPath(e.target, TABLE_WRAPPER_CLASS_NAME);
    let is_cell_focus = isAnyTableSelectionElementsVisible();

    if (e.button == 2) {
        if (IsBetterTableOpsMenuEnabled) {
            if (!is_click_in_cell && is_cell_focus) {
                // BUG prevent better table bug where cell element is null (quill-better-table.js:2942 )
                // mentioned here https://github.com/soccerloway/quill-better-table/issues/77#issue-999274656
                return true;
            }
        } else {
            if (is_click_in_cell) {
                // prevent show table context menu
                return true;
            }
        }
    }
    
    if (is_click_in_cell &&
        !isClassInElementPath(e.target,'file-list-path') &&
        !IsBetterTableInteractionEnabled) {
        // disable any table clicks for file list
        return true;
    }
    return false;
}

function disableTableContextMenu() {
    IsBetterTableOpsMenuEnabled = false;
}

function enableTableContextMenu() {
    //IsBetterTableOpsMenuEnabled = true;
}

function disableTableInteraction() {
    IsBetterTableInteractionEnabled = false;
}

function enableTableInteraction() {
    IsBetterTableInteractionEnabled = true;
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers