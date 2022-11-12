// #region Globals

const TABLE_WRAPPER_CLASS_NAME = 'quill-better-table-wrapper';

var IsBetterTableOpsMenuEnabled = true;
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
        quill.update();
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
    return document.getElementsByClassName(TABLE_WRAPPER_CLASS_NAME);
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

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

// #endregion State

// #region Actions

function rejectTableMouseEvent(e) {
    if (isClassInElementPath(e.target, TABLE_WRAPPER_CLASS_NAME) &&
        !IsBetterTableOpsMenuEnabled &&
        e.button == 2) {
        e.preventDefault();
        e.stopPropagation();
        return true;
    }
    if (isClassInElementPath(e.target, TABLE_WRAPPER_CLASS_NAME) &&
        !isClassInElementPath(e.target,'file-list-path') &&
        !IsBetterTableInteractionEnabled) {
        e.preventDefault();
        e.stopPropagation();
        return true;
    }
    return false;
}

function disableTableContextMenu() {
    IsBetterTableOpsMenuEnabled = false;
}

function enableTableContextMenu() {
    IsBetterTableOpsMenuEnabled = true;
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