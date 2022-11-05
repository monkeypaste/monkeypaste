// #region Globals

var DefaultCsvProps = {
    ColSeparator: ',',
    RowSeparator: '\n'
};

const MAX_TABLE_ROWS = 7;
const MAX_TABLE_COLS = 7;

// #endregion Globals

// #region Life Cycle

function initTable() {
    initTableToolbarButton();
    DefaultCsvProps.RowSeparator = envNewLine();
}

function initTableToolbarButton() {
    if (!UseBetterTable) {
        return;
    }
    //const addTableToolbarButton = new QuillToolbarButton({
    //    icon: getSvgHtml('addtable')
    //});

    //addTableToolbarButton.qlFormatsEl.addEventListener('click', onAddTableToolbarButtonClick);
    //addTableToolbarButton.attach(quill);

    let addTableButton_elm = getAddTableToolbarButtonElement();
    addClickOrKeyClickEventListener(addTableButton_elm, onAddTableToolbarButtonClick);

    getAddTableToolbarLabelElement().innerHTML = getSvgHtml('addtable');
}
// #endregion Life Cycle

// #region Getters

function getAddTableToolbarButtonElement() {
    return document.getElementById('addTableToolbarButton');
}

function getAddTableToolbarLabelElement() {
    let ttb_elm = getAddTableToolbarButtonElement();
    return ttb_elm.getElementsByClassName('ql-picker-label')[0];
}

function getAddTableToolbarOptionsElement() {
    let ttb_elm = getAddTableToolbarButtonElement();
    if (!ttb_elm) {
        return null;
    }
    return ttb_elm.getElementsByClassName('ql-picker-options')[0];
}

function getAddTableOptionDataValues() {
    var tableOptions = [];

    for (let r = 1; r <= MAX_TABLE_ROWS; r++) {
        for (let c = 1; c <= MAX_TABLE_COLS; c++) {
            tableOptions.push('newtable_' + r + '_' + c);
        }
    }
    return tableOptions;
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
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isAddTableValid() {
    return true;
}

function isContentATable() {
    if (ContentItemType != 'Text') {
        return false;
    }
    return Array.from(document.getElementsByClassName('quill-better-table-wrapper')).length == 1;
}

// #endregion State

// #region Actions

function hideAddTableContextMenu() {
    getAddTableToolbarButtonElement().classList.remove('ql-expanded');
    getAddTableToolbarOptionsElement().classList.add('hidden');
}

function showAddTableContextMenu() {
    getAddTableToolbarButtonElement().classList.add('ql-expanded');

    let addTableOptions_elm = getAddTableToolbarOptionsElement();
    getAddTableToolbarOptionsElement().classList.remove('hidden');
    
    addTableOptions_elm.innerHTML = '';

    let table_options = getAddTableOptionDataValues();
    for (var i = 0; i < table_options.length; i++) {
        //<span tabindex="0" role="button" class="ql-picker-item" data-value="newtable_1_1"></span>
        let opt_span = document.createElement('SPAN');
        opt_span.tabIndex = i;
        opt_span.role = 'button';
        opt_span.classList.add('ql-picker-item');
        opt_span.classList.add('add-table-item');
        opt_span.setAttribute('data-value', table_options[i]);
        addTableOptions_elm.appendChild(opt_span);

        opt_span.addEventListener('mouseover', onAddTableOptionMouseOver);
        addClickOrKeyClickEventListener(opt_span, onAddTableOptionClick);
    }
}
// #endregion Actions

// #region Event Handlers

function onAddTableOptionMouseOver(e) {
    let data_val = e.currentTarget.getAttribute('data-value');
    let over_row = parseInt_safe(data_val.split('_')[1]);
    let over_col = parseInt_safe(data_val.split('_')[2]);

    for (let r = 1; r <= MAX_TABLE_ROWS; r++) {
        for (let c = 1; c <= MAX_TABLE_COLS; c++) {
            let dataVal = `newtable_${r}_${c}`;
            let opt_elm = Array.from(getAddTableToolbarOptionsElement().children).find(x => x.getAttribute('data-value') == dataVal);

            let is_included = r <= over_row && c <= over_col;
            if (is_included) {
                opt_elm.classList.add('add-table-item-included');
            } else {
                opt_elm.classList.remove('add-table-item-included');
			}
        }
    }
}

function onAddTableOptionClick(e) {
    let data_val = e.currentTarget.getAttribute('data-value');
    let row_count = parseInt_safe(data_val.split('_')[1]);
    let col_count = parseInt_safe(data_val.split('_')[2]);

    let better_table_mod = quill.getModule('better-table');
    better_table_mod.insertTable(row_count, col_count);

    hideAddTableContextMenu();
}

function onAddTableToolbarButtonClick(e) {
    if (!isAddTableValid()) {
        return;
    }

    showAddTableContextMenu();
}
// #endregion Event Handlers