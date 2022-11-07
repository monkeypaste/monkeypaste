// #region Globals

const MAX_TABLE_ROWS = 7;
const MAX_TABLE_COLS = 7;

// #endregion Globals

// #region Life Cycle

function initTableToolbarButton() {
    if (!UseBetterTable) {
        return;
    }

    let createTableButton_elm = getCreateTableToolbarButtonElement();
    addClickOrKeyClickEventListener(createTableButton_elm, onCreateTableToolbarButtonClick);

    getCreateTableToolbarLabelElement().innerHTML = getSvgHtml('createtable');
}

function showCreateTableContextMenu() {
    getCreateTableToolbarButtonElement().classList.add('ql-expanded');

    let addTableOptions_elm = getCreateTableToolbarOptionsElement();
    getCreateTableToolbarOptionsElement().classList.remove('hidden');

    addTableOptions_elm.innerHTML = '';

    let table_options = getCreateTableOptionDataValues();
    for (var i = 0; i < table_options.length; i++) {
        //<span tabindex="0" role="button" class="ql-picker-item" data-value="newtable_1_1"></span>
        let opt_span = document.createElement('SPAN');
        opt_span.tabIndex = i;
        opt_span.role = 'button';
        opt_span.classList.add('ql-picker-item');
        opt_span.classList.add('create-table-item');
        opt_span.setAttribute('data-value', table_options[i]);
        addTableOptions_elm.appendChild(opt_span);

        opt_span.addEventListener('mouseover', onCreateTableOptionMouseOver);
        addClickOrKeyClickEventListener(opt_span, onCreateTableOptionClick);
    }

    window.addEventListener('click', onWindowClickWithCreateTableOpen);
    window.addEventListener('keydown', onWindowKeyDownWithCreateTableOpen, true);

    setCreateTableSelection(1, 1);
}

function hideCreateTableContextMenu() {
    getCreateTableToolbarButtonElement().classList.remove('ql-expanded');
    getCreateTableToolbarOptionsElement().classList.add('hidden');

    window.removeEventListener('click', onWindowClickWithCreateTableOpen);
    window.removeEventListener('keydown', onWindowKeyDownWithCreateTableOpen, true);
}
// #endregion Life Cycle

// #region Getters

function getCreateTableToolbarButtonElement() {
    return document.getElementById('createTableToolbarButton');
}

function getCreateTableToolbarLabelElement() {
    let ttb_elm = getCreateTableToolbarButtonElement();
    return ttb_elm.getElementsByClassName('ql-picker-label')[0];
}

function getCreateTableToolbarOptionsElement() {
    let ttb_elm = getCreateTableToolbarButtonElement();
    if (!ttb_elm) {
        return null;
    }
    return ttb_elm.getElementsByClassName('ql-picker-options')[0];
}

function getCreateTableOptionDataValues() {
    var tableOptions = [];

    for (let r = 1; r <= MAX_TABLE_ROWS; r++) {
        for (let c = 1; c <= MAX_TABLE_COLS; c++) {
            tableOptions.push('newtable_' + r + '_' + c);
        }
    }
    return tableOptions;
}

function getCreateTableDimensionsFromElement(elm) {
    let data_val = elm.getAttribute('data-value');
    let row_count = parseInt_safe(data_val.split('_')[1]);
    let col_count = parseInt_safe(data_val.split('_')[2]);

    return {
        rows: row_count,
        cols: col_count
    };
}

function getSelectedCreateTableDimensions() {
    let included_dims = { rows: 0, cols: 0 };
    for (let r = 1; r <= MAX_TABLE_ROWS; r++) {
        for (let c = 1; c <= MAX_TABLE_COLS; c++) {
            let dataVal = `newtable_${r}_${c}`;
            let opt_elm = Array.from(getCreateTableToolbarOptionsElement().children).find(x => x.getAttribute('data-value') == dataVal);

            let is_included = opt_elm.classList.contains('create-table-item-included');
            if (is_included) {
                if (r > included_dims.rows) {
                    included_dims.rows = r;
                }
                if (c > included_dims.cols) {
                    included_dims.rows = c;
                }
            } 
        }
    }
    return included_dims;
}

// #endregion Getters

// #region Setters

function setCreateTableSelection(row_count, col_count) {
    for (let r = 1; r <= MAX_TABLE_ROWS; r++) {
        for (let c = 1; c <= MAX_TABLE_COLS; c++) {
            let dataVal = `newtable_${r}_${c}`;
            let opt_elm = Array.from(getCreateTableToolbarOptionsElement().children).find(x => x.getAttribute('data-value') == dataVal);

            let is_included = r <= row_count && c <= col_count;
            if (is_included) {
                opt_elm.classList.add('create-table-item-included');
            } else {
                opt_elm.classList.remove('create-table-item-included');
            }
        }
    }
}

// #endregion Setters

// #region State

function isCreateTableValid() {
    let sel = getDocSelection();
    if (!sel) {
        return false;
    }
    if (isDocIdxInTable(sel.index) ||
        isDocIdxInTable(sel.index + sel.length)) {
        return false;
    }
    if (isDocIdxInListItem(sel.index) ||
        isDocIdxInListItem(sel.index + sel.length)) {
        return false;
    }
    return true;
}

function isCreateTableContextMenuVisible() {
    return !getCreateTableToolbarOptionsElement().classList.contains('hidden');
}
// #endregion State

// #region Actions

function updateCreateTableToolbarButtonIsEnabled() {
    if (isCreateTableValid()) {
        getCreateTableToolbarButtonElement().classList.remove('disabled');
    } else {
        getCreateTableToolbarButtonElement().classList.add('disabled');
	}
}

function createTableAtDocSelection(rows, cols) {
    let better_table_mod = quill.getModule('better-table');
    better_table_mod.insertTable(rows,cols);
}
// #endregion Actions

// #region Event Handlers

function onCreateTableOptionMouseOver(e) {
    let opt_dims = getCreateTableDimensionsFromElement(e.currentTarget);
    setCreateTableSelection(opt_dims.rows,opt_dims.cols);
}

function onCreateTableOptionClick(e) {
    let opt_dims = getCreateTableDimensionsFromElement(e.currentTarget);
    createTableAtDocSelection(opt_dims.rows, opt_dims.cols);    

    hideCreateTableContextMenu();
}

function onCreateTableToolbarButtonClick(e) {
    
    if (isCreateTableContextMenuVisible()) {
        hideCreateTableContextMenu();
    } else {
        showCreateTableContextMenu();
	}

    

    // prevent onWindowClickWithCreateTableOpen from auto firing during show
    e.stopPropagation();
}

function onWindowClickWithCreateTableOpen(e) {
    if (isChildOfElement(e.target, getCreateTableToolbarOptionsElement())) {

    }
    hideCreateTableContextMenu();
}

function onWindowKeyDownWithCreateTableOpen(e) {
    if (e.key == 'Escape' || !isCreateTableContextMenuVisible()) {
        hideCreateTableContextMenu();
        return;
    }
    if (e.key == 'Enter' || e.key == ' ') {
        let sel_dims = getSelectedCreateTableDimensions();
        createTableAtDocSelection(sel_dims.rows, sel_dims.cols);
        hideCreateTableContextMenu();
        return;
    }
}

// #endregion Event Handlers